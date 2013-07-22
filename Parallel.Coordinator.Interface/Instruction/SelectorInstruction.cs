using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Parallel.Worker.Interface.Instruction;

namespace Parallel.Coordinator.Interface.Instruction
{
    /// <summary>
    /// Instruction for easily modifying arguments
    /// </summary>
    /// <typeparam name="TArgument"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    public class SelectorInstruction<TArgument, TResult> : LocalInstruction<TArgument, TResult>
        where TArgument : class
        where TResult : class
    {
        public SelectorInstruction(Func<TArgument, TResult> function) : base(Wrapper.Wrap(function)) {}
    }
}
