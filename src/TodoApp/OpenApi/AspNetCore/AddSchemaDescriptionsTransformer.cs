// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json.Serialization.Metadata;
using System.Xml;
using System.Xml.XPath;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace TodoApp.OpenApi.AspNetCore;

/// <summary>
/// An OpenAPI schema transformer that adds descriptions from XML documentation.
/// </summary>
public class AddSchemaDescriptionsTransformer : IOpenApiSchemaTransformer
{
    private readonly Assembly _thisAssembly = typeof(AddSchemaDescriptionsTransformer).Assembly;
    private readonly ConcurrentDictionary<string, string?> _descriptions = [];
    private XPathNavigator? _navigator;

    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        if (schema.Description is null &&
            GetMemberName(context.JsonTypeInfo, context.JsonPropertyInfo) is { Length: > 0 } memberName &&
            GetDescription(memberName) is { Length: > 0 } description)
        {
            schema.Description = description;
        }

        return Task.CompletedTask;
    }

    private string? GetDescription(string memberName)
    {
        if (_descriptions.TryGetValue(memberName, out var description))
        {
            return description;
        }

        var navigator = CreateNavigator();
        var node = navigator.SelectSingleNode($"/doc/members/member[@name='{memberName}']/summary");

        if (node is not null)
        {
            description = node.Value.Trim();
        }

        _descriptions[memberName] = description;

        return description;
    }

    private string? GetMemberName(JsonTypeInfo typeInfo, JsonPropertyInfo? propertyInfo)
    {
        if (typeInfo.Type.Assembly != _thisAssembly &&
            propertyInfo?.DeclaringType.Assembly != _thisAssembly)
        {
            return null;
        }
        else if (propertyInfo is not null)
        {
            var typeName = propertyInfo.DeclaringType.FullName;
            var propertyName =
                propertyInfo.AttributeProvider is PropertyInfo property ?
                property.Name :
                $"{char.ToUpperInvariant(propertyInfo.Name[0])}{propertyInfo.Name[1..]}";

            return $"P:{typeName}{Type.Delimiter}{propertyName}";
        }
        else
        {
            return $"T:{typeInfo.Type.FullName}";
        }
    }

    private XPathNavigator CreateNavigator()
    {
        if (_navigator is null)
        {
            var path = Path.Combine(AppContext.BaseDirectory, $"{_thisAssembly.GetName().Name}.xml");
            using var reader = XmlReader.Create(path);
            _navigator = new XPathDocument(reader).CreateNavigator();
        }

        return _navigator;
    }
}
