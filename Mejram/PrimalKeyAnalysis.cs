using System;
using System.Collections.Generic;
using Mejram.Model;
using System.Linq;

namespace Mejram
{
	public class PrimalKeyAnalysis
	{      
		//                // primal key
        //                if (col.Attributes.Count != 1)
        //                    throw new Exception(
        //                        @"Well, need to rewrite some code below concerning primal keys if this is going to work.
        //If a compounded primal key is needed, maybe you should consider a user defined type.
        //Or perhaps it is because you have forgotten a foreign key constraint?");

		private Dictionary<string, Table> Tables ;
		private List<ForeignKeyConstraint> ForeignKeys;
		public PrimalKeyAnalysis (IEnumerable<Table> tables, IEnumerable<ForeignKeyConstraint> foreignKeys)
		{
			Tables = new Dictionary<string, Table>(StringComparer.CurrentCultureIgnoreCase);
			foreach (var table in tables) 
			{
				Tables.Add(table.TableName,table);
			}
			this.ForeignKeys = new List<ForeignKeyConstraint>(foreignKeys);
		}
		
		  /// <summary>
        /// Primal keys:
        /// They are defined using foreign keys: The primary key that does not have any foreign key reference.
        /// </summary>
        /// <returns></returns>
        public Dictionary<ColumnKey, PrimalKey> PrimalPrimaryKeys()
        {
            var PrimalKeys = new Dictionary<ColumnKey, PrimalKey>(new AttributeComparer());
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
			return PrimalKeys;
        }
		public bool TryGetPrimaryKeyConstraint(ColumnKey attr, out PrimaryKeyConstraint primaryKey)
        {
            primaryKey = Tables[attr.TableName].PrimaryKey.ConstraintKeys.Contains(attr)
                             ? Tables[attr.TableName].PrimaryKey
                             : null;
            return primaryKey != null;
        }
		
        /// <summary>
        /// not safe if the db contains foreign key loops
        /// </summary>
        /// <param name="attr"></param>
        /// <param name="className"></param>
        /// <returns></returns>
        public bool TryGetPrimalKey(Dictionary<ColumnKey, PrimalKey> PrimalKeys, ColumnKey attr, out string className)
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
                        return TryGetPrimalKey(PrimalKeys, foreignKeyV, out className);

                        #endregion
                    }
                }
            }

            #endregion

            // primal key not found
            className = null;
            return false;
        }
	}
}

