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

        public SqlServerMethodCallTranslatorProvider(ILoggerFactory loggerFactory)
            : base(loggerFactory)
        {
            _sqlServerMethodCallTranslators = new List<IMethodCallTranslator>
            {
                new NewGuidTranslator(),
                new SubstringTranslator(),
            };
        }

        public override IReadOnlyList<IMethodCallTranslator> MethodCallTranslators 
            => base.MethodCallTranslators.Concat(_sqlServerMethodCallTranslators).ToList();
    }
}
