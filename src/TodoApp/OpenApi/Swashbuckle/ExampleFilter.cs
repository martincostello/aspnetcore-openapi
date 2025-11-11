// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TodoApp.OpenApi.Swashbuckle;

/// <summary>
/// A class representing an operation and schema filter that adds the example to use for display in OpenAPI documentation.
/// </summary>
public class ExampleFilter : ExamplesProcessor, IOperationFilter, ISchemaFilter
{
    /// <inheritdoc />
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
        => Process(operation, context.ApiDescription);

    /// <inheritdoc />
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema is OpenApiSchema concrete)
        {
            Process(concrete, context.Type);
        }
    }
}
