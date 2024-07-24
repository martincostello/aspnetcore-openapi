// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace TodoApp.OpenApi;

/// <summary>
/// A class representing an operation processor that adds examples to API endpoints.
/// </summary>
public class AddExamplesTransformer : IOpenApiOperationTransformer, IOpenApiSchemaTransformer
{
    private static readonly TodoJsonSerializerContext Context = TodoJsonSerializerContext.Default;

    /// <inheritdoc />
    public Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        var examples = context.Description.ActionDescriptor.EndpointMetadata
            .OfType<IOpenApiExampleMetadata>()
            .ToArray();

        if (operation.Parameters is { Count: > 0 } parameters)
        {
            TryAddParameterExamples(parameters, context, examples);
        }

        if (operation.RequestBody is { } body)
        {
            TryAddRequestExamples(body, context, examples);
        }

        TryAddResponseExamples(operation.Responses, context, examples);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        if (context.JsonTypeInfo.Type == typeof(ProblemDetails))
        {
            schema.Example = ExampleFormatter.AsJson<ProblemDetails, ProblemDetailsExampleProvider>(Context);
        }
        else
        {
            var metadata = context.JsonTypeInfo.Type.GetCustomAttributes(false)
                .OfType<IOpenApiExampleMetadata>()
                .FirstOrDefault();

            if (metadata?.GenerateExample(Context) is { } value)
            {
                schema.Example = value;
            }
        }

        return Task.CompletedTask;
    }

    private static void TryAddParameterExamples(
        IList<OpenApiParameter> parameters,
        OpenApiOperationTransformerContext context,
        IList<IOpenApiExampleMetadata> examples)
    {
        var methodInfo = context.Description.ActionDescriptor.EndpointMetadata
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

    private static void TryAddRequestExamples(
        OpenApiRequestBody body,
        OpenApiOperationTransformerContext context,
        IList<IOpenApiExampleMetadata> examples)
    {
        if (!body.Content.TryGetValue("application/json", out var mediaType) || mediaType.Example is not null)
        {
            return;
        }

        var bodyParameter = context.Description.ParameterDescriptions.Single((p) => p.Source == BindingSource.Body);

        var metadata = bodyParameter.Type.GetCustomAttributes(false)
            .OfType<IOpenApiExampleMetadata>()
            .FirstOrDefault();

        metadata ??= examples.FirstOrDefault((p) => p.SchemaType == bodyParameter.Type);

        if (metadata is not null)
        {
            mediaType.Example ??= metadata.GenerateExample(Context);
        }
    }

    private static void TryAddResponseExamples(
        OpenApiResponses responses,
        OpenApiOperationTransformerContext context,
        IList<IOpenApiExampleMetadata> examples)
    {
        foreach (var schemaResponse in context.Description.SupportedResponseTypes)
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
