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
    }
}
