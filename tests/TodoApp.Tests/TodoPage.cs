﻿// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.Playwright;

namespace TodoApp;

/// <summary>
/// A class representing the Page Object Model for the Todo application.
/// </summary>
public class TodoPage(IPage page)
{
    public async Task AddItemAsync(string text)
    {
        await page.FillAsync(Selectors.AddItemText, text);
        await page.ClickAsync(Selectors.AddItemButton);

        var input = await page.QuerySelectorAsync(Selectors.AddItemText);
        await input!.WaitForElementStateAsync(ElementState.Editable);
    }

    public async Task<IReadOnlyList<TodoPageItem>> GetItemsAsync()
    {
        var elements = await page.QuerySelectorAllAsync(Selectors.TodoItem);
        return elements.Select(x => new TodoPageItem(x)).ToArray();
    }

    public async Task WaitForNoItemsAsync()
        => await page.WaitForSelectorAsync(Selectors.NoItems);

    public async Task WaitForPageAsync()
        => await page.WaitForSelectorAsync(Selectors.AddItemButton);

    public sealed class TodoPageItem(IElementHandle item)
    {
        public async Task CompleteAsync()
        {
            var element = await item.QuerySelectorAsync(Selectors.CompleteItem);
            await element!.ClickAsync();
        }

        public async Task DeleteAsync()
        {
            var element = await item.QuerySelectorAsync(Selectors.DeleteItem);
            await element!.ClickAsync();
        }

        public async Task<string> TextAsync()
        {
            var element = await item.QuerySelectorAsync(Selectors.ItemText);
            return await element!.InnerTextAsync();
        }

        public async Task<string> LastUpdatedAsync()
        {
            var element = await item.QuerySelectorAsync(Selectors.ItemTimestamp);
            return await element!.InnerTextAsync();
        }
    }

    private sealed class Selectors
    {
        internal const string AddItemButton = "id=add-new-item";
        internal const string AddItemText = "id=new-item-text";
        internal const string CompleteItem = "button[class*='todo-item-complete']";
        internal const string DeleteItem = "button[class*='todo-item-delete']";
        internal const string ItemText = "[class*='todo-item-text']";
        internal const string ItemTimestamp = "[class*='todo-item-timestamp']";
        internal const string NoItems = "id=banner";
        internal const string TodoItem = "[class='todo-item']";
    }
}
