using OpenFlow_PluginFramework.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFlow_Core.Primitives.TypeDefinitionManagers
{
    /// <summary>
    /// Defines a constraint which is part of a chain, with a name and a value
    /// </summary>
    public class ValueConstraint<T> : IValueConstraint<T>
    {
        public ValueConstraint()
        {
            MyFunc = x => x;
            TotalFunc = MyFunc;
        }

        /// <summary>
        /// The function which represents the constraint
        /// </summary>
        public Func<T, T> MyFunc { get; set; }

        /// <summary>
        /// The total function which represents the entire chain up until this point
        /// </summary>
        public Func<T, T> TotalFunc { get; private set; }

        /// <summary>
        /// Adds this constraint to the end of a chain of constraints
        /// </summary>
        /// <param name="constraint">The chain of constraints to add this to</param>
        public void AddToEndOfChain(IValueConstraint<T> constraint)
        {
            if (constraint is null)
            {
                return;
            }

            Func<T, T> PreviousTotal = constraint.TotalFunc;
            TotalFunc = (x) => MyFunc(PreviousTotal(x));;
        }
    }
}
