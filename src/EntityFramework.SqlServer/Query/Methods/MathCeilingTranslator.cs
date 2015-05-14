// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Data.Entity.Relational.Query.Methods;
using Microsoft.Data.Entity.Relational.Query.Expressions;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.SqlServer.Query.Methods
{
    public class MathCeilingTranslator : IMethodCallTranslator
    {
        public Expression Translate([NotNull] MethodCallExpression methodCallExpression)
        {
            var methodInfos = typeof(Math).GetMethods().Where(m => m.Name == "Ceiling");
            if (methodInfos.Contains(methodCallExpression.Method))
            {
                return new SqlFunctionExpression("CEILING", methodCallExpression.Arguments, methodCallExpression.Type);
            }

            return null;
        }
    }
}
