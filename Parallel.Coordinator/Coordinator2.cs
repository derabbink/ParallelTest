using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Parallel.Coordinator.Instruction;
using Parallel.Coordinator.Interface;
using Parallel.Coordinator.Interface.Instruction;

namespace Parallel.Coordinator
{
    public static class Coordinator2
    {
        public static Coordinator<TArgument, Tuple<TResult1, TResult2>> Do<TArgument, TResult1, TResult2>
            (CoordinatedInstruction<TArgument, TResult1> instruction1,
             CoordinatedInstruction<TArgument, TResult2> instruction2,
             TArgument argument)
            where TArgument : class
            where TResult1 : class
            where TResult2 : class
        {
            return Interface.Coordinator.Do(ComposeInstruction(instruction1, instruction2), argument);
        }

        public static Coordinator<TResult, Tuple<TNextResult1, TNextResult2>> ThenDo
            <TArgument, TResult, TNextResult1, TNextResult2>
            (this Coordinator<TArgument, TResult> coordinator,
             CoordinatedInstruction<TResult, TNextResult1> instruction1,
             CoordinatedInstruction<TResult, TNextResult2> instruction2)
            where TArgument : class
            where TResult : class
            where TNextResult1 : class
            where TNextResult2 : class
        {
            return coordinator.ThenDo(ComposeInstruction(instruction1, instruction2));
        }

        private static LocalInstruction<TArgument, Tuple<TResult1, TResult2>> ComposeInstruction
            <TArgument, TResult1, TResult2>
            (CoordinatedInstruction<TArgument, TResult1> instruction1,
             CoordinatedInstruction<TArgument, TResult2> instruction2)
            where TArgument : class
            where TResult1 : class
            where TResult2 : class
        {
            return new ParallelInstruction<TArgument, TResult1, TResult2>(instruction1, instruction2);
        }
    }
}
