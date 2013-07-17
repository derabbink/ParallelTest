using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Parallel.Coordinator.Interface.Instruction;
using Parallel.Worker.Interface;
using Parallel.Worker.Interface.Instruction;

namespace Parallel.Coordinator.Interface
{
    public static class Coordinator
    {
        public static Coordinator<TArgument, TResult> Do<TArgument, TResult>(CoordinatedInstruction<TArgument, TResult> instruction,
                                                                             TArgument argument)
            where TArgument : class
            where TResult : class
        {
            var result = new Coordinator<TArgument, TResult>(instruction);
            result.Start(argument);
            return result;
        }
    }

    public class Coordinator<TArgument, TResult>
        where TArgument : class
        where TResult : class
    {
        private CoordinatedInstruction<TArgument, TResult> _instruction;

        private Future<TResult> _result;
        private ManualResetEvent _resultBlock;

        public Coordinator(CoordinatedInstruction<TArgument, TResult> instruction)
        {
            _resultBlock = new ManualResetEvent(false);
            _instruction = instruction;
        }

        /// <summary>
        /// Chains a new Coordinator to the sequential pipeline of coordinators.
        /// this coordinator is then returned
        /// </summary>
        /// <typeparam name="TNextResult"></typeparam>
        /// <param name="instruction">Must consume the result type of the previous instruction</param>
        /// <returns></returns>
        public Coordinator<Future<TResult>, TNextResult> ThenDo<TNextResult>(
                CoordinatedInstruction<Future<TResult>, TNextResult> instruction)
            where TNextResult : class
        {
            var nextCoordinator = new Coordinator<Future<TResult>, TNextResult>(instruction);
            nextCoordinator.Start(Result);
            return nextCoordinator;
        }

        protected internal void Start(TArgument argument)
        {
            _result = _instruction.Invoke(argument);
            _result.Wait();
            _resultBlock.Set();
        }

        /// <summary>
        /// Gets the result of the current execution.
        /// Resulting future IsDone
        /// </summary>
        public Future<TResult> Result
        {
            get
            {
                _resultBlock.WaitOne();
                return _result;
            }
        }
    }
}
