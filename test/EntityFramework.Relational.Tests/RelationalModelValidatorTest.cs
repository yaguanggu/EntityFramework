﻿
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Tests.Infrastructure;
using Microsoft.Data.Entity.Tests.TestUtilities;
using Xunit;

namespace Microsoft.Data.Entity.Relational.Tests
{
    public class RelationalModelValidatorTest : LoggingModelValidatorTest
    {
        [Fact]
        public virtual void Detects_duplicate_table_names()
        {
            var model = new Entity.Metadata.Model();
            var entityA = model.AddEntityType(typeof(A));
            var entityB = model.AddEntityType(typeof(B));
            entityA.Relational().Table = "Table";
            entityB.Relational().Table = "Table";

            VerifyError(Strings.DuplicateTableName("Table", null, entityB.DisplayName()), model);
        }

        [Fact]
        public virtual void Detects_duplicate_table_names_with_schema()
        {
            var model = new Entity.Metadata.Model();
            var entityA = model.AddEntityType(typeof(A));
            var entityB = model.AddEntityType(typeof(B));
            entityA.Relational().Table = "Table";
            entityA.Relational().Schema = "Schema";
            entityB.Relational().Table = "Table";
            entityB.Relational().Schema = "Schema";

            VerifyError(Strings.DuplicateTableName("Table", "Schema", entityB.DisplayName()), model);
        }

        [Fact]
        public virtual void Does_not_detects_duplicate_table_names_in_different_schema()
        {
            var model = new Entity.Metadata.Model();
            var entityA = model.AddEntityType(typeof(A));
            var entityB = model.AddEntityType(typeof(B));
            entityA.Relational().Table = "Table";
            entityA.Relational().Schema = "SchemaA";
            entityB.Relational().Table = "Table";
            entityB.Relational().Schema = "SchemaB";

            CreateModelValidator().Validate(model);
        }

        protected override ModelValidator CreateModelValidator()
        {
            return new RelationalModelValidator(new ListLoggerFactory(Log, l => l == typeof(ModelValidator).FullName));
        }
    }
}
