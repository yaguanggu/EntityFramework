// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.InMemory;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Builders;
using Microsoft.Framework.DependencyInjection;
using Xunit;

namespace Microsoft.Data.Entity.FunctionalTests
{
    public class ModelSourceTest
    {
        [Fact] // Issue #943
        public void Can_replace_ModelSource_without_access_to_internals()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddInMemoryStore()
                .AddDbContext<JustSomeContext>()
                .ServiceCollection()
                .AddSingleton<InMemoryModelSource, MyModelSource>()
                .BuildServiceProvider();

            using (var context = serviceProvider.GetRequiredService<JustSomeContext>())
            {
                var model = context.Model;

                Assert.Equal("Us!", model["AllYourModelAreBelongTo"]);
                Assert.Equal("Us!", model.EntityTypes.Single(e => e.DisplayName() == "Base")["AllYourBaseAreBelongTo"]);
                Assert.Contains("Peak", model.EntityTypes.Select(e => e.DisplayName()));
            }
        }

        private class MyModelSource : InMemoryModelSource
        {
            public MyModelSource(IDbSetFinder setFinder)
                : base(setFinder)
            {
            }

            protected override IModel CreateModel(DbContext context, IModelBuilderFactory modelBuilderFactory, IModelValidator validator)
            {
                var model = base.CreateModel(context, modelBuilderFactory, validator) as Model;

                model["AllYourModelAreBelongTo"] = "Us!";

                return model;
            }
        }

        private class JustSomeContext : DbContext
        {
            public JustSomeContext(IServiceProvider serviceProvider)
                : base(serviceProvider)
            {
            }

            public DbSet<Peak> Peaks { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Base>().Annotation("AllYourBaseAreBelongTo", "Us!");
            }
        }

        private class Base
        {
            public int Id { get; set; }
        }

        private class Peak
        {
            public int Id { get; set; }
        }
    }
}
