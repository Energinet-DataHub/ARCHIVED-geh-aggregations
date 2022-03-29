using Energinet.DataHub.Aggregations.Domain.MasterData;

namespace Energinet.DataHub.Aggregations.Infrastructure.Repository.SqlInstructions
{
    /// <summary>
    /// This interface describes the insert and update script used on the master data object
    /// </summary>
    /// <typeparam name="T">a master data object</typeparam>
    internal interface ISqlInstructions<T>
        where T : IMasterDataObject
    {
        /// <summary>
        /// The SQL string for updating the master data object
        /// </summary>
        string UpdateSql { get; }

        /// <summary>
        /// The SQL string for inserting the master data object
        /// </summary>
        string InsertSql { get;  }

        /// <summary>
        /// The SQL string getting objects by their Id
        /// </summary>
        string GetSql { get; }

        /// <summary>
        /// An anonymous object with the parameters used in the SQL script when updating
        /// </summary>
        /// <param name="masterDataObject"></param>
        /// <returns>An anonymous object </returns>
        object UpdateParameters(T masterDataObject);

        /// <summary>
        /// An anonymous object with the parameters used in the SQL script when inserting
        /// </summary>
        /// <param name="masterDataObject"></param>
        /// <returns>An anonymous object </returns>
        object InsertParameters(T masterDataObject);
    }
}
