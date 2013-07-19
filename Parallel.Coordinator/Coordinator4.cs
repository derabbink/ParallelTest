using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading;
using Parallel.Coordinator.Instruction;
using Parallel.Coordinator.Interface;
using Parallel.Coordinator.Interface.Instruction;

namespace Parallel.Coordinator
{
    public static class Coordinator4
    {
        public static Coordinator<TArgument, Tuple<TResult1, TResult2, TResult3, TResult4>> Do<TArgument, TResult1, TResult2, TResult3, TResult4>
            (CoordinatedInstruction<TArgument, TResult1> instruction1,
             CoordinatedInstruction<TArgument, TResult2> instruction2,
             CoordinatedInstruction<TArgument, TResult3> instruction3,
             CoordinatedInstruction<TArgument, TResult4> instruction4,
             TArgument argument)
            where TArgument : class
            where TResult1 : class
            where TResult2 : class
            where TResult3 : class
            where TResult4 : class
        {
            return Interface.Coordinator.Do(ComposeInstruction(instruction1, instruction2, instruction3, instruction4), argument);
        }

        public static Coordinator<TArgument, Tuple<TResult1, TResult2, TResult3, TResult4>> Do<TArgument, TResult1, TResult2, TResult3, TResult4>
            (CancellationToken cancellationToken,
             CoordinatedInstruction<TArgument, TResult1> instruction1,
             CoordinatedInstruction<TArgument, TResult2> instruction2,
             CoordinatedInstruction<TArgument, TResult3> instruction3,
             CoordinatedInstruction<TArgument, TResult4> instruction4,
             TArgument argument)
            where TArgument : class
            where TResult1 : class
            where TResult2 : class
            where TResult3 : class
            where TResult4 : class
        {
            return Interface.Coordinator.Do(cancellationToken, ComposeInstruction(instruction1, instruction2, instruction3, instruction4), argument);
        }

        public static Coordinator<TResult, Tuple<TNextResult1, TNextResult2, TNextResult3, TNextResult4>> ThenDo
            <TArgument, TResult, TNextResult1, TNextResult2, TNextResult3, TNextResult4>
            (this Coordinator<TArgument, TResult> coordinator,
             CoordinatedInstruction<TResult, TNextResult1> instruction1,
             CoordinatedInstruction<TResult, TNextResult2> instruction2,
             CoordinatedInstruction<TResult, TNextResult3> instruction3,
             CoordinatedInstruction<TResult, TNextResult4> instruction4)
            where TArgument : class
            where TResult : class
            where TNextResult1 : class
            where TNextResult2 : class
            where TNextResult3 : class
            where TNextResult4 : class
        {
            return coordinator.ThenDo(ComposeInstruction(instruction1, instruction2, instruction3, instruction4));
        }

        private static LocalInstruction<TArgument, Tuple<TResult1, TResult2, TResult3, TResult4>> ComposeInstruction
            <TArgument, TResult1, TResult2, TResult3, TResult4>
            (CoordinatedInstruction<TArgument, TResult1> instruction1,
             CoordinatedInstruction<TArgument, TResult2> instruction2,
             CoordinatedInstruction<TArgument, TResult3> instruction3,
             CoordinatedInstruction<TArgument, TResult4> instruction4)
            where TArgument : class
            where TResult1 : class
            where TResult2 : class
            where TResult3 : class
            where TResult4 : class
        {
            return new ParallelInstruction<TArgument, TResult1, TResult2, TResult3, TResult4>(instruction1, instruction2, instruction3, instruction4);
        }
    }
}
