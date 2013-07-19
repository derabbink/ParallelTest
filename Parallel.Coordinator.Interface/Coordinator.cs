using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Parallel.Coordinator.Interface.Instruction;
using Parallel.Worker.Interface;
using Parallel.Worker.Interface.Instruction;

namespace Parallel.Coordinator.Interface
{
    public abstract class Coordinator
    {
        public static Coordinator<TArgument, TResult> Do<TArgument, TResult>(CoordinatedInstruction<TArgument, TResult>
                                                                                 instruction,
                                                                             TArgument argument)
            where TArgument : class
            where TResult : class
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            return Do(cts.Token, instruction, argument);
        }

        public static Coordinator<TArgument, TResult> Do<TArgument, TResult>(CancellationToken cancellationToken,
                                                                             CoordinatedInstruction<TArgument, TResult>
                                                                                 instruction,
                                                                             TArgument argument)
            where TArgument : class
            where TResult : class
        {
            var result = new Coordinator<TArgument, TResult>(cancellationToken, instruction);
            result.Start(argument);
            return result;
        }

        protected internal Coordinator(CancellationToken cancellationToken) : this(cancellationToken, null) {}

        protected internal Coordinator(CancellationToken cancellationToken, Coordinator parent)
        {
            Parent = parent;
            CancellationToken = cancellationToken;
        }

        protected Coordinator Parent { get; private set; }

        protected CancellationToken CancellationToken { get; private set; }

        protected bool HasParent {get { return Parent != null; }}

        protected void ProcessError(Exception e)
        {
            ProcessErrorLocally(e);
            if (HasParent)
                Parent.ProcessError(e);
            else
                throw e;
        }

        protected virtual void ProcessErrorLocally(Exception e)
        {
            //by default, do nothing
        }
    }

    public class Coordinator<TArgument, TResult> : Coordinator
        where TArgument : class
        where TResult : class
    {
        private CoordinatedInstruction<TArgument, TResult> _instruction;

        private Future<TResult> _result;
        private ManualResetEvent _resultBlock;

        protected internal Coordinator(CancellationToken cancellationToken, CoordinatedInstruction<TArgument, TResult> instruction) : this(cancellationToken, instruction, null) { }

        protected internal Coordinator(CancellationToken cancellationToken, CoordinatedInstruction<TArgument, TResult> instruction, Coordinator parent)
            :base(cancellationToken, parent)
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
        public Coordinator<TResult, TNextResult> ThenDo<TNextResult>(
                CoordinatedInstruction<TResult, TNextResult> instruction)
            where TNextResult : class
        {
            CancellationTokenSource nextCts = new CancellationTokenSource();
            CancellationToken.Register(nextCts.Cancel);
            var nextCoordinator = new Coordinator<TResult, TNextResult>(nextCts.Token, instruction, Parent);
            nextCoordinator.Start(Result.Unwrap());
            return nextCoordinator;
        }

        protected internal void Start(TArgument argument)
        {
            WaitOrProcessError(argument);
            _resultBlock.Set();
        }

        private void WaitOrProcessError(TArgument argument)
        {
            try
            {
                _result = _instruction.InvokeAndWait(CancellationToken, argument);
                //tease out any exceptions
                _result.Wait();
            }
            catch (Exception e)
            {
                ProcessError(e);
            }
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
