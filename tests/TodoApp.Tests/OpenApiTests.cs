// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Validations;

namespace TodoApp;

[Collection(TodoAppCollection.Name)]
public class OpenApiTests
{
    public OpenApiTests(TodoAppFixture fixture, ITestOutputHelper outputHelper)
    {
        Fixture = fixture;

        // Route output from the fixture's logs to xunit's output
        OutputHelper = outputHelper;
        Fixture.SetOutputHelper(OutputHelper);
    }

    private TodoAppFixture Fixture { get; }

    private ITestOutputHelper OutputHelper { get; }

    public static TheoryData<string> OpenApiUrls() => new()
    {
        { "/nswag/v1.json" },
        { "/openapi/v1.json" },
        ////{ "/swagger/v1/swagger.json" }, // TODO Disabled due to missing schema references
    };

    [Theory]
    [MemberData(nameof(OpenApiUrls))]
    public async Task Schema_Is_Correct(string schemaUrl)
    {
        // Arrange
        var provider = schemaUrl.Split('/')[1];

        using var client = Fixture.CreateDefaultClient();

        // Act
        var actual = await client.GetStringAsync(schemaUrl, TestContext.Current.CancellationToken);

        // Assert
        var settings = new VerifySettings();
        settings.DontScrubDateTimes();
        settings.DontScrubGuids();
        settings.UseParameters(provider);

        await Verifier.VerifyJson(actual, settings);
    }

    [Theory]
    [MemberData(nameof(OpenApiUrls))]
    public async Task Schema_Has_No_Validation_Warnings(string schemaUrl)
    {
        // Arrange
        var ruleSet = ValidationRuleSet.GetDefaultRuleSet();
        using var client = Fixture.CreateDefaultClient();

        // Act
        using var schema = await client.GetStreamAsync(schemaUrl, TestContext.Current.CancellationToken);

        // Assert
        var actual = await OpenApiDocument.LoadAsync(schema, "json", cancellationToken: TestContext.Current.CancellationToken);

        Assert.Empty(actual.Diagnostic.Errors);

        var errors = actual.Document.Validate(ruleSet);

        Assert.Empty(errors);
    }
}
