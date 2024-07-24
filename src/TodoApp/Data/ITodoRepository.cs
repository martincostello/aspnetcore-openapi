// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace TodoApp.Data;

public interface ITodoRepository
{
    Task<TodoItem> AddItemAsync(string text, CancellationToken cancellationToken = default);

    Task<bool?> CompleteItemAsync(Guid itemId, CancellationToken cancellationToken = default);

    Task<bool> DeleteItemAsync(Guid itemId, CancellationToken cancellationToken = default);

    Task<TodoItem?> GetItemAsync(Guid itemId, CancellationToken cancellationToken = default);

    Task<IList<TodoItem>> GetItemsAsync(CancellationToken cancellationToken = default);
}
