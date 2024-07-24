// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using TodoApp.Models;

namespace TodoApp.Services;

public interface ITodoService
{
    Task<string> AddItemAsync(string text, CancellationToken cancellationToken);

    Task<bool?> CompleteItemAsync(Guid itemId, CancellationToken cancellationToken);

    Task<bool> DeleteItemAsync(Guid itemId, CancellationToken cancellationToken);

    Task<TodoItemModel?> GetAsync(Guid itemId, CancellationToken cancellationToken);

    Task<TodoListViewModel> GetListAsync(CancellationToken cancellationToken);
}
