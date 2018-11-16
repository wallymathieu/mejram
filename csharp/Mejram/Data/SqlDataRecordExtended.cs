using System;
using System.Collections.Generic;
using System.Data;

namespace Mejram.Data
{
    public class SqlDataRecordExtended : IDisposable
    {
        private readonly IDataReader _dataReader;
        private readonly Dictionary<string, int> _ordinals;

        public SqlDataRecordExtended(IDataReader reader)
        {
            //
            _dataReader = reader;
            _ordinals = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);
            for (int i = 0; i < reader.FieldCount; i++)
                _ordinals.Add(reader.GetName(i), i);
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (_dataReader != null)
                _dataReader.Dispose();
        }

        #endregion

        public bool Read()
        {
            return _dataReader.Read();
        }


        public int GetInt32(string columnname)
        {
            return _dataReader.GetInt32(_ordinals[columnname]);
        }


        public string GetString(string columnname)
        {
            return _dataReader.IsDBNull(_ordinals[columnname]) ? string.Empty : _dataReader.GetString(_ordinals[columnname]);
        }
    }
}