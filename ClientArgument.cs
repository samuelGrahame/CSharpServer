using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cSharpServer
{
    /// <summary>
    /// No Currently used. can help getting data.
    /// </summary>
    public class ClientArgument
    {
        public string Name;        
        public object Value;
        public byte Type;

        public int GetInt()
        {
            return (int)Value;
        }

        public long GetLong()
        {
            return (long)Value;
        }

        public short GetShort()
        {
            return (short)Value;
        }

        public string GetString()
        {
            return (string)Value;
        }

        public float GetFloat()
        {
            return (float)Value;
        }

        public double GetDouble()
        {
            return (double)Value;
        }

        public decimal GetDecimal()
        {
            return (decimal)Value;
        }

        public DateTime GetDateTime()
        {
            return (DateTime)Value;
        }

        public byte[] GetBlob()
        {
            return (byte[])Value;
        }

        public object GetObject()
        {
            return Value;
        }

        public bool IsNull()
        {
            return Value == null;
        }

        public ClientArgument(string name, byte type, byte[] data)
        {
            this.Name = name;
            this.Type = type;

            switch(type)
            {
                case 0: // byte
                    Value = data[0];
                    break;
                case 1: // short
                    Value = BitConverter.ToInt16(data, 0);
                    break;
                case 2: // int
                    Value = BitConverter.ToInt32(data, 0);
                    break;
                case 3: // long
                    Value = BitConverter.ToInt64(data, 0);
                    break;
                case 4: // float
                    Value = BitConverter.ToSingle(data, 0);
                    break;
                case 5: // double
                    Value = BitConverter.ToDouble(data, 0);
                    break;
                case 6: // decimal
                    Value = new decimal(new int[] {BitConverter.ToInt32(data, 0), BitConverter.ToInt32(data, 4), BitConverter.ToInt32(data, 8), BitConverter.ToInt32(data, 12)});
                    break;
                case 7: // datetime
                    Value = DateTime.FromBinary(BitConverter.ToInt64(data, 0));                    
                    break;
                case 8: // string
                    Value = Encoding.Default.GetString(data);
                    break;
                case 9: // blob
                    Value = data;
                    break;
                default: // null
                    this.Type = 10;
                    Value = null;
                    break;
            }
        }
    }
}
