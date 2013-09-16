using System;
using Inedo.BuildMaster.Extensibility.Providers.Database;

namespace Inedo.BuildMasterExtensions.DB2
{
    /// <summary>
    /// Represents a DB2 change script.
    /// </summary>
    [Serializable]
    public sealed class DB2ChangeScript : ChangeScript
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DB2ChangeScript"/> class.
        /// </summary>
        /// <param name="numericReleaseNumber">The numeric release number.</param>
        /// <param name="scriptId">The script ID.</param>
        /// <param name="name">The script name.</param>
        /// <param name="executionDate">The execution date.</param>
        /// <param name="successfullyExecuted">Value indicating whether the script was successfully executed.</param>
        public DB2ChangeScript(long numericReleaseNumber, int scriptId, string name, DateTime executionDate, bool successfullyExecuted)
            : base(numericReleaseNumber, scriptId, name, executionDate, successfullyExecuted)
        {
        }
    }
}
