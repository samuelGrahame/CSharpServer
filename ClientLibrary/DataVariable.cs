using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cSharpServer.ClientLibrary
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class DataVariable : Attribute
    {
        public int Order;
        public string FieldName;

        public DataVariable(int order, string fieldName)
        {
            this.Order = order;
            this.FieldName = fieldName;
        }
    }
}
