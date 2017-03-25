using System;
using System.Collections.Generic;

namespace Mejram.Model
{
    [Serializable]
    public class DataTypes
    {
        private static readonly object[][] dotnetTypes = new[]
                                                             {
                                                                 new object[] {"uniqueidentifier", typeof (Guid)},
                                                                 new object[] {"int", typeof (Int32)},
                                                                 new object[] {"float", typeof (double)},
                                                                 new object[] {"smallint", typeof (Int16)},
                                                                 new object[] {"tinyint", typeof (Byte)},
                                                                 new object[] {"decimal", typeof (Decimal)},
                                                                 new object[] {"numeric", typeof (Decimal)},
                                                                 new object[] {"real", typeof (Single)},
                                                                 new object[] {"bigint", typeof (Int64)},
                                                                 new object[] {"bit", typeof (Boolean)},
                                                                 new object[] {"nvarchar", typeof (String)},
                                                                 new object[] {"varchar", typeof (String)},
                                                                 new object[] {"nchar", typeof (String)},
                                                                 new object[] {"ntext", typeof (String)},
                                                                 new object[] {"text", typeof (String)},
                                                                 new object[] {"char", typeof (String)},
                                                                 new object[] {"datetime", typeof (DateTime)},
                                                                 new object[] {"smalldatetime", typeof (DateTime)},
                                                             };

        private static readonly object[][] ignore = new object[][]
                                                        {
                                                            //new object[] { "ntext", "No direct mapping? Special handling?" } ,
                                                            //new object[] { "text", "No direct mapping? Special handling?" } 
                                                        };

        protected readonly Dictionary<string, Type> typesDict;
        protected Dictionary<string, string> ignoreDict;

        public DataTypes()
        {
            typesDict = getSqlToDotnetDataTypeMap();
            ignoreDict = GetDict<string, string>(ignore);
        }

        // TODO: move
        public static Dictionary<TKey, TValue> GetDict<TKey, TValue>(object[][] val)
        {
            var types = new Dictionary<TKey, TValue>(val.Length);
            for (int i = 0; i < val.Length; i++)
            {
                types.Add((TKey) val[i][0], (TValue) val[i][1]);
            }
            return types;
        }

        public Type GetDataType(Column Attr)
        {
            if (!typesDict.ContainsKey(Attr.ColumnType))
                throw new Exception(string.Format("Well, couldnt find the right type:{2} {1} {0}",
                                                  Attr.ColumnType, Attr.ColumnName,
                                                  Attr.TableName));
            return typesDict[Attr.ColumnType];
        }

        public Type GetDataTypeNullable(Column Attr)
        {
            if (!typesDict.ContainsKey(Attr.ColumnType))
                return null;
            return typesDict[Attr.ColumnType];
        }

        private static Dictionary<string, Type> getSqlToDotnetDataTypeMap()
        {
            return GetDict<string, Type>(dotnetTypes);
            //Dictionary<string, Type> types = new Dictionary<string, Type>();
            //for (int i = 0; i < dotnetTypes.Length; i++)
            //{
            //    types.Add((string) dotnetTypes[i][0], (Type) dotnetTypes[i][1]);
            //}
            //return types;
        }
    }
}