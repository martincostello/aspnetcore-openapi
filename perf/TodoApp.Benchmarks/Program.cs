﻿// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using BenchmarkDotNet.Running;
using TodoApp;

if (args.SequenceEqual(["--test"]))
{
    await using var benchmarks = new OpenApiBenchmarks();
    await benchmarks.StartServer();

    try
    {
        _ = await benchmarks.AspNetCore();
        _ = await benchmarks.NSwag();
        _ = await benchmarks.Swashbuckle();
    }
    finally
    {
        await benchmarks.StopServer();
    }

    return 0;
}
else
{
    var summary = BenchmarkRunner.Run<OpenApiBenchmarks>(args: args);
    return summary.Reports.Any((p) => !p.Success) ? 1 : 0;
}
