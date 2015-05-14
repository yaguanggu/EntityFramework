// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Linq.Expressions;
using Microsoft.Data.Entity.Relational.Query.Methods;
using Microsoft.Data.Entity.Relational.Query.Expressions;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.SqlServer.Query.Methods
{
    public class StringToUpperTranslator : IMethodCallTranslator
    {
        public Expression Translate([NotNull] MethodCallExpression methodCallExpression)
        {
            var methodInfo = typeof(string).GetMethods().Where(m => m.Name == "ToUpper" && m.GetParameters().Count() == 0).Single();
            if (methodInfo == methodCallExpression.Method)
            {
                var sqlArguments = new[] { methodCallExpression.Object };
                return new SqlFunctionExpression("UPPER", sqlArguments, methodCallExpression.Type);
            }

            return null;
        }
    }
}
