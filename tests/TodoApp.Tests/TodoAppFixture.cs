// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.Logging.XUnit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TodoApp;

public class TodoAppFixture : WebApplicationFactory<Program>, ITestOutputHelperAccessor
{
    public TodoAppFixture()
    {
        // Use HTTPS by default and do not follow
        // redirects so they can tested explicitly.
        ClientOptions.AllowAutoRedirect = false;
        ClientOptions.BaseAddress = new Uri("https://localhost");
    }

    public ITestOutputHelper? OutputHelper { get; set; }

    public void ClearOutputHelper()
        => OutputHelper = null;

    public void SetOutputHelper(ITestOutputHelper value)
        => OutputHelper = value;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(configBuilder =>
        {
            // Configure the test fixture to write the SQLite database
            // to a temporary directory, rather than in App_Data.
            var dataDirectory = Directory.CreateTempSubdirectory().FullName;

            configBuilder.AddInMemoryCollection([KeyValuePair.Create<string, string?>("DataDirectory", dataDirectory)]);
        });

        // Route the application's logs to the xunit output
        builder.ConfigureLogging(loggingBuilder => loggingBuilder.ClearProviders().AddXUnit(this));

        // Configure the correct content root for the static content and Razor pages
        builder.UseSolutionRelativeContentRoot(Path.Combine("src", "TodoApp"), "*.slnx");
    }
}
