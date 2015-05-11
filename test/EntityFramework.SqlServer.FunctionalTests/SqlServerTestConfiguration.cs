// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Framework.ConfigurationModel;
using System.Data.SqlClient;

#if DNX451 || DNXCORE50
using Microsoft.Framework.Runtime.Infrastructure;
using Microsoft.Framework.Runtime;
#endif

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class SqlServerTestConfiguration
    {
        private static readonly string _connectionString;

        static SqlServerTestConfiguration()
        {
            var applicationBasePath = ".";

#if DNX451 || DNXCORE50
            var services = CallContextServiceLocator.Locator.ServiceProvider;
            var appEnv = (IApplicationEnvironment)services.GetService(typeof(IApplicationEnvironment));
            applicationBasePath = appEnv.ApplicationBasePath;
#endif

            var configuration = new Configuration(applicationBasePath);

#if true
            configuration.AddJsonFile(@"..\..\..\EntityFramework.SqlServer.FunctionalTests\config.json");
#else
            configuration.AddJsonFile(@"..\EntityFramework.SqlServer.FunctionalTests\config.json");
#endif
            configuration.AddEnvironmentVariables();

            CommandTimeout = configuration.Get<int>("EF:SqlServer:CommandTimeout");
            _connectionString = configuration.Get("EF:SqlServer:ConnectionString");
        }
        public static int CommandTimeout { get; }

        public static string CreateConnectionString(string dataStore)
        {
            return new SqlConnectionStringBuilder
            {
                ConnectionString = _connectionString,
                InitialCatalog = dataStore
            }.ConnectionString;
        }
    }
}
