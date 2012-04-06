
using System;
using System.Collections.Generic;
using Mejram.Model;
using System.Linq;

namespace Mejram
{
	public class PropableForeignKeyAnalysis
	{
		public PropableForeignKeyAnalysis ()
		{
		}
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

		 public List<ForeignKeyConstraint> GetProbableForeignKeys(IEnumerable<Table> Tables)
        {
            //this.Tables.Values.Select(t=>t.ForeignKeys)
            var list = new List<ForeignKeyConstraint>();
            foreach (var table in Tables)
            {
                foreach (var attribute in table.Columns)
                {
                    var _keyname =
                        _keynames.FirstOrDefault(
                            keyname =>
                            attribute.ColumnName.EndsWith(keyname, StringComparison.InvariantCultureIgnoreCase));
                    if (null == _keyname) continue;

                    var other =
                        Tables.Where(
                            t => TableNameTrim(t.TableName) == ColumnExtractTableName(attribute.ColumnName)
                                 && t.TableName != table.TableName);
                    switch (other.Count())
                    {
                        case 0:
                            break;
                        case 1:
                            var firstOther = other.First();

                            var otherTableKeyAttribute = firstOther.Columns.FirstOrDefault(a =>
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
                                                  table.TableName, firstOther.TableName, attribute.GetKey());
                                continue;
                            }
                            //throw new Exception(string.Format("! {0}_{1}_{2}", table.TableName, first.TableName, attribute.Key));
                            list.Add(new ForeignKeyConstraint(string.Format("{0}_{1}_{2}",
                                                                            table.TableName, firstOther.TableName,
                                                                            attribute.GetKey().ColumnName), table.TableName)
                                         {
                                             ConstraintKeys = new List<ForeignKeyColumn>()
                                                                  {
                                                                      new ForeignKeyColumn(attribute.GetKey(),
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
	}
}

