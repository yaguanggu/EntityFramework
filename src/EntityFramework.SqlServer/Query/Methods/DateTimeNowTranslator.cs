// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using Microsoft.Data.Entity.Relational.Query.Methods;
using Microsoft.Data.Entity.Relational.Query.Expressions;
using JetBrains.Annotations;
using System;
using System.Linq;

namespace Microsoft.Data.Entity.SqlServer.Query.Methods
{
    public class DateTimeNowTranslator : IPropertyTranslator
    {
        public bool CanTranslate([NotNull] MemberExpression memberExpression)
        {
            return memberExpression.Expression == null
                && memberExpression.Member.DeclaringType == typeof(DateTime)
                && memberExpression.Member.Name == "Now";
        }

        public Expression Translate([NotNull] MemberExpression memberExpression)
        {
            return new SqlFunctionExpression("GETDATE", Enumerable.Empty<Expression>(), memberExpression.Type);
        }
    }
}
