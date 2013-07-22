using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parallel.Coordinator.Interface.Instruction
{
    /// <summary>
    /// Instruction that echos its input as the result
    /// </summary>
    /// <typeparam name="TArgument"></typeparam>
    public class IdentityInstruction<TArgument> : SelectorInstruction<TArgument, TArgument>
        where TArgument : class
    {
        public IdentityInstruction() : base(a => a)
        {
        }
    }
}
