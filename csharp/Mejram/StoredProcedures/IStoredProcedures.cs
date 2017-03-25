using Mejram.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mejram.StoredProcedures
{
    public interface IStoredProcedures
    {
        string GetRoutineDefinition(Routine routine);
        IEnumerable<Routine> GetRoutines();
    }
}
