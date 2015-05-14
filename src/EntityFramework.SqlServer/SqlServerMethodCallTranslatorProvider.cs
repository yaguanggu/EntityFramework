// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.Relational.Query;
using Microsoft.Data.Entity.Relational.Query.Methods;
using Microsoft.Data.Entity.SqlServer.Query.Methods;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.SqlServer
{
    public class SqlServerMethodCallTranslatorProvider : RelationalMethodCallTranslatorProvider
    {
        private List<IMethodCallTranslator> _sqlServerMethodCallTranslators;

        private List<IPropertyTranslator> _sqlServerPropertyTranslators;

        public SqlServerMethodCallTranslatorProvider(ILoggerFactory loggerFactory)
            : base(loggerFactory)
        {
            _sqlServerMethodCallTranslators = new List<IMethodCallTranslator>
            {
                new NewGuidTranslator(),
                new StringSubstringTranslator(),
                new MathAbsTranslator(),
                new MathCeilingTranslator(),
                new MathFloorTranslator(),
                new MathPowerTranslator(),
                new MathRoundTranslator(),
                new MathTruncateTranslator(),
                new StringReplaceTranslator(),
                new StringToLowerTranslator(),
                new StringToUpperTranslator(),
            };

            _sqlServerPropertyTranslators = new List<IPropertyTranslator>
            {
                new StringLengthTranslator(),
                new DateTimeNowTranslator(),
            };
        }

        public override IReadOnlyList<IMethodCallTranslator> MethodCallTranslators 
            => base.MethodCallTranslators.Concat(_sqlServerMethodCallTranslators).ToList();

        public override IReadOnlyList<IPropertyTranslator> PropertyTranslators
            => base.PropertyTranslators.Concat(_sqlServerPropertyTranslators).ToList();

    }
}
