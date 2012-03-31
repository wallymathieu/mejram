/*  
 This program is licensed under the GNU Lesser General Public License (LGPL).  You should 
 have received a copy of the license along with the source code.  If not, an online copy
 of the license can be found at http://www.gnu.org/copyleft/lesser.html.
*/
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Mejram.Data;
using Mejram.Model;
using Mejram.Util;
using ForeignKeyConstraint = Mejram.Model.ForeignKeyConstraint;
using UniqueConstraint = Mejram.Model.UniqueConstraint;

namespace Mejram
{
    public class DataBaseObjects
    {
        private readonly SqlConnection _conn;

        public Dictionary<ColumnKey, PrimalKey> PrimalKeys =
            new Dictionary<ColumnKey, PrimalKey>(new AttributeComparer());

        public Dictionary<ColumnKey, Column> Columns =
            new Dictionary<ColumnKey, Column>(new AttributeComparer());

        public List<ForeignKeyConstraint> ForeignKeys = new List<ForeignKeyConstraint>();
        public List<UniqueConstraint> UniqueConstraints = new List<UniqueConstraint>();

        public Dictionary<string, Table> Tables = new Dictionary<string, Table>(StringComparer.CurrentCultureIgnoreCase);


        public DataBaseObjects(SqlConnection conn, IEnumerable<ITableFilter> tablesToGenerate,
                               IEnumerable<ITableFilter> columnsToGenerate)
        {
            _conn = conn;
            InitPublicTables(tablesToGenerate, columnsToGenerate);
            InitPrimalPrimaryKeys();
        }

        public DataBaseObjects()
        {
        }

        public Column GetTableAttributeCached(ColumnKey key)
        {
            if (Columns.ContainsKey(key))
                return Columns[key];
            throw new Exception("not found: " + key);
        }

        /// <summary>
        /// Primal keys:
        /// They are defined using foreign keys: The primary key that does not have any foreign key reference.
        /// </summary>
        /// <returns></returns>
        private void InitPrimalPrimaryKeys()
        {
            PrimalKeys = new Dictionary<ColumnKey, PrimalKey>(new AttributeComparer());
            foreach (Table tbl in Tables.Values)
            {
                // all primary keys that are not part of any foreign keys referencing other tables

                Boolean found = false;
                if (tbl.PrimaryKey == null) continue;

                var primaryKeyConstraintKeys = tbl.PrimaryKey.ConstraintKeys;
                foreach (var key in primaryKeyConstraintKeys)
                {
                    foreach (ForeignKeyConstraint con in ForeignKeys.Where(fk => fk.TableName == key.TableName))
                    {
                        foreach (var fkeys in con.ConstraintKeys)
                        {
                            if (fkeys.From.Equals(key))
                            {
                                found = true;
                                break;
                            }
                        }
                    }
                }
                if (!found)
                {
                    switch (primaryKeyConstraintKeys.Count)
                    {
                        case 0:
                            break;
                        case 1:
                            var enumerator =
                                primaryKeyConstraintKeys.GetEnumerator();
                            enumerator.MoveNext();
                            PrimalKeys.Add(enumerator.Current,
                                           new PrimalKey(tbl.TableName, enumerator.Current));

                            break;
                        default: // several keys
                            Console.WriteLine(
                                string.Format(
                                    "Warning: Primal key with several keys is currently not supported. Ignoring primal key in {0}",
                                    tbl.TableName));
                            // BUG: This should be handled!
                            break;
                            //throw new Exception("Primal key with several keys is currently not supported: " +
                            //                    tbl.TableName +
                            //                    ", foreignKeys:" + tbl.ForeignKeys +
                            //                    ", primaryKey: " + primaryKeyConstraintKeys);
                    }
                }
            }
        }

        //                // primal key
        //                if (col.Attributes.Count != 1)
        //                    throw new Exception(
        //                        @"Well, need to rewrite some code below concerning primal keys if this is going to work.
        //If a compounded primal key is needed, maybe you should consider a user defined type.
        //Or perhaps it is because you have forgotten a foreign key constraint?");

