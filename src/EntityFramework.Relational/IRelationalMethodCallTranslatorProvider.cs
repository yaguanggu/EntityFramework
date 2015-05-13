// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Relational.Query.Methods;
using System.Collections.Generic;

namespace Microsoft.Data.Entity.Relational
{
    public interface IRelationalMethodCallTranslatorProvider
    {
        IReadOnlyList<IMethodCallTranslator> MethodCallTranslators { get;  }
    }
}
