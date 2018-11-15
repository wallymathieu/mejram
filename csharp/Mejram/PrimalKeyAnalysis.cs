using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Mejram.Model;
using System.Linq;
using Mejram.Util;

namespace Mejram
{
	public class PrimalKeyAnalysis
	{
	    private readonly IList<Table> _tables;

	    private readonly IList<ForeignKeyConstraint> _foreignKeys;
	    //                // primal key
        //                if (col.Attributes.Count != 1)
        //                    throw new Exception(
        //                        @"Well, need to rewrite some code below concerning primal keys if this is going to work.
        //If a compounded primal key is needed, maybe you should consider a user defined type.
        //Or perhaps it is because you have forgotten a foreign key constraint?");

		public PrimalKeyAnalysis (IEnumerable<Table> tables, IEnumerable<ForeignKeyConstraint> foreignKeys)
		{
		    _tables = tables.ToList();
		    _foreignKeys = foreignKeys.ToList();
		}
		
		  /// <summary>
        /// Primal keys:
        /// They are defined using foreign keys: The primary key that does not have any foreign key reference.
        /// </summary>
        /// <returns></returns>
        public PrimalKeys PrimalPrimaryKeys()
        {
			var tables = new Dictionary<string, Table>(StringComparer.CurrentCultureIgnoreCase);
			foreach (var table in _tables) 
			{
				tables.Add(table.TableName,table);
			}
			var foreignKeys = new List<ForeignKeyConstraint>(_foreignKeys);
            var primalKeys = new Dictionary<ColumnKey, PrimalKey>(new AttributeComparer());
            foreach (Table tbl in tables.Values)
            {
                // all primary keys that are not part of any foreign keys referencing other tables

                Boolean found = false;
                if (tbl.PrimaryKey == null) continue;

                var primaryKeyConstraintKeys = tbl.PrimaryKey.ConstraintKeys;
                foreach (var key in primaryKeyConstraintKeys)
                {
                    foreach (var con in foreignKeys.Where(fk => fk.TableName == key.TableName))
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
                            var first = primaryKeyConstraintKeys.First();
                            primalKeys.Add(first,
                                           new PrimalKey(tbl.TableName, first));

                            break;
                        default: // several keys
                            Console.WriteLine(
                                $"Warning: Primal key with several keys is currently not supported. Ignoring primal key in {tbl.TableName}");
                            // BUG: This should be handled!
                            break;
                            //throw new Exception("Primal key with several keys is currently not supported: " +
                            //                    tbl.TableName +
                            //                    ", foreignKeys:" + tbl.ForeignKeys +
                            //                    ", primaryKey: " + primaryKeyConstraintKeys);
                    }
                }
            }
			return new PrimalKeys(primalKeys, _tables, _foreignKeys);
        }

		
	}

    public class PrimalKeys
    {
        private readonly IList<Table> _tables;
        private readonly IList<ForeignKeyConstraint> _foreignKeys;
        public ReadOnlyDictionary<ColumnKey, PrimalKey> Keys { get; }

        public PrimalKeys(Dictionary<ColumnKey,PrimalKey> primalKeys,IList<Table> tables, IList<ForeignKeyConstraint> foreignKeys)
        {
            _tables = tables;
            _foreignKeys = foreignKeys;
            Keys = new ReadOnlyDictionary<ColumnKey,PrimalKey>(primalKeys);
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

            if (Keys.ContainsKey(attr))
            {
                className = Keys[attr].ClassTypeName;
                return true;
            }

            #endregion

            #region find the table owning the attribute and then try to find a foreign key pair

            foreach (var f in _foreignKeys.Where(fk => fk.TableName == attr.TableName))
            {
                foreach (var ta in f.ConstraintKeys)
                {
                    if (ta.From.TableName == attr.TableName && ta.From.ColumnName == attr.ColumnName)
                    {
                        #region foreign key found

                        var foreignKeyV = new ColumnKey
                            (ta.To.TableName, ta.To.ColumnName);

                        if (Keys.ContainsKey(foreignKeyV))
                        {
                            className = Keys[foreignKeyV].ClassTypeName;
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
    }
}

