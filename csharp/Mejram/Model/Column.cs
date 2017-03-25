using System;
using System.Diagnostics;
using Newtonsoft.Json;

namespace Mejram.Model
{
    [Serializable]
    public class ColumnKey : IEquatable<ColumnKey> //, IShowable
    {
        /// <summary>
        /// readonly 
        /// </summary>
        [JsonProperty()] public String TableName;

        /// <summary>
        /// readonly 
        /// </summary>
        [JsonProperty()] public string ColumnName;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="x2"></param>
        //[Tested]
        public ColumnKey(string x1, string x2)
        {
            TableName = x1;
            ColumnName = x2;
        }

        #region IEquatable<Rec<string,string>> Members

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        //[Tested]
        public bool Equals(ColumnKey other)
        {
            return
                (TableName == null ? other.TableName == null : TableName.Equals(other.TableName)) &&
                (ColumnName == null ? other.ColumnName == null : ColumnName.Equals(other.ColumnName))
                ;
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        //[Tested]
        public override bool Equals(object obj)
        {
            return obj is ColumnKey ? Equals((ColumnKey) obj) : false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="record1"></param>
        /// <param name="record2"></param>
        /// <returns></returns>
        //[Tested]
        public static bool operator ==(ColumnKey record1, ColumnKey record2)
        {
            return record1.Equals(record2);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="record1"></param>
        /// <param name="record2"></param>
        /// <returns></returns>
        //[Tested]
        public static bool operator !=(ColumnKey record1, ColumnKey record2)
        {
            return !record1.Equals(record2);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        //[Tested]
        public override int GetHashCode()
        {
            int hashcode = TableName == null ? 0 : TableName.GetHashCode();
            hashcode = hashcode*Hashfactor + (ColumnName == null ? 0 : ColumnName.GetHashCode());
            return hashcode;
        }

        protected const int Hashfactor = 542;


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("({0}, {1})", TableName, ColumnName);
        }
    }

    [Serializable]
    [DebuggerDisplay("{ColumnName}, {ColumnType}, {AttributeNumber} ")]
    public class Column
    {
        // +attnum
        /// <summary>
        /// oid of owning table, readonly
        /// </summary>
        public string TableName;

        /// <summary>
        /// readonly
        /// </summary>
        public string ColumnName;

        /// <summary>
        /// readonly
        /// </summary>
        public Int16 AttributeNumber;

        /// <summary>
        /// readonly
        /// </summary>
        public string ColumnType;

        /// <summary>
        /// readonly
        /// </summary>
        private ColumnKey Key;
		public ColumnKey GetKey()
		{
			return Key;
		}
		
        /// <summary>
        /// readonly
        /// </summary>
        public bool NotNullConstraint;
       
        public bool HasPrimitiveType;

        public Column(string columnName, string columnType, Int16 attributenumber, string tableName,
                      bool notNullConstraint)
        {
            this.ColumnName = columnName;
            this.ColumnType = columnType;
            this.AttributeNumber = attributenumber;
            this.TableName = tableName;
            this.NotNullConstraint = notNullConstraint;
            Key = new ColumnKey(tableName, columnName);
        }

        public override string ToString()
        {
            return ColumnName;
        }
    }
}