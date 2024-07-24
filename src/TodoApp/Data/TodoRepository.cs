﻿// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.EntityFrameworkCore;

namespace TodoApp.Data;

public class TodoRepository(TimeProvider timeProvider, TodoContext context) : ITodoRepository
{
    public async Task<TodoItem> AddItemAsync(string text, CancellationToken cancellationToken = default)
    {
        await EnsureDatabaseAsync(cancellationToken);

        var item = new TodoItem
        {
            CreatedAt = UtcNow(),
            Text = text
        };

        context.Add(item);

        await context.SaveChangesAsync(cancellationToken);

        return item;
    }

    public async Task<bool?> CompleteItemAsync(Guid itemId, CancellationToken cancellationToken = default)
    {
        var item = await GetItemAsync(itemId, cancellationToken);

        if (item is null)
        {
            return null;
        }

        if (item.CompletedAt.HasValue)
        {
            return false;
        }

        item.CompletedAt = UtcNow();

        context.Items.Update(item);

        await context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> DeleteItemAsync(Guid itemId, CancellationToken cancellationToken = default)
    {
        var item = await GetItemAsync(itemId, cancellationToken);

        if (item is null)
        {
            return false;
        }

        context.Items.Remove(item);

        await context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<TodoItem?> GetItemAsync(Guid itemId, CancellationToken cancellationToken = default)
    {
        await EnsureDatabaseAsync(cancellationToken);

        return await context.Items.FindItemAsync(itemId, cancellationToken);
    }

    public async Task<IList<TodoItem>> GetItemsAsync(CancellationToken cancellationToken = default)
    {
        await EnsureDatabaseAsync(cancellationToken);

        return await context.Items
            .OrderBy(x => x.CompletedAt.HasValue)
            .ThenBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    private async Task EnsureDatabaseAsync(CancellationToken cancellationToken)
        => await context.Database.EnsureCreatedAsync(cancellationToken);

    private DateTime UtcNow() => timeProvider.GetUtcNow().UtcDateTime;
}
