// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Data.Entity.Relational.Query.Sql;
using Microsoft.Data.Entity.Utilities;
using JetBrains.Annotations;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing;

namespace Microsoft.Data.Entity.Relational.Query.Expressions
{
    public class SqlFunctionExpression : ExtensionExpression
    {
        public SqlFunctionExpression(
            string functionName,
            IEnumerable<Expression> arguments,
            Type returnType)
            : base(returnType)
        {
            FunctionName = functionName;
            Arguments = arguments.ToList();
        }

        public string FunctionName { get; set; }

        public IReadOnlyList<Expression> Arguments { get; set; }

        public override Expression Accept([NotNull] ExpressionTreeVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            var specificVisitor = visitor as ISqlExpressionVisitor;

            return specificVisitor != null
                ? specificVisitor.VisitSqlFunctionExpression(this)
                : base.Accept(visitor);
        }

        protected override Expression VisitChildren(ExpressionTreeVisitor visitor)
        {
            var arguments = Arguments.Select(visitor.VisitExpression);

            return arguments.Count() != Arguments.Count() || arguments.Zip(Arguments, (l, r) => l != r).Any()
                ? new SqlFunctionExpression(FunctionName, arguments, Type)
                : this;
        }
    }
}
