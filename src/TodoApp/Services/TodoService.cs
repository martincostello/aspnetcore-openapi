﻿// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using TodoApp.Data;
using TodoApp.Models;

namespace TodoApp.Services;

public class TodoService(ITodoRepository repository) : ITodoService
{
    public async Task<string> AddItemAsync(string text, CancellationToken cancellationToken)
    {
        var item = await repository.AddItemAsync(text, cancellationToken);
        return item.Id.ToString();
    }

    public async Task<bool?> CompleteItemAsync(Guid itemId, CancellationToken cancellationToken)
    {
        return await repository.CompleteItemAsync(itemId, cancellationToken);
    }

    public async Task<bool> DeleteItemAsync(Guid itemId, CancellationToken cancellationToken)
    {
        return await repository.DeleteItemAsync(itemId, cancellationToken);
    }

    public async Task<TodoItemModel?> GetAsync(Guid itemId, CancellationToken cancellationToken)
    {
        var item = await repository.GetItemAsync(itemId, cancellationToken);
        return item is null ? null : MapItem(item);
    }

    public async Task<TodoListViewModel> GetListAsync(CancellationToken cancellationToken)
    {
        var items = await repository.GetItemsAsync(cancellationToken);

        var result = new List<TodoItemModel>(items.Count);

        foreach (var todo in items)
        {
            result.Add(MapItem(todo));
        }

        return new() { Items = result };
    }

    private static TodoItemModel MapItem(TodoItem item)
    {
        return new()
        {
            Id = item.Id.ToString(),
            IsCompleted = item.CompletedAt.HasValue,
            LastUpdated = (item.CompletedAt ?? item.CreatedAt).ToString("u", CultureInfo.InvariantCulture),
            Text = item.Text
        };
    }
}
