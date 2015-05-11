// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class SqlServerTestConnection : DbConnection
    {
        private readonly SqlConnection _sqlConnection;
        private readonly int _commandTimeout;

        public SqlServerTestConnection(string connectionString, int commandTimeout)
        {
            _sqlConnection = new SqlConnection(connectionString);
            _commandTimeout = commandTimeout;
        }
        public DbCommand CreateCommand(string commandText, object[] parameters, SqlTransaction transaction)
        {
            var command = _sqlConnection.CreateCommand();

            if (transaction != null)
            {
                command.Transaction = transaction;
            }

            command.CommandText = commandText;
            command.CommandTimeout = _commandTimeout;

            for (var i = 0; i < parameters.Length; i++)
            {
                command.Parameters.AddWithValue("p" + i, parameters[i]);
            }

            return command;
        }

        public void ClearPool()
        {
            SqlConnection.ClearPool(_sqlConnection);
        }

        public override string ConnectionString
        {
            get { return _sqlConnection.ConnectionString; }
            set { _sqlConnection.ConnectionString = value; }
        }

        public override string Database => _sqlConnection.Database;

        public override string DataSource => _sqlConnection.DataSource;

        public override string ServerVersion => _sqlConnection.ServerVersion;

        public override ConnectionState State => _sqlConnection.State;

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
            => _sqlConnection.BeginTransaction(isolationLevel);

        public override void Close() => _sqlConnection.Close();

        public override void ChangeDatabase(string databaseName)
            => _sqlConnection.ChangeDatabase(databaseName);

        protected override DbCommand CreateDbCommand()
        {
            var dbCommand = SqlClientFactory.Instance.CreateCommand();
            dbCommand.Connection = _sqlConnection;
            dbCommand.CommandTimeout = _commandTimeout;

            return dbCommand;
        }

        public override void Open()
        {
            _sqlConnection.Open();
        }

        public override Task OpenAsync(CancellationToken cancellationToken)
        {
            return _sqlConnection.OpenAsync(cancellationToken);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _sqlConnection.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
