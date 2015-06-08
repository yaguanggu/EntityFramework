// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Builders;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Tests
{
    public class ModelSourceTest
    {
        [Fact]
        public void Adds_all_entities_based_on_all_distinct_entity_types_found()
        {
            var setFinderMock = new Mock<IDbSetFinder>();
            setFinderMock.Setup(m => m.FindSets(It.IsAny<DbContext>())).Returns(
                new[]
                    {
                        new DbSetProperty(typeof(JustAClass), "One", typeof(Random), hasSetter: true),
                        new DbSetProperty(typeof(JustAClass), "Two", typeof(object), hasSetter: true),
                        new DbSetProperty(typeof(JustAClass), "Three", typeof(Random), hasSetter: true)
                    });

            var model = CreateDefaultModelSource(setFinderMock.Object).GetModel(new Mock<DbContext>().Object, new ModelBuilderFactory(), new LoggingModelValidator(new LoggerFactory()));

            Assert.Equal(
                new[] { TypeNameHelper.GetTypeDisplayName(typeof(Random)), TypeNameHelper.GetTypeDisplayName(typeof(object)) },
                model.EntityTypes.Select(e => e.Name).ToArray());
        }

        private class JustAClass
        {
            public DbSet<Random> One { get; set; }
            protected DbSet<object> Two { get; set; }
            private DbSet<string> Three { get; set; }
            private DbSet<string> Four { get; set; }
        }

        [Fact]
        public void Caches_model_by_context_type()
        {
            var modelSource = CreateDefaultModelSource(new DbSetFinder());

            var model1 = modelSource.GetModel(new Context1(), new ModelBuilderFactory(), new LoggingModelValidator(new LoggerFactory()));
            var model2 = modelSource.GetModel(new Context2(), new ModelBuilderFactory(), new LoggingModelValidator(new LoggerFactory()));

            Assert.NotSame(model1, model2);
            Assert.Same(model1, modelSource.GetModel(new Context1(), new ModelBuilderFactory(), new LoggingModelValidator(new LoggerFactory())));
            Assert.Same(model2, modelSource.GetModel(new Context2(), new ModelBuilderFactory(), new LoggingModelValidator(new LoggerFactory())));
        }

        private class Context1 : DbContext
        {
        }

        private class Context2 : DbContext
        {
        }

        private IModelSource CreateDefaultModelSource(IDbSetFinder setFinder) 
            => new ConcreteModelSource(setFinder);

        private class ConcreteModelSource : ModelSource
        {
            public ConcreteModelSource(IDbSetFinder setFinder)
                : base(setFinder)
            {
            }
        }

    }
}
