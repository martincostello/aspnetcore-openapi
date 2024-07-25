// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using NSwag;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

namespace TodoApp.OpenApi.NSwag;

/// <summary>
/// A class representing an operation processor for removing NSwag extensions from an OpenAPI document.
/// </summary>
public class RemoveNSwagExtensionsProcessor : IOperationProcessor
{
    public bool Process(OperationProcessorContext context)
    {
        // Remove the custom "x-" properties that NSwag adds from the OpenAPI document
        foreach (var parameter in context.Parameters.Values)
        {
            parameter.Position = null;

            if (parameter.Kind is OpenApiParameterKind.Body)
            {
                parameter.Name = null;
            }
        }

        return true;
    }
}