        public void GetPublicTableAttributesCached(string tableName,
                                                   IList<Column> attributes)
        {
            using (
                SqlDataRecordExtended dr2 = _conn.ExecuteDataReader(
                    @"
   SELECT distinct c.TABLE_NAME , c.ordinal_position, c.COLUMN_NAME
            FROM INFORMATION_SCHEMA.COLUMNS c  
            INNER JOIN INFORMATION_SCHEMA.TABLES t  ON c.TABLE_NAME = t.TABLE_NAME  
            LEFT JOIN INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE ccu  on c.TABLE_NAME = ccu.TABLE_NAME and c.COLUMN_NAME = ccu.COLUMN_NAME  
            WHERE t.TABLE_TYPE = 'BASE TABLE' and t.table_name = @id
",
                    new {id = tableName}))
            {
                // SqlDataRecordExtended rs2 = new SqlDataRecordExtended(dr2);
                var list = new List<ColumnKey>();
                while (dr2.Read())
                {
                    list.Add(new ColumnKey((string) dr2.GetString("TABLE_NAME"), (string) dr2.GetString("COLUMN_NAME")));
                }
                foreach (Column attr in list.Select(rec => GetTableAttributeCached(rec)))
                {
                    if (attributes.Any(a => a.Key == attr.Key))
                        throw new Exception("Key already found: " + attr.Key.TableName + ", " + attr.Key.ColumnName +
                                            " in table: " +
                                            tableName);
                    attributes.Add(attr);
                }
            }
        }

