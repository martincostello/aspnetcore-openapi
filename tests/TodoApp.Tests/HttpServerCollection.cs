﻿// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace TodoApp;

[CollectionDefinition(Name)]
public sealed class HttpServerCollection : ICollectionFixture<HttpServerFixture>
{
    public const string Name = "TodoApp HTTP server collection";
}
