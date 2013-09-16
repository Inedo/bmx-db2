using System;
using System.Collections.Generic;
using System.Data.Common;
using IBM.Data.DB2;
using Inedo.BuildMaster.Extensibility.Providers;
using Inedo.BuildMaster.Extensibility.Providers.Database;
using Inedo.BuildMaster.Web;

namespace Inedo.BuildMasterExtensions.DB2
{
    /// <summary>
    /// IBM DB2 database provider.
    /// </summary>
    [ProviderProperties("DB2", "Supports DB2 9.7 and later.")]
    [CustomEditor(typeof(DB2DatabaseProviderEditor))]
    public sealed class DB2DatabaseProvider : DatabaseProviderBase, IChangeScriptProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DB2DatabaseProvider"/> class.
        /// </summary>
        public DB2DatabaseProvider()
        {
        }

        /// <summary>
        /// When implemented by a derived class, runs each query in the provided array.
        /// </summary>
        /// <param name="queries">An array of query text.</param>
        public override void ExecuteQueries(string[] queries)
        {
            using (var conn = (DB2Connection)CreateConnection(this.ConnectionString))
            {
                conn.Open();

                using (var cmd = new DB2Command(string.Empty, conn))
                {
                    foreach (var query in queries)
                    {
                        cmd.CommandText = query;
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }
        /// <summary>
        /// When implemented by a derived class, runs the specified query.
        /// </summary>
        /// <param name="query">The database query to execute.</param>
        public override void ExecuteQuery(string query)
        {
            using (var conn = (DB2Connection)CreateConnection(this.ConnectionString))
            {
                conn.Open();

                using (var cmd = new DB2Command(query, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }
        /// <summary>
        /// When implemented in a derived class, indicates whether the provider
        /// is installed and available for use in the current execution context.
        /// </summary>
        /// <returns></returns>
        public override bool IsAvailable()
        {
            try
            {
                using (CreateConnection(null)) { }
                return true;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// When implemented in a derived class, attempts to connect with the
        /// current configuration and, if not successful, throws a
        /// <see cref="ConnectionException"/>.
        /// </summary>
        public override void ValidateConnection()
        {
            ExecuteQuery("SELECT 1 FROM \"SYSIBM\".DUAL");
        }
        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            try
            {
                using (var conn = CreateConnection(this.ConnectionString))
                {
                    return string.Format("DB2 on server {0}", conn.DataSource);
                }
            }
            catch
            {
                return "DB2";
            }
        }

        /// <summary>
        /// When implemented by a derived class, initializes the database by installing metadata tables
        /// for tracking change scripts and version numbers.
        /// </summary>
        public void InitializeDatabase()
        {
            if (IsDatabaseInitialized())
                throw new InvalidOperationException("Database is already initialized.");

            ExecuteQueries(
                new[]
                {
                    Queries.CreateChangeScriptTable,
                    string.Format(Queries.InsertChangeScript, 0, 0, "CREATE TABLE __BuildMaster_DbSchemaChanges", "Y")
                }
            );
        }
        /// <summary>
        /// When implemented by a derived class, indicates whether the database has been initialized.
        /// </summary>
        /// <returns>
        /// Value indicating whether the database has been initialized.
        /// </returns>
        public bool IsDatabaseInitialized()
        {
            var query = "SELECT COUNT(*) FROM SYSCAT.TABLES WHERE TABNAME='__BuildMaster_DbSchemaChanges'";
            using (var conn = (DB2Connection)CreateConnection(this.ConnectionString))
            {
                conn.Open();

                using (var cmd = new DB2Command(query, conn))
                {
                    return (int)cmd.ExecuteScalar() > 0;
                }
            }
        }
        /// <summary>
        /// When implemented by a derived class, retrieves the changes that occurred in the
        /// specified database in a table that matches the <see cref="TableDefs.DatabaseChangeHistory"/> schema
        /// </summary>
        /// <returns></returns>
        public ChangeScript[] GetChangeHistory()
        {
            var changeScripts = new List<DB2ChangeScript>();
            var query = "SELECT * FROM \"__BuildMaster_DbSchemaChanges\"";
            using (var conn = (DB2Connection)CreateConnection(this.ConnectionString))
            {
                conn.Open();

                using (var cmd = new DB2Command(query, conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            changeScripts.Add(
                                new DB2ChangeScript(
                                    reader.GetInt64(0),
                                    reader.GetInt32(1),
                                    reader.GetString(2),
                                    reader.GetDateTime(3),
                                    reader.GetString(4) == "Y"
                                )
                            );
                        }
                    }
                }
            }

            return changeScripts.ToArray();
        }
        /// <summary>
        /// When implemented in a derived class, retrieves the numeric release number of the
        /// database.
        /// </summary>
        /// <returns>
        /// The numeric release number of the database.
        /// </returns>
        public long GetSchemaVersion()
        {
            var query = "SELECT COALESCE(MAX(\"Numeric_Release_Number\"), 0) FROM \"__BuildMaster_DbSchemaChanges\"";
            using (var conn = (DB2Connection)CreateConnection(this.ConnectionString))
            {
                conn.Open();

                using (var cmd = new DB2Command(query, conn))
                {
                    return Convert.ToInt64(cmd.ExecuteScalar());
                }
            }
        }
        /// <summary>
        /// When implemented by a derived class, executes the specified script provided that the
        /// specified script has not already been executed, and returns a Boolean indicating whether
        /// the script was skipped as a result of being executed.
        /// </summary>
        /// <param name="numericReleaseNumber">Release number for the specified script name.</param>
        /// <param name="scriptId"></param>
        /// <param name="scriptName">Name of the script to be executed.</param>
        /// <param name="scriptText">Script text to be run.</param>
        /// <returns>
        /// Value indicating whether the script was skipped.
        /// </returns>
        public ExecutionResult ExecuteChangeScript(long numericReleaseNumber, int scriptId, string scriptName, string scriptText)
        {
            var query = "SELECT COUNT(*) FROM \"__BuildMaster_DbSchemaChanges\" WHERE \"Script_Id\" = " + scriptId;
            using (var conn = (DB2Connection)CreateConnection(this.ConnectionString))
            {
                conn.Open();

                using (var cmd = new DB2Command(query, conn))
                {
                    if ((int)cmd.ExecuteScalar() > 0)
                        return new ExecutionResult(ExecutionResult.Results.Skipped, scriptName + " already executed.");

                    Exception ex = null;
                    cmd.CommandText = scriptText;
                    try { cmd.ExecuteNonQuery(); }
                    catch (Exception _ex) { ex = _ex; }

                    cmd.CommandText = string.Format(Queries.InsertChangeScript, numericReleaseNumber, scriptId, scriptName.Replace("'", "''"), ex == null ? "Y" : "N");
                    cmd.ExecuteNonQuery();

                    if (ex == null)
                        return new ExecutionResult(ExecutionResult.Results.Success, scriptName + " executed successfully.");
                    else
                        return new ExecutionResult(ExecutionResult.Results.Failed, scriptName + " execution failed: " + ex.Message);
                }
            }
        }

        /// <summary>
        /// Creates a new DB2 database connection.
        /// </summary>
        /// <param name="connectionString">Optional connection string.</param>
        /// <returns>DB2 database connection.</returns>
        private static DbConnection CreateConnection(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                return new DB2Connection();
            else
                return new DB2Connection(connectionString);
        }
    }
}
