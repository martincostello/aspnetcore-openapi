﻿// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace Microsoft.EntityFrameworkCore;

public static class DbSetExtensions
{
    public static ValueTask<TEntity?> FindItemAsync<TEntity, TKey>(
        this DbSet<TEntity> set,
        TKey keyValue,
        CancellationToken cancellationToken)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(keyValue);
        return set.FindAsync([keyValue], cancellationToken);
    }
}
