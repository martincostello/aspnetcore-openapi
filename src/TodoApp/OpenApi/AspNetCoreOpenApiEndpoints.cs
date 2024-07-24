// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json.Serialization.Metadata;
using System.Xml;
using System.Xml.XPath;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace TodoApp;

public static class AspNetCoreOpenApiEndpoints
{
    public static IServiceCollection AddAspNetCoreOpenApi(this IServiceCollection services)
    {
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, _, _) =>
            {
                document.Info.Title = "Todo API (ASP.NET Core OpenAPI)";
                document.Info.Version = "v1";

                document.Info.Contact = new()
                {
                    Name = "Martin Costello",
                    Url = new("https://www.martincostello.com"),
                };

                document.Info.License = new()
                {
                    Name = "Apache 2.0",
                    Url = new("https://www.apache.org/licenses/LICENSE-2.0"),
                };

                var scheme = new OpenApiSecurityScheme()
                {
                    BearerFormat = "JSON Web Token",
                    Description = "Bearer authentication using a JWT.",
                    Scheme = "bearer",
                    Type = SecuritySchemeType.Http,
                    Reference = new()
                    {
                        Id = "Bearer",
                        Type = ReferenceType.SecurityScheme,
                    },
                };

                document.Components ??= new();
                document.Components.SecuritySchemes ??= new Dictionary<string, OpenApiSecurityScheme>();
                document.Components.SecuritySchemes[scheme.Reference.Id] = scheme;
                document.SecurityRequirements.Add(new() { [scheme] = [] });

                return Task.CompletedTask;
            });

            // Add a custom schema transformer to add descriptions from XML comments
            options.AddSchemaTransformer<AddSchemaDescriptionsTransformer>();
        });

        return services;
    }

    public static T UseAspnetCoreOpenApi<T>(this T builder)
        where T : IApplicationBuilder, IEndpointRouteBuilder
    {
        builder.MapOpenApi();

        return builder;
    }

    public class AddSchemaDescriptionsTransformer : IOpenApiSchemaTransformer
    {
        private readonly Assembly _thisAssembly = typeof(AddSchemaDescriptionsTransformer).Assembly;
        private readonly ConcurrentDictionary<string, string?> _descriptions = [];
        private XPathNavigator? _navigator;

        /// <inheritdoc/>
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
            if (_descriptions.TryGetValue(memberName, out string? description))
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
                string? typeName = propertyInfo.DeclaringType.FullName;
                string propertyName =
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
                string path = Path.Combine(AppContext.BaseDirectory, $"{_thisAssembly.GetName().Name}.xml");
                using var reader = XmlReader.Create(path);
                _navigator = new XPathDocument(reader).CreateNavigator();
            }

            return _navigator;
        }
    }
}
