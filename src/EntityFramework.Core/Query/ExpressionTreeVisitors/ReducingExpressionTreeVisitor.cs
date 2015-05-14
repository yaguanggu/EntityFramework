// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using Remotion.Linq.Parsing;

namespace Microsoft.Data.Entity.Query.ExpressionTreeVisitors
{
    public class ReducingExpressionVisitor : ExpressionTreeVisitor
    {
        public override Expression VisitExpression(Expression node)
            => node != null
               && node.CanReduce
                ? base.VisitExpression(node.Reduce())
                : base.VisitExpression(node);
    }
}
