using System;

namespace ALX.TableStorage.Infrastructure.Configuration
{
    public class TableStorageConfiguration
    {
        #region Constructor

        public TableStorageConfiguration(string accountName, string key)
        {
            if (string.IsNullOrEmpty(accountName))
                throw new ArgumentNullException("Invalid account");

            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("Invalid key");
            
            AccountName = accountName;
            Key = key;            
        }

        public TableStorageConfiguration(string connectionStrings)
        {
            if (string.IsNullOrEmpty(connectionStrings))
                throw new ArgumentNullException("Invalid connection strings");            

            ConnectionStrings = connectionStrings;            
        }

        #endregion

        #region Properties

        public string AccountName { get; }
        public string Key { get; }
        public string ConnectionStrings { get; internal set; }
        public string TableName { get; }        

        #endregion
    }
}