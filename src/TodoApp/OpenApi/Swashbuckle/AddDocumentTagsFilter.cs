// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TodoApp.OpenApi.Swashbuckle;

/// <summary>
/// A Swashbuckle document filter that adds document-level tags.
/// </summary>
public class AddDocumentTagsFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        swaggerDoc.Tags ??= new HashSet<OpenApiTag>();
        swaggerDoc.Tags.Add(new() { Name = "TodoApp" });
    }
}
