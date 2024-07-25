// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

namespace TodoApp.OpenApi.NSwag;

/// <summary>
/// A class representing an operation processor for updating the media type for HTTP 4xx responses.
/// </summary>
public class UpdateProblemDetailsMediaTypeProcessor : IOperationProcessor
{
    public bool Process(OperationProcessorContext context)
    {
        foreach ((var status, var response) in context.OperationDescription.Operation.Responses)
        {
            if (!status.StartsWith('4'))
            {
                continue;
            }

            // Update the default media type for 4xx responses to "application/problem+json"
            foreach ((var key, var mediaType) in response.Content.ToDictionary())
            {
                if (key is "application/json")
                {
                    response.Content["application/problem+json"] = mediaType;
                    response.Content.Remove(key);
                }
            }
        }

        return true;
    }
}
