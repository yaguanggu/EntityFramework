using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Relational
{
    public class RelationalModelValidator : LoggingModelValidator
    {
        public RelationalModelValidator([NotNull] ILoggerFactory loggerFactory)
            : base(loggerFactory)
        {
        }

        public override void Validate(IModel model)
        {
            base.Validate(model);
            EnsureDistinctTableNames(model);
        }

        protected void EnsureDistinctTableNames(IModel model)
        {
            var tables = new HashSet<string>();
            foreach (var entityType in model.EntityTypes.Where(et => et.BaseType == null))
            {
                var tableName = entityType.Relational().Schema + "." + entityType.Relational().Table;
                if (tables.Contains(tableName))
                {
                    ShowError(Strings.DuplicateTableName(entityType.Relational().Table, entityType.Relational().Schema, entityType.DisplayName()));
                }
                tables.Add(tableName);
            }
        }
    }
}
