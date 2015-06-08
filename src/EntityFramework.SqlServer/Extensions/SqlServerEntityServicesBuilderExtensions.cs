// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Query.Methods;
using Microsoft.Data.Entity.SqlServer;
using Microsoft.Data.Entity.SqlServer.Metadata;
using Microsoft.Data.Entity.SqlServer.Migrations;
using Microsoft.Data.Entity.SqlServer.Query.Methods;
using Microsoft.Data.Entity.SqlServer.Update;
using Microsoft.Data.Entity.SqlServer.ValueGeneration;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace

namespace Microsoft.Framework.DependencyInjection
{
    public static class SqlServerEntityServicesBuilderExtensions
    {
        public static EntityFrameworkServicesBuilder AddSqlServer([NotNull] this EntityFrameworkServicesBuilder builder)
        {
            Check.NotNull(builder, nameof(builder));

            ((IAccessor<IServiceCollection>)builder.AddRelational()).Service
                .AddSingleton<IDataStoreSource, SqlServerDataStoreSource>()
                .TryAdd(new ServiceCollection()
                    .AddSingleton<SqlServerModelBuilderFactory>()
                    .AddSingleton<ISqlServerValueGeneratorCache, SqlServerValueGeneratorCache>()
                    .AddSingleton<ISqlServerSqlGenerator, SqlServerSqlGenerator>()
                    .AddSingleton<SqlServerTypeMapper>()
                    .AddScoped<SqlServerModelSource>()
                    .AddSingleton<SqlServerMetadataExtensionProvider>()
                    .AddScoped<ISqlServerSequenceValueGeneratorFactory, SqlServerSequenceValueGeneratorFactory>()
                    .AddScoped<SqlServerModificationCommandBatchFactory>()
                    .AddScoped<SqlServerValueGeneratorSelector>()
                    .AddScoped<SqlServerDataStoreServices>()
                    .AddScoped<SqlServerDataStore>()
                    .AddScoped<ISqlServerConnection, SqlServerConnection>()
                    .AddScoped<SqlServerModelDiffer>()
                    .AddScoped<SqlServerMigrationSqlGenerator>()
                    .AddScoped<SqlServerDataStoreCreator>()
                    .AddScoped<SqlServerHistoryRepository>()
                    .AddScoped<SqlServerCompositeMethodCallTranslator>()
                    .AddScoped<SqlServerCompositeMemberTranslator>());

            return builder;
        }
    }
}
