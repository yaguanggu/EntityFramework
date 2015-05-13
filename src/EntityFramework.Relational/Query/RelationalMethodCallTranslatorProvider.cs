// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Data.Entity.Relational.Query.Methods;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Relational.Query
{
    public abstract class RelationalMethodCallTranslatorProvider : IRelationalMethodCallTranslatorProvider
    {
        private List<IMethodCallTranslator> _relationalMethodCallTranslators;

        public RelationalMethodCallTranslatorProvider(ILoggerFactory loggerFactory)
        {
            _relationalMethodCallTranslators = new List<IMethodCallTranslator>
            {
                new ContainsTranslator(),
                new EndsWithTranslator(),
                new EqualsTranslator(loggerFactory),
                new StartsWithTranslator(),
            };
        }

        public virtual IReadOnlyList<IMethodCallTranslator> MethodCallTranslators => _relationalMethodCallTranslators;
    }
}
