using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace cSharpServer
{
    public class BinaryBuffer
    {
        private const string Str0001 = "You are at the End of File!";
        private const string Str0002 = "You are Not Reading from the Buffer!";
        private const string Str0003 = "You are Currenlty Writing to the Buffer!";
        private const string Str0004 = "You are Currenlty Reading from the Buffer!";
        private const string Str0005 = "You are Not Writing to the Buffer!";
        private const string Str0006 = "You are trying to Reverse Seek, Unable to add a Negative value!";
        private bool _inRead;
        private bool _inWrite;
        private List<byte> _newBytes;
        private int _pointer;
        public byte[] ByteBuffer;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
        {
            return Helper.DefaultEncoding.GetString(ByteBuffer, 0, ByteBuffer.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BinaryBuffer(string data)
            : this(Helper.DefaultEncoding.GetBytes(data))
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BinaryBuffer()
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BinaryBuffer(byte[] data, bool openRead = false)
            : this(ref data)
        {
            if(openRead)
            {
                this.BeginRead();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BinaryBuffer(ref byte[] data)
        {
            ByteBuffer = data;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncrementPointer(int add)
        {
            if (add < 0)
            {
                throw new Exception(Str0006);
            }
            _pointer += add;
            if (EofBuffer())
            {
                throw new Exception(Str0001);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetPointer()
        {
            return _pointer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetString(ref byte[] buffer)
        {
            return Helper.DefaultEncoding.GetString(buffer, 0, buffer.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetString(byte[] buffer)
        {
            return GetString(ref buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void BeginWrite()
        {
            if (_inRead)
            {
                throw new Exception(Str0004);
            }
            _inWrite = true;

            _newBytes = new List<byte>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(float value)
        {
            if (!_inWrite)
            {
                throw new Exception(Str0005);
            }
            _newBytes.AddRange(BitConverter.GetBytes(value));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(byte value)
        {
            if (!_inWrite)
            {
                throw new Exception(Str0005);
            }
            _newBytes.Add(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(int value)
        {
            if (!_inWrite)
            {
                throw new Exception(Str0005);
            }

            _newBytes.AddRange(BitConverter.GetBytes(value));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(long value)
        {
            if (!_inWrite)
            {
                throw new Exception(Str0005);
            }
            byte[] byteArray = new byte[8];

            unsafe
            {
                fixed (byte* bytePointer = byteArray)
                {
                    *((long*)bytePointer) = value;
                }
            }

            _newBytes.AddRange(byteArray);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int UncommitedLength()
        {
            return _newBytes == null ? 0 : _newBytes.Count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteField(string value)
        {
            Write(value.Length);
            Write(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(string value)
        {
            if (!_inWrite)
            {
                throw new Exception(Str0005);
            }
            byte[] byteArray = Helper.DefaultEncoding.GetBytes(value);
            _newBytes.AddRange(byteArray);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(decimal value)
        {
            if (!_inWrite)
            {
                throw new Exception(Str0005);
            }
            int[] intArray = decimal.GetBits(value);

            Write(intArray[0]);
            Write(intArray[1]);
            Write(intArray[2]);
            Write(intArray[3]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetInt(int value, int pos)
        {
            byte[] byteInt = BitConverter.GetBytes(value);
            for (int i = 0; i < byteInt.Length; i++)
            {
                _newBytes[pos + i] = byteInt[i];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetLong(long value, int pos)
        {
            byte[] byteInt = BitConverter.GetBytes(value);
            for (int i = 0; i < byteInt.Length; i++)
            {
                _newBytes[pos + i] = byteInt[i];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(byte[] value)
        {
            Write(ref value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(ref byte[] value)
        {
            if (!_inWrite)
            {
                throw new Exception(Str0005);
            }
            _newBytes.AddRange(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EndWrite()
        {
            if (ByteBuffer != null)
            {
                _newBytes.InsertRange(0, ByteBuffer);
            }
            ByteBuffer = _newBytes.ToArray();
            _newBytes = null;
            _inWrite = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EndRead()
        {
            _inRead = false;
            _pointer = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void BeginRead()
        {
            if (_inWrite)
            {
                throw new Exception(Str0003);
            }
            _inRead = true;
            _pointer = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte ReadByte()
        {
            if (!_inRead)
            {
                throw new Exception(Str0002);
            }
            if (EofBuffer())
            {
                throw new Exception(Str0001);
            }
            return ByteBuffer[_pointer++];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReadInt()
        {
            if (!_inRead)
            {
                throw new Exception(Str0002);
            }
            if (EofBuffer(4))
            {
                throw new Exception(Str0001);
            }
            int startPointer = _pointer;
            _pointer += 4;

            return BitConverter.ToInt32(ByteBuffer, startPointer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float[] ReadFloatArray()
        {
            float[] dataFloats = new float[ReadInt()];
            for (int i = 0; i < dataFloats.Length; i++)
            {
                dataFloats[i] = ReadFloat();
            }
            return dataFloats;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float ReadFloat()
        {
            if (!_inRead)
            {
                throw new Exception(Str0002);
            }
            if (EofBuffer(sizeof(float)))
            {
                throw new Exception(Str0001);
            }
            int startPointer = _pointer;
            _pointer += sizeof(float);

            return BitConverter.ToSingle(ByteBuffer, startPointer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public decimal ReadDecimal()
        {
            if (!_inRead)
            {
                throw new Exception(Str0002);
            }
            if (EofBuffer(16))
            {
                throw new Exception(Str0001);
            }
            return new decimal(new[] { ReadInt(),
				ReadInt(),
				ReadInt(),
				ReadInt()
			});
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long ReadLong()
        {
            if (!_inRead)
            {
                throw new Exception(Str0002);
            }
            if (EofBuffer(8))
            {
                throw new Exception(Str0001);
            }
            int startPointer = _pointer;
            _pointer += 8;

            return BitConverter.ToInt64(ByteBuffer, startPointer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ReadString(int size)
        {
            return Helper.DefaultEncoding.GetString(ReadByteArray(size), 0, size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte[] ReadByteArray(int size)
        {
            if (!_inRead)
            {
                throw new Exception(Str0002);
            }
            if (EofBuffer(size))
            {
                throw new Exception(Str0001);
            }
            byte[] newBuffer = new byte[size];

            Array.Copy(ByteBuffer, _pointer, newBuffer, 0, size);

            _pointer += size;

            return newBuffer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool EofBuffer(int over = 1)
        {
            return ByteBuffer == null || ((_pointer + over) > ByteBuffer.Length);
        }

        public static DataTable ConvertBlobToDataTable(ref byte[] data)
        {
            DataTable dt = null;

            if (data == null || data.Length == 0)
                return dt;

            BinaryBuffer buff = new BinaryBuffer(data);

            buff.BeginRead();

            buff.ReadInt();

            int ColumnCount = buff.ReadInt();

            if (ColumnCount > 0)
            {
                dt = new DataTable();
                TypeCode[] types = new TypeCode[ColumnCount];
                for (int i = 0; i < ColumnCount; i++)
                {
                    string colName = buff.ReadString(buff.ReadInt());
                    TypeCode type = (TypeCode)buff.ReadByte();
                    types[i] = type;

                    switch (type)
                    {
                        case TypeCode.Boolean:
                            dt.Columns.Add(colName, typeof(Boolean));
                            break;
                        case TypeCode.Byte:
                            dt.Columns.Add(colName, typeof(byte));
                            break;
                        case TypeCode.Char:
                            dt.Columns.Add(colName, typeof(char));
                            break;
                        case TypeCode.DateTime:
                            dt.Columns.Add(colName, typeof(DateTime));
                            break;
                        case TypeCode.DBNull:
                            dt.Columns.Add(colName);
                            break;
                        case TypeCode.Decimal:
                            dt.Columns.Add(colName, typeof(decimal));
                            break;
                        case TypeCode.Double:
                            dt.Columns.Add(colName, typeof(double));
                            break;
                        case TypeCode.Empty:
                            dt.Columns.Add(colName);
                            break;
                        case TypeCode.Int16:
                            dt.Columns.Add(colName, typeof(short));
                            break;
                        case TypeCode.Int32:
                            dt.Columns.Add(colName, typeof(int));
                            break;
                        case TypeCode.Int64:
                            dt.Columns.Add(colName, typeof(long));
                            break;
                        case TypeCode.Single:
                            dt.Columns.Add(colName, typeof(float));
                            break;
                        case TypeCode.String:
                            dt.Columns.Add(colName, typeof(string));
                            break;
                        default:
                            dt.Columns.Add(colName);
                            break;
                    }
                }

                int RowCount = buff.ReadInt();

                dt.BeginLoadData();

                for (int i = 0; i < RowCount; i++)
                {
                    //DataRow dr = dt.NewRow();
                    object[] obj = new object[ColumnCount];
                    for (int y = 0; y < ColumnCount; y++)
                    {
                        TypeCode type = (TypeCode)buff.ReadByte();

                        switch (type)
                        {
                            case TypeCode.Byte:
                            case TypeCode.Boolean:
                                obj[y] = buff.ReadByte();
                                break;
                            case TypeCode.String:
                                obj[y] = buff.ReadString(buff.ReadInt());
                                break;
                            case TypeCode.Int16:
                                obj[y] = BitConverter.ToInt16(buff.ReadByteArray(2), 0);
                                break;
                            case TypeCode.Int32:
                                obj[y] = buff.ReadInt();
                                break;
                            case TypeCode.Int64:
                                obj[y] = buff.ReadLong();
                                break;
                            case TypeCode.Single:
                                obj[y] = buff.ReadFloat();
                                break;
                            case TypeCode.Double:
                                obj[y] = BitConverter.ToDouble(buff.ReadByteArray(8), 0);
                                break;
                            case TypeCode.Decimal:
                                obj[y] = buff.ReadDecimal();
                                break;
                            case TypeCode.DBNull:
                                obj[y] = DBNull.Value;
                                break;
                            case TypeCode.DateTime:
                                obj[y] = DateTime.FromBinary(buff.ReadLong());
                                break;
                            default:
                                obj[y] = null;
                                break;
                        }
                    }
                    dt.Rows.Add(obj);
                }

                dt.EndLoadData();

                if (RowCount > 0)
                    dt.AcceptChanges();
            }

            return dt;
        }

        /// <summary>
        /// Compress data, make sure you check the size. the stream might be compressed already.
        /// </summary>
        /// <param name="raw"></param>
        /// <param name="data"></param>
        public static void Compress(ref byte[] raw, out byte[] data)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                using (GZipStream gzip = new GZipStream(memory,
                CompressionMode.Compress, true))
                {
                    gzip.Write(raw, 0, raw.Length);
                }
                data = memory.ToArray();
            }
        }

        public static void Decompress(ref byte[] gzip, out byte[] data)
        {
            // Create a GZIP stream with decompression mode.
            // ... Then create a buffer and write into while reading from the GZIP stream.
            using (GZipStream stream = new GZipStream(new MemoryStream(gzip), CompressionMode.Decompress))
            {
                const int size = 4096;
                byte[] buffer = new byte[size];
                using (MemoryStream memory = new MemoryStream())
                {
                    int count = 0;
                    do
                    {
                        count = stream.Read(buffer, 0, size);
                        if (count > 0)
                        {
                            memory.Write(buffer, 0, count);
                        }
                    }
                    while (count > 0);
                    data = memory.ToArray();
                }
            }
        }

        /// <summary>
        ///  Datatable to Buffer
        /// </summary>
        /// <param name="result"></param>
        /// <param name="buff"></param>
        public static void ConvertDataTableToBuffer(DataTable result, ref BinaryBuffer buff)
        {            
            DataTable dt = (DataTable)result;
            buff.Write(dt.Columns.Count);
            TypeCode[] types = new TypeCode[dt.Columns.Count];

            for (int i = 0; i < dt.Columns.Count; i++)
            {
                buff.WriteField(dt.Columns[i].ColumnName);
                types[i] = Type.GetTypeCode(dt.Columns[i].DataType);
                buff.Write((byte)types[i]);
            }

            buff.Write(dt.Rows.Count);
            for (int y = 0; y < dt.Rows.Count; y++)
            {
                DataRow dr = dt.Rows[y];
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    // we need to get the typecode, depending on what type code we set the bytes...
                    object value = dr[i];
                    TypeCode type = ((IConvertible)value).GetTypeCode();
                    buff.Write((byte)type);

                    switch (type)
                    {
                        case TypeCode.Byte:
                        case TypeCode.Boolean:
                            buff.Write((byte)value);
                            break;
                        case TypeCode.String:
                            buff.WriteField((string)value);
                            break;
                        case TypeCode.Int16:
                            buff.Write(BitConverter.GetBytes((short)value));
                            break;
                        case TypeCode.Int32:
                            buff.Write((int)value);
                            break;
                        case TypeCode.Int64:
                            buff.Write((long)value);
                            break;
                        case TypeCode.Single:
                            buff.Write((float)value);
                            break;
                        case TypeCode.Double:
                            buff.Write(BitConverter.GetBytes((double)value));
                            break;
                        case TypeCode.Decimal:
                            buff.Write((decimal)value);
                            break;
                        case TypeCode.DBNull:
                            //buff.Write((double)value);
                            break;
                        case TypeCode.DateTime:
                            buff.Write(((DateTime)value).ToBinary());
                            break;
                    }
                }
            }
        }
    }
}
