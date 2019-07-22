using System;

namespace ALX.TableStorage.Infrastructure
{
    public class TableStorageAttribute : Attribute
    {
        public string TableName { get; private set; }

        public TableStorageAttribute(string name)
        {
            this.TableName = name;            
        }
    }
}