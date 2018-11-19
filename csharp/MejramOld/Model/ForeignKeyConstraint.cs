using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Mejram.Model
{
    [Serializable]
    public class ForeignKeyColumn : IEquatable<ForeignKeyColumn>
    {
        /// <summary>
        /// 
        /// </summary>
        public ColumnKey From { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public ColumnKey To { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="x2"></param>
        //[Tested]
        public ForeignKeyColumn(ColumnKey x1, ColumnKey x2)
        {
            this.From = x1;
            this.To = x2;
        }

        public ForeignKeyColumn()
        {
        }

        #region IEquatable<Rec<T1,T2>> Members

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        //[Tested]
        public bool Equals(ForeignKeyColumn other)
        {
            return From.Equals(other.From) && To.Equals(other.To);
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
            return obj is ForeignKeyColumn ? Equals((ForeignKeyColumn) obj) : false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="record1"></param>
        /// <param name="record2"></param>
        /// <returns></returns>
        //[Tested]
        public static bool operator ==(ForeignKeyColumn record1, ForeignKeyColumn record2)
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
        public static bool operator !=(ForeignKeyColumn record1, ForeignKeyColumn record2)
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
            //TODO: don't use 0 as hashcode for null, but something else!
            int hashcode = From.GetHashCode();
            hashcode = hashcode*Hashfactor + (To.GetHashCode());
            return hashcode;
        }

        protected const int Hashfactor = 241;
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("({0}, {1})", From, To);
        }
    }

    [Serializable]
    public class ForeignKeyConstraint : TableConstraint, IEquatable<ForeignKeyConstraint>
    {
        [JsonIgnore] public List<ForeignKeyColumn> ConstraintKeys =
            new List<ForeignKeyColumn>();

        [JsonProperty("ConstraintKeys")]
        public ForeignKeyColumn[] Columns
        {
            get { return ConstraintKeys.ToArray(); }
            set { ConstraintKeys = new List<ForeignKeyColumn>(value); }
        }

        public List<string> TableNames()
        {
            return new List<string> {FromTableName(), ToTableName()};
        }

        public ForeignKeyConstraint(string constraintName, string tablename)
            : base(constraintName, tablename)
        {
        }

        public void Validate()
        {
            if (ConstraintKeys.Any(p => p.From.TableName != this.TableName))
                throw new Exception("Not valid: ConstraintKeys.Any(p=>p.Tables.TableName!=tablename)");
        }

        public ForeignKeyConstraint()
        {
        }

        public string FromTableName()
        {
            return TableName;
        }

        public string ToTableName()
        {
            return ConstraintKeys.First().To.TableName;
        }

        public bool Equals(ForeignKeyConstraint other)
        {
            return this.TableName.Equals(other.TableName) && ConstraintKeys.Count == other.ConstraintKeys.Count
                   && !ConstraintKeys.Any(p => !other.ConstraintKeys.Any(okey => okey.Equals(p)));
        }

        public override bool Equals(object obj)
        {
            return obj is ForeignKeyConstraint ? Equals((ForeignKeyConstraint) obj) : false;
        }

        public override int GetHashCode()
        {
            return (ConstraintKeys != null ? ConstraintKeys.GetHashCode() : 0);
        }
    }
}