// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Data.Entity.Relational.Query.Methods;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Relational.Query
{
    public abstract class RelationalMethodCallTranslatorProvider : IRelationalFunctionTranslationProvider
    {
        private List<IMethodCallTranslator> _relationalMethodCallTranslators;
        private List<IPropertyTranslator> _relationalPropertyTranslators;

        public RelationalMethodCallTranslatorProvider(ILoggerFactory loggerFactory)
        {
            _relationalMethodCallTranslators = new List<IMethodCallTranslator>
            {
                new ContainsTranslator(),
                new EndsWithTranslator(),
                new EqualsTranslator(loggerFactory),
                new StartsWithTranslator(),
            };

            _relationalPropertyTranslators = new List<IPropertyTranslator>();
        }

        public virtual IReadOnlyList<IMethodCallTranslator> MethodCallTranslators => _relationalMethodCallTranslators;

        public virtual IReadOnlyList<IPropertyTranslator> PropertyTranslators => _relationalPropertyTranslators;
    }
}
