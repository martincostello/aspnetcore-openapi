// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NSwag.Annotations;
using TodoApp.Data;
using TodoApp.Models;
using TodoApp.OpenApi;
using TodoApp.OpenApi.NSwag;
using TodoApp.Services;

namespace TodoApp;

/// <summary>
/// A class containing the HTTP endpoints for the Todo API.
/// </summary>
public static class ApiEndpoints
{
    /// <summary>
    /// Adds the services for the Todo API to the application.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <returns>
    /// A <see cref="IServiceCollection"/> that can be used to further configure the application.
    /// </returns>
    public static IServiceCollection AddTodoApi(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<ITodoRepository, TodoRepository>();
        services.AddScoped<ITodoService, TodoService>();

        services.AddDbContext<TodoContext>((serviceProvider, options) =>
        {
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var dataDirectory = configuration["DataDirectory"];

            if (string.IsNullOrEmpty(dataDirectory) || !Path.IsPathRooted(dataDirectory))
            {
                var environment = serviceProvider.GetRequiredService<IHostEnvironment>();
                dataDirectory = Path.Combine(environment.ContentRootPath, "App_Data");
            }

            // Ensure the configured data directory exists
            if (!Directory.Exists(dataDirectory))
            {
                Directory.CreateDirectory(dataDirectory);
            }

            var databaseFile = Path.Combine(dataDirectory, "TodoApp.db");

            options.UseSqlite("Data Source=" + databaseFile);
        });

        return services;
    }

