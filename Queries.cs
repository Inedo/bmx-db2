namespace Inedo.BuildMasterExtensions.DB2
{
    /// <summary>
    /// Contains SQL queries used by the DB2 provider.
    /// </summary>
    internal static class Queries
    {
        /// <summary>
        /// SQL query for creating the __BuildMaster_DbSchemaChanges table.
        /// </summary>
        public const string CreateChangeScriptTable =
@"CREATE TABLE ""__BuildMaster_DbSchemaChanges""
(
  ""Numeric_Release_Number"" BIGINT NOT NULL,
  ""Script_Id"" INTEGER NOT NULL,
  ""Batch_Name"" VARCHAR(50) NOT NULL,
  ""Executed_Date"" TIMESTAMP NOT NULL,
  ""Success_Indicator"" CHAR(1) NOT NULL,
  PRIMARY KEY(""Numeric_Release_Number"", ""Script_Id"")
)";

        /// <summary>
        /// SQL query for inserting a row into the __BuildMaster_DbSchemaChanges table.
        /// </summary>
        public const string InsertChangeScript =
@"INSERT INTO ""__BuildMaster_DbSchemaChanges""
(
  ""Numeric_Release_Number"", ""Script_Id"", ""Batch_Name"", ""Executed_Date"", ""Success_Indicator""
)
VALUES
(
  {0}, {1}, '{2}', CURRENT TIMESTAMP, '{3}'
)";
    }
}
