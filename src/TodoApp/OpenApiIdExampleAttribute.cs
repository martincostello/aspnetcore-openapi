// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace TodoApp;

/// <summary>
/// An attribute representing an example for an OpenAPI ID operation parameter.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
public class OpenApiIdExampleAttribute() : OpenApiGuidExampleAttribute("a03952ca-880e-4af7-9cfa-630be0feb4a5");
