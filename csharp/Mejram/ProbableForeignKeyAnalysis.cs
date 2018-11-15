using System;
using System.Collections.Generic;
using Mejram.Model;
using System.Linq;
using System.Text.RegularExpressions;

namespace Mejram
{
    public class ProbableForeignKeyAnalysis
    {
        private readonly Action<string> _onWarn;

        public ProbableForeignKeyAnalysis(string[] tablePrefixes, string[] keyNames, Action<string> onWarn)
        {
            _onWarn = onWarn;
            _tablePrefix = new[] { "tbl" }.Union(tablePrefixes).ToArray();
            _keynames = new[] {/*"number",*/ "id" /*, "key"*/}.Union(keyNames).ToArray();
        }
        private readonly string[] _tablePrefix;

        private String TableNameTrim(string s)
        {
            var v = s.ToLower();
            foreach (var prefix in _tablePrefix)
            {
                if (v.StartsWith(prefix, StringComparison.InvariantCultureIgnoreCase))
                    return v.Remove(0, prefix.Length);
            }
            return v;
        }

        private readonly string[] _keynames;

        private string ColumnExtractTableName(string s)
        {
            var v = s.ToLower();
            foreach (var keyName in _keynames)
            {
                if (v.EndsWith(keyName, StringComparison.InvariantCultureIgnoreCase))
                    return v.Substring(0, v.Length - keyName.Length);
            }
            return v;
        }

        public List<ForeignKeyConstraint> GetProbableForeignKeys(IEnumerable<Table> tables)
        {
            var tableList = tables.ToList();
            var list = new List<ForeignKeyConstraint>();
            foreach (var table in tableList)
            {
                foreach (var column in table.Columns)
                {
                    var keyName = _keynames.FirstOrDefault(EndsWith(column));
                    if (null == keyName) continue;
                    var other = tableList.Where(TableMatchingColumn(column, table)).ToList();
                    switch (other.Count)
                    {
                        case 0:
                            break;
                        case 1:
                            var firstOther = other.First();

                            var otherTableKeyAttribute = firstOther.Columns.FirstOrDefault(ColumnWithKeyName(keyName));
                            if (null == otherTableKeyAttribute)
                            {
                                _onWarn($"Could not find matching key: {table.TableName}, other : {firstOther.TableName}, column: {column.GetKey()}");
                                continue;
                            }
                            //throw new Exception(string.Format("! {0}_{1}_{2}", table.TableName, first.TableName, attribute.Key));
                            list.Add(new ForeignKeyConstraint(
                                $"{table.TableName}_{firstOther.TableName}_{column.GetKey().ColumnName}", table.TableName)
                            {
                                ConstraintKeys = new List<ForeignKeyColumn>()
                                                                      {
                                                                          new ForeignKeyColumn(column.GetKey(),
                                                                                               otherTableKeyAttribute.GetKey())
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
        private static string ReplaceColumnPrefix(string text)
        {
            return Regex.Replace(text, @"^[a-zA-Z]{2,4}_", "");
        }
        private static Func<Column, bool> ColumnWithKeyName(string keyName)
        {
            return a => ReplaceColumnPrefix(a.ColumnName)
                       .Equals(keyName, StringComparison.InvariantCultureIgnoreCase);
        }

        private Func<Table, bool> TableMatchingColumn(Column attribute, Table table)
        {
            return t => TableNameTrim(t.TableName) == ColumnExtractTableName(attribute.ColumnName)
                        && t.TableName != table.TableName;
        }

        private static Func<string, bool> EndsWith(Column attribute)
        {
            return k =>
                attribute.ColumnName.EndsWith(k, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}

