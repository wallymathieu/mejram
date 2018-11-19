using System;
using System.Collections.Generic;
using System.Linq;
using Mejram.Data;
using Mejram.Model;
using Mejram.Util;
using ForeignKeyConstraint = Mejram.Model.ForeignKeyConstraint;
using UniqueConstraint = Mejram.Model.UniqueConstraint;
using System.Data;

namespace Mejram
{
    public class DataBaseObjects
    {
        private readonly IDbConnection _conn;
        private readonly Action<string> onWarn;
        public Dictionary<ColumnKey, Column> Columns =
            new Dictionary<ColumnKey, Column>(new ColumnComparer());

        public List<ForeignKeyConstraint> ForeignKeys = new List<ForeignKeyConstraint>();
        public List<UniqueConstraint> UniqueConstraints = new List<UniqueConstraint>();

        public Dictionary<string, Table> Tables = new Dictionary<string, Table>(StringComparer.CurrentCultureIgnoreCase);

        public DataBaseObjects(IDbConnection conn,
                               IEnumerable<ITableFilter> tablesToGenerate,
                               IEnumerable<ITableFilter> columnsToGenerate,
                               Action<string> onWarn)
        {
            _conn = conn;
            this.onWarn = onWarn;
            var tablefilters = new List<ITableFilter>();
            InitPublicTables(tablefilters.Union(tablesToGenerate ?? new ITableFilter[0]),
                             columnsToGenerate);
        }

        public Column GetTableAttributeCached(ColumnKey key)
        {
            if (Columns.ContainsKey(key))
                return Columns[key];
            throw new Exception("not found: " + key);
        }

        public int GetTableCount(string tableName)
        {
            using (
                SqlDataRecordExtended rs2 = _conn.ExecuteDataReader(
                    @"
      SELECT COUNT(*) _count
  FROM [" + tableName + @"]", new { }))
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
                    p => !table.Columns.First(atr => atr.ColumnName == p.From.ColumnName).NotNullConstraint))
                using (
                    SqlDataRecordExtended rs2 =
                        _conn.ExecuteDataReader(
                            string.Format(@"
         SELECT COUNT(*) _count
  FROM  [{0}] WHERE ({1})",
                                          keyConstraint.FromTableName(),
                                          string.Join(" AND ",
                                                      keyConstraint.ConstraintKeys.Select(
                                                          c => c.From.ColumnName + " IS NOT NULL ").ToArray())), new { })
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
                    new { id = tablename }))
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
                new { id = tablename }))
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
                    new { id = tablename }))
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

        private static bool GetBoolean(string boolean)
        {
            var valids = new[] { "yes", "true", "1" };
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
                            (short)rs2.GetInt32("ordinal_position"),
                            rs2.GetString("TABLE_NAME"),
                            !GetBoolean(rs2.GetString("IsNullable")));
                        if (Columns.ContainsKey(attr.GetKey()))
                        {
                            onWarn($"Duplicate column {attr.GetKey()}");
                        }
                        else
                        {
                            Columns.Add(attr.GetKey(), attr);
                        }
                    }
                }
            }
            foreach (var curr in Tables)
            {
                GetTableConstraintsCached(curr.Value.TableName, curr.Value);
            }
            // namespace
        }
    }
}