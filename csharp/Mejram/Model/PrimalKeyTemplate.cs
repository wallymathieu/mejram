using System;

namespace Mejram.Model
{
    [Serializable]
    public abstract class PrimalKeyTemplate
    {
        public abstract Object Key { get; }

        public override string ToString()
        {
            return Key.ToString();
        }
    }
}