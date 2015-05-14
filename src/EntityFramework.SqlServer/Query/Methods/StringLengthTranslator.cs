// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using Microsoft.Data.Entity.Relational.Query.Methods;
using Microsoft.Data.Entity.Relational.Query.Expressions;
using JetBrains.Annotations;
using System;

namespace Microsoft.Data.Entity.SqlServer.Query.Methods
{
    public class StringLengthTranslator : IPropertyTranslator
    {
        public bool CanTranslate([NotNull] MemberExpression memberExpression)
        {
            return memberExpression.Expression != null 
                && memberExpression.Expression.Type == typeof(string)
                && memberExpression.Member.Name == "Length";
        }

        public Expression Translate([NotNull] MemberExpression memberExpression)
        {
            return new SqlFunctionExpression("LEN", new[] { memberExpression.Expression }, memberExpression.Type);
        }
    }
}
