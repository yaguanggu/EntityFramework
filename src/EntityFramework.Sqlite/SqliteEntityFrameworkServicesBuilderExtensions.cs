// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Sqlite;
using Microsoft.Data.Entity.Sqlite.Metadata;
using Microsoft.Data.Entity.Sqlite.Migrations;
using Microsoft.Data.Entity.Sqlite.Update;
using Microsoft.Data.Entity.Sqlite.ValueGeneration;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using CoreStrings = Microsoft.Data.Entity.Internal.Strings;

// ReSharper disable once CheckNamespace

namespace Microsoft.Framework.DependencyInjection
{
    public static class SqliteEntityFrameworkServicesBuilderExtensions
    {
        public static EntityFrameworkServicesBuilder AddSqlite([NotNull] this EntityFrameworkServicesBuilder builder)
        {
            Check.NotNull(builder, nameof(builder));

            var serviceCollection = ((IAccessor<IServiceCollection>)builder.AddRelational()).Service;

            if (serviceCollection.Any(d
                => d.ServiceType == typeof(IDataStoreSource)
                   && d.ImplementationType == typeof(SqliteDataStoreSource)))
            {
                throw new InvalidOperationException(CoreStrings.MultipleCallsToAddProvider(nameof(AddSqlite)));
            }

            serviceCollection.AddSingleton<IDataStoreSource, SqliteDataStoreSource>()
                .TryAdd(new ServiceCollection()
                    .AddSingleton<SqliteValueGeneratorCache>()
                    .AddSingleton<SqliteSqlGenerator>()
                    .AddSingleton<SqliteMetadataExtensionProvider>()
                    .AddSingleton<SqliteTypeMapper>()
                    .AddSingleton<SqliteModelSource>()
                    .AddScoped<SqliteModificationCommandBatchFactory>()
                    .AddScoped<SqliteDataStoreServices>()
                    .AddScoped<SqliteDataStore>()
                    .AddScoped<SqliteDataStoreConnection>()
                    .AddScoped<SqliteMigrationSqlGenerator>()
                    .AddScoped<SqliteDataStoreCreator>()
                    .AddScoped<SqliteHistoryRepository>()
                    .AddScoped<SqliteCompositeMethodCallTranslator>()
                    .AddScoped<SqliteCompositeMemberTranslator>());

            return builder;
        }
    }
}
