// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using TodoApp.OpenApi;

namespace TodoApp.Swashbuckle;

/// <summary>
/// A class representing an operation filter that adds the example to use for display in Swagger documentation.
/// </summary>
public class ExampleFilter : IOperationFilter, ISchemaFilter
{
    private static readonly TodoJsonSerializerContext Context = TodoJsonSerializerContext.Default;

    /// <inheritdoc />
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var examples = context.ApiDescription.ActionDescriptor.EndpointMetadata
            .OfType<IOpenApiExampleMetadata>()
            .ToArray();

        AddParameterExamples(operation.Parameters, context, examples);
        AddResponseExamples(operation.Responses, context, examples);
    }

    /// <inheritdoc />
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type == typeof(ProblemDetails))
        {
            schema.Example = ExampleFormatter.AsJson<ProblemDetails, ProblemDetailsExampleProvider>(Context);
        }
        else
        {
            var metadata = context.Type
                .GetCustomAttributes()
                .OfType<IOpenApiExampleMetadata>()
                .FirstOrDefault();

            if (metadata != null)
            {
                schema.Example = metadata.GenerateExample(Context);
            }
        }
    }

    private static void AddParameterExamples(
        IList<OpenApiParameter> parameters,
        OperationFilterContext context,
        IList<IOpenApiExampleMetadata> examples)
    {
        var methodInfo = context.ApiDescription.ActionDescriptor.EndpointMetadata
            .OfType<MethodInfo>()
            .FirstOrDefault();

        var arguments = methodInfo?
            .GetParameters()
            .ToArray();

        if (arguments is { Length: > 0 })
        {
            foreach (var argument in arguments)
            {
                var metadata = argument.GetCustomAttributes()
                    .OfType<IOpenApiExampleMetadata>()
                    .FirstOrDefault((p) => p.SchemaType == argument.ParameterType);

                metadata ??= argument.ParameterType.GetCustomAttributes()
                    .OfType<IOpenApiExampleMetadata>()
                    .FirstOrDefault((p) => p.SchemaType == argument.ParameterType);

                metadata ??= examples.FirstOrDefault((p) => p.SchemaType == argument.ParameterType);

                if (metadata?.GenerateExample(Context) is { } value)
                {
                    var parameter = parameters.FirstOrDefault((p) => p.Name == argument.Name);
                    if (parameter is not null)
                    {
                        parameter.Example = value;
                    }
                }
            }
        }
    }

    private static void AddResponseExamples(
        OpenApiResponses responses,
        OperationFilterContext context,
        IList<IOpenApiExampleMetadata> examples)
    {
        foreach (var schemaResponse in context.ApiDescription.SupportedResponseTypes)
        {
            var metadata = schemaResponse.Type?.GetCustomAttributes()
                .OfType<IOpenApiExampleMetadata>()
                .FirstOrDefault((p) => p.SchemaType == schemaResponse.Type);

            schemaResponse.Type ??= schemaResponse.ModelMetadata?.ModelType;

            foreach (var responseFormat in schemaResponse.ApiResponseFormats)
            {
                if (responses.TryGetValue(schemaResponse.StatusCode.ToString(CultureInfo.InvariantCulture), out var response) &&
                    response.Content.TryGetValue(responseFormat.MediaType, out var mediaType))
                {
                    mediaType.Example ??= metadata?.GenerateExample(Context);
                    mediaType.Example ??= examples.SingleOrDefault((p) => p.SchemaType == schemaResponse.Type)?.GenerateExample(Context);
                }
            }
        }
    }
}
