// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace TodoApp;

/// <summary>
/// An attribute representing an example for an OpenAPI operation parameter. This class cannot be inherited.
/// </summary>
/// <param name="value">The example value.</param>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
public class OpenApiGuidExampleAttribute(string value) : OpenApiExampleAttribute<Guid, OpenApiGuidExampleAttribute>, IExampleProvider<Guid>
{
    /// <summary>
    /// Gets the example value.
    /// </summary>
    public Guid Value { get; } = Guid.Parse(value);

    /// <inheritdoc/>
    static Guid IExampleProvider<Guid>.GenerateExample() => Guid.Parse("a03952ca-880e-4af7-9cfa-630be0feb4a5");

    /// <inheritdoc />
    public override Guid GenerateExample() => Value;
}
