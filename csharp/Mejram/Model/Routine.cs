using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mejram.Model
{
    [Serializable]
    public class Routine
    {
        public string Name { get; private set; }
        public Routine(string name)
        {
            Name = name;
        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return false;
            }
            if (obj is Routine)
            {
                return Name.Equals((Routine)obj);
            }
            return false;
        }
        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
        public override string ToString()
        {
            return Name.ToString();
        }
    }
}
