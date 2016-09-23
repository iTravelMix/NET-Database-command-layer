namespace ADO.Query.Test.AdoMocks
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;

    public class MockDataReader : IDataReader
    {
        private int rowCounter = -1;
        private readonly IList<IDictionary<string, object>> records;

        public MockDataReader(IList<IDictionary<string, object>> records)
        {
            this.records = records;
        }

        public void Close()
        {
        }

        public DataTable GetSchemaTable()
        {
            throw new NotImplementedException();
        }

        public bool NextResult()
        {
            throw new NotImplementedException();
        }

        public bool Read()
        {
            this.rowCounter++;
            return this.rowCounter < this.records.Count;
        }

        public int Depth { get; private set; }

        public bool IsClosed { get; private set; }

        public int RecordsAffected { get; private set; }

        public string GetName(int i)
        {
            return this.records[this.rowCounter].Keys.ElementAt(i);
        }

        public string GetDataTypeName(int i)
        {
            throw new NotImplementedException();
        }

        public Type GetFieldType(int i)
        {
            throw new NotImplementedException();
        }

        public object GetValue(int i)
        {
            throw new NotImplementedException();
        }

        public int GetValues(object[] values)
        {
            throw new NotImplementedException();
        }

        public int GetOrdinal(string name)
        {
            throw new NotImplementedException();
        }

        public bool GetBoolean(int i)
        {
            throw new NotImplementedException();
        }

        public byte GetByte(int i)
        {
            throw new NotImplementedException();
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public char GetChar(int i)
        {
            throw new NotImplementedException();
        }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public Guid GetGuid(int i)
        {
            throw new NotImplementedException();
        }

        public short GetInt16(int i)
        {
            throw new NotImplementedException();
        }

        public int GetInt32(int i)
        {
            throw new NotImplementedException();
        }

        public long GetInt64(int i)
        {
            throw new NotImplementedException();
        }

        public float GetFloat(int i)
        {
            throw new NotImplementedException();
        }

        public double GetDouble(int i)
        {
            throw new NotImplementedException();
        }

        public string GetString(int i)
        {
            throw new NotImplementedException();
        }

        public decimal GetDecimal(int i)
        {
            throw new NotImplementedException();
        }

        public DateTime GetDateTime(int i)
        {
            throw new NotImplementedException();
        }

        public IDataReader GetData(int i)
        {
            throw new NotImplementedException();
        }

        public bool IsDBNull(int i)
        {
            throw new NotImplementedException();
        }

        public int FieldCount
        {
            get
            {
                return this.records[this.rowCounter].Keys.Count;
            }
        }

        object IDataRecord.this[int i]
        {
            get
            {
                return this.records[this.rowCounter].Values.ElementAt(i);
            }
        }

        public object this[string name]
        {
            get { return this.records[this.rowCounter][name]; }
        }

        public void Dispose()
        {
        }
    }
}