    /// <summary>
    /// Maps the endpoints for the Todo API.
    /// </summary>
    /// <param name="builder">The <see cref="IEndpointConventionBuilder"/>.</param>
    /// <returns>
    /// A <see cref="IEndpointConventionBuilder"/> that can be used to further customize the endpoint.
    /// </returns>
    public static IEndpointRouteBuilder MapTodoApiRoutes(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/api/items")
                           .WithTags("TodoApp")
                           .WithMetadata(new OpenApiExampleAttribute<ProblemDetails, ProblemDetailsExampleProvider>())
                           .WithMetadata(new OpenApiIdExampleAttribute());
        {
            group.MapGet(
                "/",
                [NSwagOpenApiExample<TodoListViewModel>]
                [OpenApiOperation("Get all Todo items", "Gets all of the current user's todo items.")]
                [SwaggerResponse(StatusCodes.Status200OK, typeof(TodoListViewModel), Description = "OK")]
                async (ITodoService service, CancellationToken cancellationToken) => await service.GetListAsync(cancellationToken))
                .WithName("ListTodos")
                .WithSummary("Get all Todo items")
                .WithDescription("Gets all of the current user's todo items.");

            group.MapGet(
                "/{id}",
                [NSwagOpenApiExample<ProblemDetails, ProblemDetailsExampleProvider>]
                [NSwagOpenApiExample<TodoItemModel>]
                [OpenApiOperation("Get a specific Todo item", "Gets the todo item with the specified ID.")]
                [SwaggerResponse(StatusCodes.Status200OK, typeof(TodoItemModel), Description = "OK")]
                [SwaggerResponse(StatusCodes.Status404NotFound, typeof(ProblemDetails), Description = "Not Found")]
                async Task<Results<Ok<TodoItemModel>, ProblemHttpResult>> (
                    Guid id,
                    ITodoService service,
                    CancellationToken cancellationToken) =>
                {
                    var model = await service.GetAsync(id, cancellationToken);
                    return model switch
                    {
                        null => TypedResults.Problem("Item not found.", statusCode: StatusCodes.Status404NotFound),
                        _ => TypedResults.Ok(model),
                    };
                })
                .ProducesProblem(StatusCodes.Status404NotFound)
                .WithName("GetTodo")
                .WithSummary("Get a specific Todo item")
                .WithDescription("Gets the todo item with the specified ID.");

            group.MapPost(
                "/",
                [NSwagOpenApiExample<CreateTodoItemModel>]
                [NSwagOpenApiExample<CreatedTodoItemModel>]
                [NSwagOpenApiExample<ProblemDetails, ProblemDetailsExampleProvider>]
                [OpenApiOperation("Create a new Todo item", "Creates a new todo item for the current user and returns its ID.")]
                [SwaggerResponse(StatusCodes.Status201Created, typeof(CreatedTodoItemModel), Description = "Created")]
                [SwaggerResponse(StatusCodes.Status400BadRequest, typeof(ProblemDetails), Description = "Bad Request")]
                async Task<Results<Created<CreatedTodoItemModel>, ProblemHttpResult>> (
                    CreateTodoItemModel model,
                    ITodoService service,
                    CancellationToken cancellationToken) =>
                {
                    if (string.IsNullOrWhiteSpace(model.Text))
                    {
                        return TypedResults.Problem("No item text specified.", statusCode: StatusCodes.Status400BadRequest);
                    }

                    var id = await service.AddItemAsync(model.Text, cancellationToken);

                    return TypedResults.Created($"/api/items/{id}", new CreatedTodoItemModel() { Id = id });
                })
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .WithName("CreateTodo")
                .WithSummary("Create a new Todo item")
                .WithDescription("Creates a new todo item for the current user and returns its ID.");

            group.MapPost(
                "/{id}/complete",
                [NSwagOpenApiExample<ProblemDetails, ProblemDetailsExampleProvider>]
                [OpenApiOperation("Mark a Todo item as completed", "Marks the todo item with the specified ID as complete.")]
                [SwaggerResponse(StatusCodes.Status204NoContent, typeof(void), Description = "No Content")]
                [SwaggerResponse(StatusCodes.Status400BadRequest, typeof(ProblemDetails), Description = "Bad Request")]
                [SwaggerResponse(StatusCodes.Status404NotFound, typeof(ProblemDetails), Description = "Not Found")]
                async Task<Results<NoContent, ProblemHttpResult>> (
                    Guid id,
                    ITodoService service,
                    CancellationToken cancellationToken) =>
                {
                    var wasCompleted = await service.CompleteItemAsync(id, cancellationToken);

                    return wasCompleted switch
                    {
                        true => TypedResults.NoContent(),
                        false => TypedResults.Problem("Item already completed.", statusCode: StatusCodes.Status400BadRequest),
                        _ => TypedResults.Problem("Item not found.", statusCode: StatusCodes.Status404NotFound),
                    };
                })
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .WithName("CompleteTodo")
                .WithSummary("Mark a Todo item as completed")
                .WithDescription("Marks the todo item with the specified ID as complete.");

            group.MapDelete(
                "/{id}",
                [NSwagOpenApiExample<ProblemDetails, ProblemDetailsExampleProvider>]
                [OpenApiOperation("Delete a Todo item", "Deletes the todo item with the specified ID.")]
                [SwaggerResponse(StatusCodes.Status204NoContent, typeof(void), Description = "No Content")]
                [SwaggerResponse(StatusCodes.Status404NotFound, typeof(ProblemDetails), Description = "Not Found")]
                async Task<Results<NoContent, ProblemHttpResult>> (
                    Guid id,
                    ITodoService service,
                    CancellationToken cancellationToken) =>
                {
                    var wasDeleted = await service.DeleteItemAsync(id, cancellationToken);
                    return wasDeleted switch
                    {
                        true => TypedResults.NoContent(),
                        false => TypedResults.Problem("Item not found.", statusCode: StatusCodes.Status404NotFound),
                    };
                })
                .ProducesProblem(StatusCodes.Status404NotFound)
                .WithName("DeleteTodo")
                .WithSummary("Delete a Todo item")
                .WithDescription("Deletes the todo item with the specified ID.");
        };

        // Redirect to Open API/Swagger documentation
        builder.MapGet("/api", () => Results.Redirect("/swagger-ui/index.html"))
               .ExcludeFromDescription();

        return builder;
    }
}
