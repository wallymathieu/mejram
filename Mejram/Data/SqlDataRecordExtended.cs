/*  
 This program is licensed under the GNU Lesser General Public License (LGPL).  You should 
 have received a copy of the license along with the source code.  If not, an online copy
 of the license can be found at http://www.gnu.org/copyleft/lesser.html.
*/

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.Common;

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

        public bool IsDbNull(string columnname)
        {
            return _dataReader.IsDBNull(_ordinals[columnname]);
        }

        public string GetName(string columnname)
        {
            return _dataReader.GetName(_ordinals[columnname]);
        }

        public string GetDataTypeName(string columnname)
        {
            return _dataReader.GetDataTypeName(_ordinals[columnname]);
        }

        public Type GetFieldType(string columnname)
        {
            return _dataReader.GetFieldType(_ordinals[columnname]);
        }

        public object GetValue(string columnname)
        {
            return _dataReader.GetValue(_ordinals[columnname]);
        }

        public object GetValueNullable(string columnname)
        {
            return _dataReader.IsDBNull(_ordinals[columnname]) ? null : _dataReader.GetValue(_ordinals[columnname]);
        }

        public bool GetBoolean(string columnname)
        {
            return _dataReader.GetBoolean(_ordinals[columnname]);
        }

        public bool? GetBooleanNullable(string columnname)
        {
            return _dataReader.IsDBNull(_ordinals[columnname])
                       ? (bool?) null
                       : _dataReader.GetBoolean(_ordinals[columnname]);
        }

        public byte GetByte(string columnname)
        {
            return _dataReader.GetByte(_ordinals[columnname]);
        }

        public byte? GetByteNullable(string columnname)
        {
            return _dataReader.IsDBNull(_ordinals[columnname]) ? (byte?) null : _dataReader.GetByte(_ordinals[columnname]);
        }

        public char GetChar(string columnname)
        {
            return _dataReader.GetChar(_ordinals[columnname]);
        }

        public char? GetCharNullable(string columnname)
        {
            return _dataReader.IsDBNull(_ordinals[columnname]) ? (char?) null : _dataReader.GetChar(_ordinals[columnname]);
        }

        public Guid GetGuid(string columnname)
        {
            return _dataReader.GetGuid(_ordinals[columnname]);
        }

        public Guid? GetGuidNullable(string columnname)
        {
            return _dataReader.IsDBNull(_ordinals[columnname]) ? (Guid?) null : _dataReader.GetGuid(_ordinals[columnname]);
        }

        public short GetInt16(string columnname)
        {
            return _dataReader.GetInt16(_ordinals[columnname]);
        }

        public short? GetInt16Nullable(string columnname)
        {
            return _dataReader.IsDBNull(_ordinals[columnname]) ? (short?) null : _dataReader.GetInt16(_ordinals[columnname]);
        }

        public int GetInt32(string columnname)
        {
            return _dataReader.GetInt32(_ordinals[columnname]);
        }

        public int? GetInt32Nullable(string columnname)
        {
            return _dataReader.IsDBNull(_ordinals[columnname]) ? (int?) null : _dataReader.GetInt32(_ordinals[columnname]);
        }

        public long GetInt64(string columnname)
        {
            return _dataReader.GetInt64(_ordinals[columnname]);
        }

        public long? GetInt64Nullable(string columnname)
        {
            return _dataReader.IsDBNull(_ordinals[columnname]) ? (long?) null : _dataReader.GetInt64(_ordinals[columnname]);
        }

        public float GetFloat(string columnname)
        {
            return _dataReader.GetFloat(_ordinals[columnname]);
        }

        public float? GetFloatNullable(string columnname)
        {
            return _dataReader.IsDBNull(_ordinals[columnname]) ? (float?) null : _dataReader.GetFloat(_ordinals[columnname]);
        }

        public double GetDouble(string columnname)
        {
            return _dataReader.GetDouble(_ordinals[columnname]);
        }

        public double? GetDoubleNullable(string columnname)
        {
            return _dataReader.IsDBNull(_ordinals[columnname])
                       ? (double?) null
                       : _dataReader.GetDouble(_ordinals[columnname]);
        }

        public string GetString(string columnname)
        {
            return _dataReader.IsDBNull(_ordinals[columnname]) ? string.Empty : _dataReader.GetString(_ordinals[columnname]);
        }

        public string GetStringNullable(string columnname)
        {
            return _dataReader.IsDBNull(_ordinals[columnname]) ? null : _dataReader.GetString(_ordinals[columnname]);
        }

        public decimal GetDecimal(string columnname)
        {
            return _dataReader.GetDecimal(_ordinals[columnname]);
        }

        public decimal? GetDecimalNullable(string columnname)
        {
            return _dataReader.IsDBNull(_ordinals[columnname])
                       ? (decimal?) null
                       : _dataReader.GetDecimal(_ordinals[columnname]);
        }

        public DateTime GetDateTime(string columnname)
        {
            return _dataReader.GetDateTime(_ordinals[columnname]);
        }

        public DateTime? GetDateTimeNullable(string columnname)
        {
            return _dataReader.IsDBNull(_ordinals[columnname])
                       ? (DateTime?) null
                       : _dataReader.GetDateTime(_ordinals[columnname]);
        }

        public Single GetSingle(string columnname)
        {
            return _dataReader.GetFloat(_ordinals[columnname]);
        }

        public Single? GetSingleNullable(string columnname)
        {
            return _dataReader.IsDBNull(_ordinals[columnname])
                       ? (Single?) null
                       : _dataReader.GetFloat(_ordinals[columnname]);
        }

        public IDataReader GetData(string columnname)
        {
            return _dataReader.GetData(_ordinals[columnname]);
        }
    }
}