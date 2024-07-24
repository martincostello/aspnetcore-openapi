// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Readers;
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

    [Theory]
    [InlineData("/nswag/v1.json")]
    [InlineData("/openapi/v1.json")]
    [InlineData("/swagger/v1/swagger.json")]
    public async Task Schema_Has_No_Validation_Warnings(string schemaUrl)
    {
        // Arrange
        var ruleSet = ValidationRuleSet.GetDefaultRuleSet();
        using var client = Fixture.CreateDefaultClient();

        // Act
        using var schema = await client.GetStreamAsync(schemaUrl);

        // Assert
        var reader = new OpenApiStreamReader();
        var actual = await reader.ReadAsync(schema);

        Assert.Empty(actual.OpenApiDiagnostic.Errors);

        var errors = actual.OpenApiDocument.Validate(ruleSet);

        Assert.Empty(errors);
    }
}
