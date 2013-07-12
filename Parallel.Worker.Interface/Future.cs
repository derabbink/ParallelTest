using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Parallel.Worker.Interface
{
    public class Future : Task
    {
        private readonly CancellationTokenSource _cancellationTokenSource;

        #region constructors
        private Future(Action action, CancellationTokenSource cancellationTokenSource)
            : base(action, cancellationTokenSource.Token)
        {
            _cancellationTokenSource = cancellationTokenSource;
        }

        private Future(Action action, CancellationTokenSource cancellationTokenSource,
                       TaskCreationOptions creationOptions)
            : base(action, cancellationTokenSource.Token, creationOptions)
        {
            _cancellationTokenSource = cancellationTokenSource;
        }

        private Future(Action<object> action, object state, CancellationTokenSource cancellationTokenSource)
            : base(action, state, cancellationTokenSource.Token)
        {
            _cancellationTokenSource = cancellationTokenSource;
        }

        private Future(Action<object> action, object state, CancellationTokenSource cancellationTokenSource,
                       TaskCreationOptions creationOptions)
            : base(action, state, cancellationTokenSource.Token, creationOptions)
        {
            _cancellationTokenSource = cancellationTokenSource;
        }
        #endregion

        #region factory methods
        public static Future Create(Action<CancellationToken> action)
        {
            var cts = new CancellationTokenSource();
            return new Future(() => action(cts.Token), cts);
        }

        public static Future Create(Action<CancellationToken> action, TaskCreationOptions creationOptions)
        {
            var cts = new CancellationTokenSource();
            return new Future(() => action(cts.Token), cts, creationOptions);
        }

        public static Future Create(Action<CancellationToken, object> action, object state)
        {
            var cts = new CancellationTokenSource();
            return new Future(s => action(cts.Token, s), state, cts);
        }

        public static Future Create(Action<CancellationToken, object> action, object state, TaskCreationOptions creationOptions)
        {
            var cts = new CancellationTokenSource();
            return new Future(s => action(cts.Token, s), state, cts, creationOptions);
        }
        #endregion

        /// <summary>
        /// Returns true if IsCompleted or IsFaulted
        /// </summary>
        public bool IsDone {
            get { return IsCompleted || IsFaulted; }
        }

        /// <summary>
        /// never throws
        /// </summary>
        public new void Wait()
        {
            try
            {
                base.Wait();
            }
            catch {}
        }

        public static void WaitAll(IEnumerable<Future> futures)
        {
            foreach (Future f in futures)
                f.Wait();
        }

        public void Cancel()
        {
            _cancellationTokenSource.Cancel();
        }

        public static void CancelAll(IEnumerable<Future> futures)
        {
            foreach (Future f in futures)
                f.Cancel();
        }

        public void Unwrap()
        {
            if (IsFaulted)
            {
                if (Exception != null)
                    throw Exception;
                else
                    throw new Exception("State was Faulted, without an Exception");
            }
        }
    }

    public class Future<TResult> : Task<TResult>
    {
        private readonly CancellationTokenSource _cancellationTokenSource;

        #region constructors

        private Future(Func<TResult> function, CancellationTokenSource cancellationTokenSource)
            : base(function, cancellationTokenSource.Token)
        {
            _cancellationTokenSource = cancellationTokenSource;
        }

        private Future(Func<TResult> function, CancellationTokenSource cancellationTokenSource,
                       TaskCreationOptions creationOptions)
            : base(function, cancellationTokenSource.Token, creationOptions)
        {
            _cancellationTokenSource = cancellationTokenSource;
        }

        private Future(Func<object, TResult> function, object state, CancellationTokenSource cancellationTokenSource)
            : base(function, state, cancellationTokenSource.Token)
        {
            _cancellationTokenSource = cancellationTokenSource;
        }

        private Future(Func<object, TResult> function, object state, CancellationTokenSource cancellationTokenSource,
                       TaskCreationOptions creationOptions)
            : base(function, state, cancellationTokenSource.Token, creationOptions)
        {
            _cancellationTokenSource = cancellationTokenSource;
        }
        #endregion

        #region factory methods
        public static Future<TResult> Create(Func<CancellationToken, TResult> function)
        {
            var cts = new CancellationTokenSource();
            return new Future<TResult>(() => function(cts.Token), cts);
        }

        public static Future<TResult> Create(Func<CancellationToken, TResult> function, TaskCreationOptions creationOptions)
        {
            var cts = new CancellationTokenSource();
            return new Future<TResult>(() => function(cts.Token), cts, creationOptions);
        }

        public static Future<TResult> Create(Func<CancellationToken, object, TResult> function, object state)
        {
            var cts = new CancellationTokenSource();
            return new Future<TResult>(s => function(cts.Token, s), state, cts);
        }

        public static Future<TResult> Create(Func<CancellationToken, object, TResult> function, object state, TaskCreationOptions creationOptions)
        {
            var cts = new CancellationTokenSource();
            return new Future<TResult>(s => function(cts.Token, s), state, cts, creationOptions);
        }
        #endregion

        /// <summary>
        /// Returns true if IsCompleted or IsFaulted
        /// </summary>
        public bool IsDone
        {
            get { return IsCompleted || IsFaulted; }
        }

        /// <summary>
        /// never throws
        /// </summary>
        public new void Wait()
        {
            try
            {
                base.Wait();
            }
            catch { }
        }

        public static void WaitAll(IEnumerable<Future<TResult>> futures)
        {
            foreach (Future<TResult> f in futures)
                f.Wait();
        }

        public void Cancel()
        {
            _cancellationTokenSource.Cancel();
        }

        public static void CancelAll(IEnumerable<Future<TResult>> futures)
        {
            foreach (Future<TResult> f in futures)
                f.Cancel();
        }

        public TResult Unwrap()
        {
            Contract.Requires(IsDone, "State must be Faulted or RanToCompletion");

            if (IsFaulted)
            {
                if (Exception != null)
                    throw Exception;
                else
                    throw new Exception("State was Faulted, without an Exception");
            }
            else if (IsCompleted)
                return Result;

            //compiler does not see this as dead code
            throw new Exception("State must be Faulted or RanToCompletion");
        }
    }
}