        public int GetTableCount(string tableName)
        {
            using (
                SqlDataRecordExtended rs2 = _conn.ExecuteDataReader(
                    @"
      SELECT COUNT(*) _count
  FROM [" + tableName + @"]", new {}))
            {
                if (rs2.Read())
                    return rs2.GetInt32("_count");
                return 0;
            }
        }

        public int GetKeyWeight(ForeignKeyConstraint keyConstraint)
        {
            var table = this.Tables[keyConstraint.FromTableName()];
            if (
                keyConstraint.ConstraintKeys.Any(
                    p => !table.Attributes.First(atr => atr.ColumnName == p.From.ColumnName).NotNullConstraint))
                using (
                    SqlDataRecordExtended rs2 =
                        _conn.ExecuteDataReader(
                            string.Format(@"
         SELECT COUNT(*) _count
  FROM  [{0}] WHERE ({1})",
                                          keyConstraint.FromTableName(),
                                          string.Join(" AND ",
                                                      keyConstraint.ConstraintKeys.Select(
                                                          c => c.From.ColumnName + " IS NOT NULL ").ToArray())), new {})
                    )
                {
                    if (rs2.Read())
                        return rs2.GetInt32("_count");
                    return 0;
                }
            else
            {
                return GetTableCount(table.TableName);
            }
        }

        private void GetTableConstraintsCached(string tablename, Table currentTable)
        {
            #region referential constraints

            using (
                SqlDataRecordExtended rs2 = _conn.ExecuteDataReader(
                    @"
SELECT
    con.CONSTRAINT_NAME,
    con.UNIQUE_CONSTRAINT_NAME , --con.*,
    tcon.table_name tcon_table_name,
    pkcon.table_name pkcon_table_name,
    con_col.column_name con_col_column_name,
    pkcon_col.column_name pkcon_col_column_name
FROM 
    INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS con
    JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS tcon ON con.CONSTRAINT_NAME = tcon.CONSTRAINT_NAME
    JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS pkcon ON 
	    con.UNIQUE_CONSTRAINT_NAME = pkcon.CONSTRAINT_NAME
	    AND pkcon.CONSTRAINT_TYPE = 'PRIMARY KEY'
    JOIN INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE con_col ON con_col.CONSTRAINT_NAME = con.CONSTRAINT_NAME
    JOIN INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE pkcon_col ON pkcon_col.CONSTRAINT_NAME = pkcon.CONSTRAINT_NAME

WHERE tcon.table_name = @id
",
                    new {id = tablename}))
            {
                ForeignKeyConstraint currentConstraint = null;

                while (rs2.Read())
                {
                    string constraintName1 = rs2.GetString("CONSTRAINT_NAME");

                    #region if nec. create constraint (class)

                    if (currentConstraint == null || currentConstraint.ConstraintName != constraintName1)
                    {
                        currentConstraint = new ForeignKeyConstraint(constraintName1, currentTable.TableName);
                        ForeignKeys.Add(currentConstraint);
                    }

                    #endregion

                    #region constraint keys

                    var key = new ColumnKey(
                        rs2.GetString("tcon_table_name"),
                        rs2.GetString("con_col_column_name"));

                    var fkey = new ColumnKey(
                        rs2.GetString("pkcon_table_name"),
                        rs2.GetString("pkcon_col_column_name"));

                    currentConstraint.ConstraintKeys.Add(new ForeignKeyColumn(key, fkey));

                    #endregion
                }
            }

            #endregion

            #region primary keys

            using (SqlDataRecordExtended rs2 = _conn.ExecuteDataReader(
                @"
SELECT 
pkcon.constraint_name,
pkcon.table_name, 
pkcon_col.column_name
FROM 
INFORMATION_SCHEMA.TABLE_CONSTRAINTS pkcon 
join INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE pkcon_col on pkcon_col.CONSTRAINT_NAME = pkcon.CONSTRAINT_NAME
/*join INFORMATION_SCHEMA.COLUMNS pkcon_colc on 
	pkcon_colc.table_name = pkcon_col.table_name
	and pkcon_colc.COLUMN_NAME = pkcon_col.COLUMN_NAME*/
WHERE 
--	con.UNIQUE_CONSTRAINT_NAME = pkcon.CONSTRAINT_NAME
pkcon.CONSTRAINT_TYPE = 'PRIMARY KEY'
AND pkcon.table_name = @id
",
                new {id = tablename}))
            {
                //_conn.ExecuteDataReader("sp_jgljsdklfjsdkljf", new List<object>(){
                //        "param1", 1,
                //        "param2", 2,
                //        "param3", 3});
                PrimaryKeyConstraint currentConstraint = null;

                while (rs2.Read())
                {
                    string conoid = rs2.GetString("CONSTRAINT_NAME");

                    #region if nec. create constraint (class)

                    if (currentConstraint == null || currentConstraint.ConstraintName != conoid)
                    {
                        string constraintName = rs2.GetString("CONSTRAINT_NAME");
                        currentConstraint = new PrimaryKeyConstraint(constraintName, tablename);
                        if (currentTable.PrimaryKey != null) throw new Exception("multiple primary keys??");
                        currentTable.PrimaryKey = currentConstraint;
                    }

                    #endregion

                    #region constraint keys

                    currentConstraint.ConstraintKeys.Add(new ColumnKey(
                                                             rs2.GetString("table_name"),
                                                             rs2.GetString("column_name")));

                    #endregion
                }
            }

            #endregion

            #region unique

#if (true)
            {
                using (SqlDataRecordExtended rs2 = _conn.ExecuteDataReader(
                    @"
select con.CONSTRAINT_NAME,con.UNIQUE_CONSTRAINT_NAME , --con.*,
tcon.table_name tcon_table_name,
pkcon.table_name pkcon_table_name,
con_col.column_name con_col_column_name,
pkcon_col.column_name pkcon_col_column_name
--con_colc.ORDINAL_POSITION con_colc_ORDINAL_POSITION,
--pkcon_colc.ORDINAL_POSITION pkcon_colc_ORDINAL_POSITION
from 
INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS con
join INFORMATION_SCHEMA.TABLE_CONSTRAINTS tcon on con.CONSTRAINT_NAME = tcon.CONSTRAINT_NAME
join INFORMATION_SCHEMA.TABLE_CONSTRAINTS pkcon on 
	con.UNIQUE_CONSTRAINT_NAME = pkcon.CONSTRAINT_NAME
	and pkcon.CONSTRAINT_TYPE = 'PRIMARY KEY'
join INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE con_col on con_col.CONSTRAINT_NAME = con.CONSTRAINT_NAME
join INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE pkcon_col on pkcon_col.CONSTRAINT_NAME = pkcon.CONSTRAINT_NAME
/*join INFORMATION_SCHEMA.COLUMNS con_colc on 
	con_colc.table_name = con_col.table_name
	and con_colc.COLUMN_NAME = con_col.COLUMN_NAME
join INFORMATION_SCHEMA.COLUMNS pkcon_colc on 
	pkcon_colc.table_name = pkcon_col.table_name
	and pkcon_colc.COLUMN_NAME = pkcon_col.COLUMN_NAME*/
where tcon.table_name = @id
",
                    new {id = tablename}))
                {
                    Model.UniqueConstraint currentConstraint = null;

                    while (rs2.Read())
                    {
                        string conoid = rs2.GetString("CONSTRAINT_NAME");
                        if (currentConstraint == null || currentConstraint.ConstraintName != conoid)
                        {
                            string ConstraintName = rs2.GetString("CONSTRAINT_NAME");

                            currentConstraint = new UniqueConstraint(ConstraintName, tablename);
                            UniqueConstraints.Add(currentConstraint);
                        }

                        currentConstraint.ConstraintKeys.Add(new ColumnKey(rs2.GetString("tcon_table_name"),
                                                                           rs2.GetString("con_col_column_name")));
                    }
                }
            }
#endif

            #endregion
        }

        public bool TryGetPrimaryKeyConstraint(ColumnKey attr, out PrimaryKeyConstraint primaryKey)
        {
            primaryKey = Tables[attr.TableName].PrimaryKey.ConstraintKeys.Contains(attr)
                             ? Tables[attr.TableName].PrimaryKey
                             : null;
            return primaryKey != null;
        }

        private static bool GetBoolean(string boolean)
        {
            var valids = new[] {"yes", "true", "1"};
            boolean = boolean == null ? "false" : boolean.ToLower();
            bool result = false;
            foreach (string valid in valids)
            {
                result |= valid.Equals(boolean);
            }
            return result;
        }

        private void InitPublicTables(IEnumerable<ITableFilter> tableFilters,
                                      IEnumerable<ITableFilter> columnsToGenerate)
        {
            {
                var sqlBuffer = new List<string>();
                var bindBuffer = new List<object>();

                foreach (ITableFilter filter in tableFilters) filter.InitSQL("t", sqlBuffer, bindBuffer);

                using (
                    SqlDataRecordExtended rs =
                        _conn.ExecuteDataReader(
                            @"
select table_name from INFORMATION_SCHEMA.TABLES t where 
t.TABLE_TYPE = 'BASE TABLE'
" +
                            StringUtil.Agg(" AND ",
                                           sqlBuffer.ToArray()),
                            bindBuffer.ToArray())
                    )
                {
                    while (rs.Read())
                    {
                        string tableName = rs.GetString("table_name");
                        var currentTable = new Table(tableName);
                        Tables.Add(currentTable.TableName, currentTable);
                    }
                }
            }
            {
                var sqlBuffer = new List<string>();
                var bindBuffer = new List<object>();

                foreach (ITableFilter filter in tableFilters) filter.InitSQL("t", sqlBuffer, bindBuffer);
                foreach (ITableFilter filter in columnsToGenerate) filter.InitSQL("c", sqlBuffer, bindBuffer);

                using (
                    SqlDataRecordExtended rs2 =
                        _conn.ExecuteDataReader(
                            @"
  select c.TABLE_NAME , c.COLUMN_NAME , c.DATA_TYPE,  
             c.CHARACTER_MAXIMUM_LENGTH as Size, c.IS_NULLABLE as IsNullable,  
             c.COLUMN_DEFAULT as DefaultValue,
             c.ordinal_position ,
            t.TABLE_TYPE as TableType  
            from INFORMATION_SCHEMA.COLUMNS c  
            inner join INFORMATION_SCHEMA.TABLES t on c.TABLE_NAME = t.TABLE_NAME  
            where 
t.TABLE_TYPE = 'BASE TABLE'
" +
                            StringUtil.Agg(" AND ",
                                           sqlBuffer.ToArray()),
                            bindBuffer.ToArray())
                    )
                {
                    while (rs2.Read())
                    {
                        var attr = new Column(
                            rs2.GetString("COLUMN_NAME"),
                            rs2.GetString("DATA_TYPE"),
                            (short) rs2.GetInt32("ordinal_position"),
                            rs2.GetString("TABLE_NAME"),
                            !GetBoolean(rs2.GetString("IsNullable")));
                        Columns.Add(attr.Key, attr);
                    }
                }
            }
            foreach (var curr in Tables)
            {
                GetPublicTableAttributesCached(curr.Value.TableName,
                                               curr.Value.Attributes);

                GetTableConstraintsCached(curr.Value.TableName, curr.Value);
            }
            // namespace
        }

        /// <summary>
        /// not safe if the db contains foreign key loops
        /// </summary>
        /// <param name="attr"></param>
        /// <param name="className"></param>
        /// <returns></returns>
        public bool TryGetPrimalKey(ColumnKey attr, out string className)
        {
            #region first try to get primal key

            if (PrimalKeys.ContainsKey(attr))
            {
                className = PrimalKeys[attr].ClassTypeName;
                return true;
            }

            #endregion

            #region find the table owning the attribute and then try to find a foreign key pair

            foreach (ForeignKeyConstraint f in ForeignKeys.Where(fk => fk.TableName == attr.TableName))
            {
                foreach (var ta in f.ConstraintKeys)
                {
                    if (ta.From.TableName == attr.TableName && ta.From.ColumnName == attr.ColumnName)
                    {
                        #region foreign key found

                        var foreignKeyV = new ColumnKey
                            (ta.To.TableName, ta.To.ColumnName);

                        if (PrimalKeys.ContainsKey(foreignKeyV))
                        {
                            className = PrimalKeys[foreignKeyV].ClassTypeName;
                            return true;
                        }
                        // recurse!
                        return TryGetPrimalKey(foreignKeyV, out className);

                        #endregion
                    }
                }
            }

            #endregion

            // primal key not found
            className = null;
            return false;
        }

        #region Nested type: AttributeComparer

        private class AttributeComparer : IEqualityComparer<ColumnKey>
        {
            #region IEqualityComparer<Rec<string,short>> Members

            public bool Equals(ColumnKey x, ColumnKey y)
            {
                return string.Equals(x.TableName, y.TableName, StringComparison.CurrentCultureIgnoreCase)
                       && x.ColumnName.Equals(y.ColumnName);
            }

            public int GetHashCode(ColumnKey obj)
            {
                return obj.TableName.ToLower().GetHashCode() + obj.ColumnName.GetHashCode();
            }

            #endregion
        }

        #endregion

        private readonly string[] _tablePrefix = new[] {"tbl"};

        public String TableNameTrim(string s)
        {
            var v = s.ToLower();
            foreach (var prefix in _tablePrefix)
            {
                if (v.StartsWith(prefix, StringComparison.InvariantCultureIgnoreCase))
                    return v.Remove(0, prefix.Length);
            }
            return v;
        }

        private readonly string[] _keynames = new[] {/*"number",*/ "id" /*, "key"*/};

        public string ColumnExtractTableName(string s)
        {
            var v = s.ToLower();
            v = RegexUtil.ReplaceColumnPrefix(v);
            foreach (var prefix in _tablePrefix)
            {
                if (v.StartsWith(prefix, StringComparison.InvariantCultureIgnoreCase))
                    v = v.Remove(0, prefix.Length);
            }
            foreach (var keyName in _keynames)
            {
                if (v.EndsWith(keyName, StringComparison.InvariantCultureIgnoreCase))
                    return v.Substring(0, v.Length - keyName.Length);
            }
            return v;
        }

        public List<ForeignKeyConstraint> GetProbableForeignKeys()
        {
            //this.Tables.Values.Select(t=>t.ForeignKeys)
            var list = new List<ForeignKeyConstraint>();
            foreach (var table in Tables.Values)
            {
                foreach (var attribute in table.Columns)
                {
                    var _keyname =
                        _keynames.FirstOrDefault(
                            keyname =>
                            attribute.ColumnName.EndsWith(keyname, StringComparison.InvariantCultureIgnoreCase));
                    if (null == _keyname) continue;

                    var other =
                        Tables.Values.Where(
                            t => TableNameTrim(t.TableName) == ColumnExtractTableName(attribute.ColumnName)
                                 && t.TableName != table.TableName);
                    switch (other.Count())
                    {
                        case 0:
                            break;
                        case 1:
                            var firstOther = other.First();

                            var otherTableKeyAttribute = firstOther.Attributes.FirstOrDefault(a =>
                                                                                              RegexUtil.
                                                                                                  ReplaceColumnPrefix(
                                                                                                      a.ColumnName).
                                                                                                  Equals(_keyname,
                                                                                                         StringComparison
                                                                                                             .
                                                                                                             InvariantCultureIgnoreCase));
                            if (null == otherTableKeyAttribute)
                            {
                                Console.WriteLine("Could not find matching key: {0}, other : {1}, column: {2}",
                                                  table.TableName, firstOther.TableName, attribute.Key);
                                continue;
                            }
                            //throw new Exception(string.Format("! {0}_{1}_{2}", table.TableName, first.TableName, attribute.Key));
                            list.Add(new ForeignKeyConstraint(string.Format("{0}_{1}_{2}",
                                                                            table.TableName, firstOther.TableName,
                                                                            attribute.Key.ColumnName), table.TableName)
                                         {
                                             ConstraintKeys = new List<ForeignKeyColumn>()
                                                                  {
                                                                      new ForeignKeyColumn(attribute.Key,
                                                                                           otherTableKeyAttribute.Key)
                                                                  }
                                         });
                            break;
                        default:
                            throw new Exception("other.Count: " + other.Count());
                    }
                }
            }
            return list;
        }
    }
}