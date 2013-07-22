using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Parallel.Worker.Interface.Instruction;

namespace Parallel.Coordinator.Interface.Instruction
{
    /// <summary>
    /// Instruction that discards any input, but produces it's own result
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    public class GeneratorInstruction<TResult> : SelectorInstruction<object, TResult>
        where TResult : class
    {
        public GeneratorInstruction(Func<TResult> generate) : base(Wrapper.Wrap(generate))
        {
        }
    }
}
