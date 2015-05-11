// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Entity.Relational.FunctionalTests;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class SqlServerTestStore : RelationalTestStore
    {
        public const int CommandTimeout = 30;

        private static int _scratchCount;

        public static Task<SqlServerTestStore> GetOrCreateSharedAsync(string name, Func<Task> initializeDatabase)
        {
            return new SqlServerTestStore(name).CreateSharedAsync(initializeDatabase);
        }

        public static SqlServerTestStore GetOrCreateShared(string name, Action initializeDatabase)
        {
            return new SqlServerTestStore(name).CreateShared(initializeDatabase);
        }

        /// <summary>
        ///     A non-transactional, transient, isolated test database. Use this in the case
        ///     where transactions are not appropriate.
        /// </summary>
        public static Task<SqlServerTestStore> CreateScratchAsync(bool createDatabase = true)
        {
            var name = "Microsoft.Data.SqlServer.Scratch_" + Interlocked.Increment(ref _scratchCount);
            return new SqlServerTestStore(name).CreateTransientAsync(createDatabase);
        }

        public static SqlServerTestStore CreateScratch(bool createDatabase = true)
        {
            var name = "Microsoft.Data.SqlServer.Scratch_" + Interlocked.Increment(ref _scratchCount);
            return new SqlServerTestStore(name).CreateTransient(createDatabase);
        }

        private SqlServerTestConnection _connection;
        private SqlTransaction _transaction;
        private readonly string _name;
        private bool _deleteDatabase;

        // Use async static factory method
        private SqlServerTestStore(string name)
        {
            _name = name;
        }

        private async Task<SqlServerTestStore> CreateSharedAsync(Func<Task> initializeDatabase)
        {
            await CreateSharedAsync(typeof(SqlServerTestStore).Name + _name, initializeDatabase);

            _connection = new SqlConnection(CreateConnectionString(_name));

            await _connection.OpenAsync();

            _transaction = _connection.BeginTransaction();

            return this;
        }

        private SqlServerTestStore CreateShared(Action initializeDatabase)
        {
            CreateShared(typeof(SqlServerTestStore).Name + _name, initializeDatabase);

            _connection = new SqlConnection(CreateConnectionString(_name));

            _connection.Open();

            _transaction = _connection.BeginTransaction();

            return this;
        }

        public static async Task CreateDatabaseIfNotExistsAsync(string name, string scriptPath = null)
        {
            using (var master = new SqlConnection(CreateConnectionString("master")))
            {
                await master.OpenAsync();

                using (var command = master.CreateCommand())
                {
                    command.CommandTimeout = CommandTimeout;
                    command.CommandText
                        = $@"SELECT COUNT(*) FROM sys.databases WHERE name = N'{name}'";

                    var exists = (int)await command.ExecuteScalarAsync() > 0;

                    if (!exists)
                    {
                        if (scriptPath == null)
                        {
                            command.CommandText = $@"CREATE DATABASE [{name}]";

                            await command.ExecuteNonQueryAsync();

                            using (var newConnection = new SqlConnection(CreateConnectionString(name)))
                            {
                                await WaitForExistsAsync(newConnection);
                            }
                        }
                        else
                        {
                            // HACK: Probe for script file as current dir
                            // is different between k build and VS run.

                            if (!File.Exists(scriptPath))
                            {
                                var appBase = Environment.GetEnvironmentVariable("DNX_APPBASE");

                                if (appBase != null)
                                {
                                    scriptPath = Path.Combine(appBase, Path.GetFileName(scriptPath));
                                }
                            }

                            var script = File.ReadAllText(scriptPath);

                            foreach (var batch
                                in new Regex("^GO", RegexOptions.IgnoreCase | RegexOptions.Multiline)
                                    .Split(script))
                            {
                                command.CommandText = batch;

                                await command.ExecuteNonQueryAsync();
                            }
                        }
                    }
                }
            }
        }

        public static void CreateDatabaseIfNotExists(string name, string scriptPath = null)
        {
            using (var master = new SqlConnection(CreateConnectionString("master")))
            {
                master.Open();

                using (var command = master.CreateCommand())
                {
                    command.CommandTimeout = CommandTimeout;
                    command.CommandText = $@"SELECT COUNT(*) FROM sys.databases WHERE name = N'{name}'";

                    var exists = (int)command.ExecuteScalar() > 0;

                    if (!exists)
                    {
                        if (scriptPath == null)
                        {
                            command.CommandText = $@"CREATE DATABASE [{name}]";

                            command.ExecuteNonQuery();

                            using (var newConnection = new SqlConnection(CreateConnectionString(name)))
                            {
                                WaitForExists(newConnection);
                            }
                        }
                        else
                        {
                            // HACK: Probe for script file as current dir
                            // is different between k build and VS run.

                            if (!File.Exists(scriptPath))
                            {
                                var appBase = Environment.GetEnvironmentVariable("DNX_APPBASE");

                                if (appBase != null)
                                {
                                    scriptPath = Path.Combine(appBase, Path.GetFileName(scriptPath));
                                }
                            }

                            var script = File.ReadAllText(scriptPath);

                            foreach (var batch
                                in new Regex("^GO", RegexOptions.IgnoreCase | RegexOptions.Multiline)
                                    .Split(script))
                            {
                                command.CommandText = batch;

                                command.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }
        }

        private static async Task WaitForExistsAsync(SqlServerTestConnection connection)
        {
            var retryCount = 0;
            while (true)
            {
                try
                {
                    await connection.OpenAsync();

                    connection.Close();

                    return;
                }
                catch (SqlException e)
                {
                    if (++retryCount >= 30
                        || (e.Number != 233 && e.Number != -2 && e.Number != 4060))
                    {
                        throw;
                    }

                    connection.ClearPool();

                    Thread.Sleep(100);
                }
            }
        }

        private static void WaitForExists(SqlServerTestConnection connection)
        {
            var retryCount = 0;
            while (true)
            {
                try
                {
                    connection.Open();

                    connection.Close();

                    return;
                }
                catch (SqlException e)
                {
                    if (++retryCount >= 30
                        || (e.Number != 233 && e.Number != -2 && e.Number != 4060))
                    {
                        throw;
                    }

                    connection.ClearPool();

                    Thread.Sleep(100);
                }
            }
        }

        private async Task<SqlServerTestStore> CreateTransientAsync(bool createDatabase)
        {
            await DeleteDatabaseAsync(_name);

            _connection = new SqlConnection(CreateConnectionString(_name));

            if (createDatabase)
            {
                using (var master = new SqlConnection(CreateConnectionString("master")))
                {
                    await master.OpenAsync();
                    using (var command = master.CreateCommand())
                    {
                        command.CommandText = $"{Environment.NewLine}CREATE DATABASE [{_name}]";

                        await command.ExecuteNonQueryAsync();

                        await WaitForExistsAsync(_connection);
                    }
                }
                await _connection.OpenAsync();
            }

            _deleteDatabase = createDatabase;
            return this;
        }

        private SqlServerTestStore CreateTransient(bool createDatabase)
        {
            DeleteDatabase(_name);

            _connection = CreateConnection(_name);

            if (createDatabase)
            {
                using (var master = CreateConnection("master"))
                {
                    master.Open();
                    using (var command = master.CreateCommand())
                    {
                        command.CommandText = $"{Environment.NewLine}CREATE DATABASE [{_name}]";

                        command.ExecuteNonQuery();

                        WaitForExists(_connection);
                    }
                }
                _connection.Open();
            }

            _deleteDatabase = createDatabase;
            return this;
        }

        private async Task DeleteDatabaseAsync(string name)
        {
            using (var master = CreateConnection("master"))
            {
                await master.OpenAsync();

                using (var command = master.CreateCommand())
                {
                    command.CommandTimeout = CommandTimeout; // Query will take a few seconds if (and only if) there are active connections

                    // SET SINGLE_USER will close any open connections that would prevent the drop
                    command.CommandText
                        = string.Format(@"IF EXISTS (SELECT * FROM sys.databases WHERE name = N'{0}')
                                          BEGIN
                                              ALTER DATABASE [{0}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                                              DROP DATABASE [{0}];
                                          END", name);

                    await command.ExecuteNonQueryAsync();

                    var userFolder = Environment.GetEnvironmentVariable("USERPROFILE");
                    try
                    {
                        File.Delete(Path.Combine(userFolder, name + ".mdf"));
                    }
                    catch (Exception)
                    {
                    }

                    try
                    {
                        File.Delete(Path.Combine(userFolder, name + "_log.ldf"));
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        private void DeleteDatabase(string name)
        {
            using (var master = CreateConnection("master"))
            {
                master.Open();

                using (var command = master.CreateCommand())
                {
                    command.CommandTimeout = CommandTimeout; // Query will take a few seconds if (and only if) there are active connections

                    // SET SINGLE_USER will close any open connections that would prevent the drop
                    command.CommandText
                        = string.Format(@"IF EXISTS (SELECT * FROM sys.databases WHERE name = N'{0}')
                                          BEGIN
                                              ALTER DATABASE [{0}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                                              DROP DATABASE [{0}];
                                          END", name);

                    command.ExecuteNonQuery();

                    var userFolder = Environment.GetEnvironmentVariable("USERPROFILE");
                    try
                    {
                        File.Delete(Path.Combine(userFolder, name + ".mdf"));
                    }
                    catch (Exception)
                    {
                    }

                    try
                    {
                        File.Delete(Path.Combine(userFolder, name + "_log.ldf"));
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        public override DbConnection Connection => _connection;

        public override DbTransaction Transaction => _transaction;

        public async Task<T> ExecuteScalarAsync<T>(string sql, CancellationToken cancellationToken, params object[] parameters)
        {
            using (var command = _connection.CreateCommand(sql, parameters, _transaction))
            {
                return (T)await command.ExecuteScalarAsync(cancellationToken);
            }
        }

        public int ExecuteNonQuery(string sql, params object[] parameters)
        {
            using (var command = _connection.CreateCommand(sql, parameters, _transaction))
            {
                return command.ExecuteNonQuery();
            }
        }

        public Task<int> ExecuteNonQueryAsync(string sql, params object[] parameters)
        {
            using (var command = _connection.CreateCommand(sql, parameters, _transaction))
            {
                return command.ExecuteNonQueryAsync();
            }
        }

        public async Task<IEnumerable<T>> QueryAsync<T>(string sql, params object[] parameters)
        {
            using (var command = _connection.CreateCommand(sql, parameters, _transaction))
            {
                using (var dataReader = await command.ExecuteReaderAsync())
                {
                    var results = Enumerable.Empty<T>();

                    while (await dataReader.ReadAsync())
                    {
                        results = results.Concat(new[] { await dataReader.GetFieldValueAsync<T>(0) });
                    }

                    return results;
                }
            }
        }

        public override void Dispose()
        {
            _transaction?.Dispose();

            _connection.Dispose();

            if (_deleteDatabase)
            {
                DeleteDatabase(_name);
            }
        }

        public static void ConfigureDbContext(DbContextOptionsBuilder optionsBuilder, string dataStore)
            => optionsBuilder
                .UseSqlServer(SqlServerTestConfiguration.CreateConnectionString(dataStore))
                .CommandTimeout(SqlServerTestConfiguration.CommandTimeout);

        public static SqlServerTestConnection CreateConnection(string dataStore)
            => new SqlServerTestConnection(
                SqlServerTestConfiguration.CreateConnectionString(dataStore),
                SqlServerTestConfiguration.CommandTimeout);
    }
}
