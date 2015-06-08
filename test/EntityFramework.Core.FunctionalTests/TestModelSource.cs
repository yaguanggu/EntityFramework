// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Builders;

namespace Microsoft.Data.Entity.FunctionalTests
{
    public class TestModelSource : ModelSource
    {
        private readonly Action<ModelBuilder> _onModelCreating;

        public TestModelSource(Action<ModelBuilder> onModelCreating, IDbSetFinder setFinder)
            : base(setFinder)
        {
            _onModelCreating = onModelCreating;
        }

        protected override IModel CreateModel(DbContext context, IModelBuilderFactory modelBuilderFactory, IModelValidator validator)
        {
            var model = new Model();
            var modelBuilder = modelBuilderFactory.CreateConventionBuilder(model);

            FindSets(modelBuilder, context);

            _onModelCreating(modelBuilder);

            validator.Validate(model);

            return model;
        }

        private class ThrowingModelValidator : ModelValidator
        {
            protected override void ShowWarning(string message)
            {
                throw new InvalidOperationException(message);
            }
        }
    }
}
