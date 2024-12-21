// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.Playwright;

namespace TodoApp;

[Collection(HttpServerCollection.Name)]
public class UITests : IAsyncLifetime
{
    public UITests(HttpServerFixture fixture, ITestOutputHelper outputHelper)
    {
        Fixture = fixture;

        // Route output from the fixture's logs to xunit's output
        OutputHelper = outputHelper;
        Fixture.SetOutputHelper(OutputHelper);
    }

    private HttpServerFixture Fixture { get; }

    private ITestOutputHelper OutputHelper { get; }

    public static TheoryData<string, string?> Browsers()
    {
        var browsers = new TheoryData<string, string?>()
        {
            { BrowserType.Chromium, null },
            { BrowserType.Chromium, "chrome" },
            { BrowserType.Firefox, null },
        };

        // Skip on macOS. See https://github.com/microsoft/playwright-dotnet/issues/2920.
        if (!OperatingSystem.IsLinux() && !OperatingSystem.IsMacOS())
        {
            browsers.Add(BrowserType.Chromium, "msedge");
        }

        if (OperatingSystem.IsMacOS())
        {
            browsers.Add(BrowserType.Webkit, null);
        }

        return browsers;
    }

    [Theory]
    [MemberData(nameof(Browsers))]
    public async Task Can_Sign_In_And_Manage_Todo_Items(string browserType, string? browserChannel)
    {
        // Arrange
        var options = new BrowserFixtureOptions
        {
            BrowserType = browserType,
            BrowserChannel = browserChannel
        };

        var browser = new BrowserFixture(options, OutputHelper);
        await browser.WithPageAsync(async page =>
        {
            // Load the application
            await page.GotoAsync(Fixture.ServerAddress);
            await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

            var app = new TodoPage(page);

            // Arrange - Wait for list to be ready
            await app.WaitForNoItemsAsync();

            // Act - Add an item
            await app.AddItemAsync("Buy cheese");

            // Assert
            var items = await app.GetItemsAsync();
            var item = Assert.Single(items);

            Assert.Equal("Buy cheese", await item.TextAsync());
            Assert.Equal("a few seconds ago", await item.LastUpdatedAsync());

            // Act - Add another item
            await app.AddItemAsync("Buy eggs");

            // Assert
            items = await app.GetItemsAsync();
            Assert.Equal(2, items.Count);

            Assert.Equal("Buy cheese", await items[0].TextAsync());
            Assert.Equal("Buy eggs", await items[1].TextAsync());

            // Act - Delete an item and complete an item
            await items[0].DeleteAsync();
            await items[1].CompleteAsync();

            await Task.Delay(TimeSpan.FromSeconds(0.5), TestContext.Current.CancellationToken);

            // Assert
            items = await app.GetItemsAsync();
            item = Assert.Single(items);

            Assert.Equal("Buy eggs", await item.TextAsync());

            // Act - Delete the remaining item
            await item.DeleteAsync();

            // Assert
            await app.WaitForNoItemsAsync();
        });
    }

    public ValueTask InitializeAsync()
    {
        InstallPlaywright();
        return ValueTask.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }

    private static void InstallPlaywright()
    {
        int exitCode = Microsoft.Playwright.Program.Main(["install"]);

        if (exitCode != 0)
        {
            throw new InvalidOperationException($"Playwright exited with code {exitCode}");
        }
    }
}
