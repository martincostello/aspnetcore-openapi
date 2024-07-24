// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TodoApp.Models;

namespace TodoApp;

[Collection(TodoAppCollection.Name)]
public class ApiTests
{
    public ApiTests(TodoAppFixture fixture, ITestOutputHelper outputHelper)
    {
        Fixture = fixture;

        // Route output from the fixture's logs to xunit's output
        OutputHelper = outputHelper;
        Fixture.SetOutputHelper(OutputHelper);
    }

    private TodoAppFixture Fixture { get; }

    private ITestOutputHelper OutputHelper { get; }

    [Fact]
    public async Task Can_Manage_Todo_Items_With_Api()
    {
        // Arrange
        using var client = Fixture.CreateDefaultClient();

        // Act - Get all the items
        var items = await client.GetFromJsonAsync<TodoListViewModel>("/api/items");

        // Assert - There should be no items
        Assert.NotNull(items);
        Assert.NotNull(items.Items);

        var beforeCount = items.Items.Count;

        // Arrange
        var text = "Buy eggs";
        var newItem = new CreateTodoItemModel { Text = text };

        // Act - Add a new item
        using var createdResponse = await client.PostAsJsonAsync("/api/items", newItem);

        // Assert - An item was created
        Assert.Equal(HttpStatusCode.Created, createdResponse.StatusCode);
        Assert.NotNull(createdResponse.Headers.Location);

        using var createdJson = await createdResponse.Content.ReadFromJsonAsync<JsonDocument>();

        // Arrange - Get the new item's URL and Id
        var itemUri = createdResponse.Headers.Location;
        var itemId = createdJson!.RootElement.GetProperty("id").GetString();

        // Act - Get the item
        var item = await client.GetFromJsonAsync<TodoItemModel>(itemUri);

        // Assert - Verify the item was created correctly
        Assert.NotNull(item);
        Assert.Equal(itemId, item.Id);
        Assert.False(item.IsCompleted);
        Assert.NotNull(item.LastUpdated);
        Assert.Equal(text, item.Text);

        // Act - Mark the item as being completed
        using var completedResponse = await client.PostAsJsonAsync(itemUri + "/complete", new { });

        // Assert - The item was completed
        Assert.Equal(HttpStatusCode.NoContent, completedResponse.StatusCode);

        item = await client.GetFromJsonAsync<TodoItemModel>(itemUri);

        Assert.NotNull(item);
        Assert.Equal(itemId, item.Id);
        Assert.Equal(text, item.Text);
        Assert.True(item.IsCompleted);

        // Act - Get all the items
        items = await client.GetFromJsonAsync<TodoListViewModel>("/api/items");

        // Assert - The item was completed
        Assert.NotNull(items);
        Assert.NotNull(items.Items);
        Assert.Equal(beforeCount + 1, items.Items.Count);
        item = items.Items.Last();

        Assert.NotNull(item);
        Assert.Equal(itemId, item.Id);
        Assert.Equal(text, item.Text);
        Assert.True(item.IsCompleted);
        Assert.NotNull(item.LastUpdated);

        // Act - Delete the item
        using var deletedResponse = await client.DeleteAsync(itemUri);

        // Assert - The item no longer exists
        Assert.Equal(HttpStatusCode.NoContent, deletedResponse.StatusCode);

        items = await client.GetFromJsonAsync<TodoListViewModel>("/api/items");

        Assert.NotNull(items);
        Assert.NotNull(items.Items);
        Assert.Equal(beforeCount, items.Items.Count);
        Assert.DoesNotContain(items.Items, x => x.Id == itemId);

        // Act
        using var getResponse = await client.GetAsync(itemUri);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);

        var problem = await getResponse.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problem);
        Assert.Equal(StatusCodes.Status404NotFound, problem.Status);
        Assert.Equal("Not Found", problem.Title);
        Assert.Equal("Item not found.", problem.Detail);
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.5", problem.Type);
        Assert.Null(problem.Instance);
    }

    [Fact]
    public async Task Cannot_Create_Todo_Item_With_No_Text()
    {
        // Arrange
        using var client = Fixture.CreateDefaultClient();
        var item = new CreateTodoItemModel { Text = string.Empty };

        // Act
        var response = await client.PostAsJsonAsync("/api/items", item);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problem);
        Assert.Equal(StatusCodes.Status400BadRequest, problem.Status);
        Assert.Equal("Bad Request", problem.Title);
        Assert.Equal("No item text specified.", problem.Detail);
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.1", problem.Type);
        Assert.Null(problem.Instance);
    }

    [Fact]
    public async Task Cannot_Complete_Todo_Item_Multiple_Times()
    {
        // Arrange
        using var client = Fixture.CreateDefaultClient();
        var item = new CreateTodoItemModel { Text = "Something" };

        using var createdResponse = await client.PostAsJsonAsync("/api/items", item);

        Assert.Equal(HttpStatusCode.Created, createdResponse.StatusCode);
        Assert.NotNull(createdResponse.Headers.Location);

        var itemUri = createdResponse.Headers.Location;

        using var completedResponse = await client.PostAsJsonAsync(itemUri + "/complete", new { });

        Assert.Equal(HttpStatusCode.NoContent, completedResponse.StatusCode);

        // Act
        using var response = await client.PostAsJsonAsync(itemUri + "/complete", new { });

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problem);
        Assert.Equal(StatusCodes.Status400BadRequest, problem.Status);
        Assert.Equal("Bad Request", problem.Title);
        Assert.Equal("Item already completed.", problem.Detail);
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.1", problem.Type);
        Assert.Null(problem.Instance);
    }

    [Fact]
    public async Task Cannot_Complete_Deleted_Todo_Item()
    {
        // Arrange
        using var client = Fixture.CreateDefaultClient();
        var item = new CreateTodoItemModel { Text = "Something" };

        using var createdResponse = await client.PostAsJsonAsync("/api/items", item);

        Assert.Equal(HttpStatusCode.Created, createdResponse.StatusCode);
        Assert.NotNull(createdResponse.Headers.Location);

        var itemUri = createdResponse.Headers.Location;

        using var deletedResponse = await client.DeleteAsync(itemUri);

        Assert.Equal(HttpStatusCode.NoContent, deletedResponse.StatusCode);

        // Act
        using var response = await client.PostAsJsonAsync(itemUri + "/complete", new { });

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problem);
        Assert.Equal(StatusCodes.Status404NotFound, problem.Status);
        Assert.Equal("Not Found", problem.Title);
        Assert.Equal("Item not found.", problem.Detail);
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.5", problem.Type);
        Assert.Null(problem.Instance);
    }

    [Fact]
    public async Task Cannot_Delete_Todo_Item_Multiple_Times()
    {
        // Arrange
        using var client = Fixture.CreateDefaultClient();
        var item = new CreateTodoItemModel { Text = "Something" };

        using var createdResponse = await client.PostAsJsonAsync("/api/items", item);

        Assert.Equal(HttpStatusCode.Created, createdResponse.StatusCode);
        Assert.NotNull(createdResponse.Headers.Location);

        var itemUri = createdResponse.Headers.Location;

        using var deletedResponse = await client.DeleteAsync(itemUri);

        Assert.Equal(HttpStatusCode.NoContent, deletedResponse.StatusCode);

        // Act
        using var response = await client.DeleteAsync(itemUri);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problem);
        Assert.Equal(StatusCodes.Status404NotFound, problem.Status);
        Assert.Equal("Not Found", problem.Title);
        Assert.Equal("Item not found.", problem.Detail);
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.5", problem.Type);
        Assert.Null(problem.Instance);
    }
}
