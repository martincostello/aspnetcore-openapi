// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace TodoApp.NSwag;

public class NSwagOpenApiExampleAttribute<T> : NSwagOpenApiExampleAttribute<T, T>
    where T : IExampleProvider<T>
{
}
