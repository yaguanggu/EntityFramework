// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq.Parsing;
using System.Linq.Expressions;

namespace Microsoft.Data.Entity.Relational.Query
{
    public class EqualityPredicateConstantPruner : ExpressionTreeVisitor
    {
        protected override Expression VisitBinaryExpression(
                  [NotNull] BinaryExpression binaryExpression)
        {
            Check.NotNull(binaryExpression, nameof(binaryExpression));

            if (binaryExpression.NodeType == ExpressionType.Equal
                || binaryExpression.NodeType == ExpressionType.NotEqual)
            {
                var optimize = TryRemoveRedundantBooleanConstants(binaryExpression);

                if (optimize != null)
                {
                    return optimize;
                }
            }

            return binaryExpression;
        }

        private Expression TryRemoveRedundantBooleanConstants(BinaryExpression binaryExpression)
        {
            var left = VisitExpression(binaryExpression.Left);
            var right = VisitExpression(binaryExpression.Right);

            if (left.Type == typeof(bool))
            {
                var leftConstant = left as ConstantExpression;
                if (leftConstant != null)
                {
                    if (binaryExpression.NodeType == ExpressionType.Equal)
                    {
                        return (bool)leftConstant.Value ? right : Expression.Not(right);
                    }

                    if (binaryExpression.NodeType == ExpressionType.NotEqual)
                    {
                        return (bool)leftConstant.Value ? Expression.Not(right) : right;
                    }
                }
            }

            if (right.Type == typeof(bool))
            {
                var rightConstant = right as ConstantExpression;
                if (rightConstant != null)
                {
                    if (binaryExpression.NodeType == ExpressionType.Equal)
                    {
                        return (bool)rightConstant.Value ? left : Expression.Not(left);
                    }

                    if (binaryExpression.NodeType == ExpressionType.NotEqual)
                    {
                        return (bool)rightConstant.Value ? Expression.Not(left) : left;
                    }
                }
            }

            return null;
        }
    }
}
