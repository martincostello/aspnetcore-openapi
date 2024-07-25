// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using TodoApp.OpenApi;

namespace TodoApp.Models;

/// <summary>
/// Represents the model for a created Todo item.
/// </summary>
[OpenApiExample<CreatedTodoItemModel>]
public class CreatedTodoItemModel : IExampleProvider<CreatedTodoItemModel>
{
    /// <summary>
    /// Gets or sets the ID of the created Todo item.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    public static CreatedTodoItemModel GenerateExample()
    {
        return new()
        {
            Id = "a03952ca-880e-4af7-9cfa-630be0feb4a5",
        };
    }
}
