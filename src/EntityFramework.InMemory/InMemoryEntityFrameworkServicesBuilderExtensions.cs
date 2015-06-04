// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.InMemory;
using Microsoft.Data.Entity.InMemory.Query;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using CoreStrings = Microsoft.Data.Entity.Internal.Strings;

// ReSharper disable once CheckNamespace

namespace Microsoft.Framework.DependencyInjection
{
    public static class InMemoryEntityFrameworkServicesBuilderExtensions
    {
        public static EntityFrameworkServicesBuilder AddInMemoryStore([NotNull] this EntityFrameworkServicesBuilder builder)
        {
            Check.NotNull(builder, nameof(builder));

            var serviceCollection = ((IAccessor<IServiceCollection>)builder).Service;

            if (serviceCollection.Any(d
                => d.ServiceType == typeof(IDataStoreSource)
                   && d.ImplementationType == typeof(InMemoryDataStoreSource)))
            {
                throw new InvalidOperationException(CoreStrings.MultipleCallsToAddProvider(nameof(AddInMemoryStore)));
            }

            serviceCollection.AddSingleton<IDataStoreSource, InMemoryDataStoreSource>()
                .TryAdd(new ServiceCollection()
                    .AddSingleton<InMemoryValueGeneratorCache>()
                    .AddSingleton<IInMemoryDatabase, InMemoryDatabase>()
                    .AddSingleton<InMemoryModelSource>()
                    .AddScoped<InMemoryValueGeneratorSelector>()
                    .AddScoped<InMemoryQueryContextFactory>()
                    .AddScoped<InMemoryDataStoreServices>()
                    .AddScoped<InMemoryDatabaseFactory>()
                    .AddScoped<IInMemoryDataStore, InMemoryDataStore>()
                    .AddScoped<InMemoryConnection>()
                    .AddScoped<InMemoryDataStoreCreator>());

            return builder;
        }
    }
}
