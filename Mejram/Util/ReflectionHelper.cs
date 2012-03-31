using System.Collections.Generic;
using System.ComponentModel;

namespace Mejram.Util
{
    public class ReflectionHelper
    {
        public static IDictionary<string, object> PropertiesToDictionary(object values)
        {
            var obj = new Dictionary<string, object>();
            foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(values))
            {
                object obj2 = descriptor.GetValue(values);
                obj.Add(descriptor.Name, obj2);
            }
            return obj;
        }
    }
}