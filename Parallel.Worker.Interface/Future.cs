using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Parallel.Worker.Interface.Events;

namespace Parallel.Worker.Interface
{
    public class Future : Task
    {
        private readonly CancellationTokenSource _cancellationTokenSource;

        #region constructors
        protected Future(Action action, CancellationTokenSource cancellationTokenSource)
            : base(action, cancellationTokenSource.Token)
        {
            _cancellationTokenSource = cancellationTokenSource;
        }

        protected Future(Action action, CancellationTokenSource cancellationTokenSource,
                         TaskCreationOptions creationOptions)
            : base(action, cancellationTokenSource.Token, creationOptions)
        {
            _cancellationTokenSource = cancellationTokenSource;
        }

        protected Future(Action<object> action, object state, CancellationTokenSource cancellationTokenSource)
            : base(action, state, cancellationTokenSource.Token)
        {
            _cancellationTokenSource = cancellationTokenSource;
        }

        protected Future(Action<object> action, object state, CancellationTokenSource cancellationTokenSource,
                         TaskCreationOptions creationOptions)
            : base(action, state, cancellationTokenSource.Token, creationOptions)
        {
            _cancellationTokenSource = cancellationTokenSource;
        }
        #endregion

        #region factory methods
        public static Future Create(Action<CancellationToken, Action> action, Action reportProgress)
        {
            var cts = new CancellationTokenSource();
            return new Future(() => action(cts.Token, reportProgress), cts);
        }

        public static Future Create(Action<CancellationToken, Action> action, Action reportProgress, TaskCreationOptions creationOptions)
        {
            var cts = new CancellationTokenSource();
            return new Future(() => action(cts.Token, reportProgress), cts, creationOptions);
        }

        public static Future Create(Action<CancellationToken, Action, object> action, Action reportProgress, object state)
        {
            var cts = new CancellationTokenSource();
            return new Future(s => action(cts.Token, reportProgress, s), state, cts);
        }

        public static Future Create(Action<CancellationToken, Action, object> action, Action reportProgress, object state, TaskCreationOptions creationOptions)
        {
            var cts = new CancellationTokenSource();
            return new Future(s => action(cts.Token, reportProgress, s), state, cts, creationOptions);
        }
        #endregion

        /// <summary>
        /// Returns true if IsCompleted or IsFaulted
        /// </summary>
        public bool IsDone {
            get { return IsCompleted || IsFaulted; }
        }

        /// <summary>
        /// never throws errors from inner operation
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

    /// <summary>
    /// Implementation for operations that report progress with a value (not just hearbeats).
    /// Name must be different from `Future&lt;TProgress&gt;` since that clashes with `Future&lt;TResult&gt;`
    /// </summary>
    /// <typeparam name="TProgress"></typeparam>
    public class FutureProgress<TProgress> : Future
        where TProgress : class
    {
        #region constructors
        protected FutureProgress(Action action, CancellationTokenSource cancellationTokenSource)
            : base(action, cancellationTokenSource)
        {
        }

        protected FutureProgress(Action action, CancellationTokenSource cancellationTokenSource,
                                 TaskCreationOptions creationOptions)
            : base(action, cancellationTokenSource, creationOptions)
        {
        }

        protected FutureProgress(Action<object> action, object state,
                                 CancellationTokenSource cancellationTokenSource)
            : base(action, state, cancellationTokenSource)
        {
        }

        protected FutureProgress(Action<object> action, object state,
                                 CancellationTokenSource cancellationTokenSource,
                                 TaskCreationOptions creationOptions)
            : base(action, state, cancellationTokenSource, creationOptions)
        {
        }
        #endregion

        #region factory methods
        public static FutureProgress<TProgress> Create(Action<CancellationToken, Action<TProgress>> action, Action<TProgress> reportProgress)
        {
            var cts = new CancellationTokenSource();
            return new FutureProgress<TProgress>(() => action(cts.Token, reportProgress), cts);
        }

        public static FutureProgress<TProgress> Create(Action<CancellationToken, Action<TProgress>> action, Action<TProgress> reportProgress, TaskCreationOptions creationOptions)
        {
            var cts = new CancellationTokenSource();
            return new FutureProgress<TProgress>(() => action(cts.Token, reportProgress), cts, creationOptions);
        }

        public static FutureProgress<TProgress> Create(Action<CancellationToken, Action<TProgress>, object> action, Action<TProgress> reportProgress, object state)
        {
            var cts = new CancellationTokenSource();
            return new FutureProgress<TProgress>(s => action(cts.Token, reportProgress, s), state, cts);
        }

        public static FutureProgress<TProgress> Create(Action<CancellationToken, Action<TProgress>, object> action, Action<TProgress> reportProgress, object state, TaskCreationOptions creationOptions)
        {
            var cts = new CancellationTokenSource();
            return new FutureProgress<TProgress>(s => action(cts.Token, reportProgress, s), state, cts, creationOptions);
        }
        #endregion
    }

    public class Future<TResult> : Task<TResult>
    {
        private readonly CancellationTokenSource _cancellationTokenSource;

        #region constructors
        protected Future(Func<TResult> function, CancellationTokenSource cancellationTokenSource)
            : base(function, cancellationTokenSource.Token)
        {
            _cancellationTokenSource = cancellationTokenSource;
        }

        protected Future(Func<TResult> function, CancellationTokenSource cancellationTokenSource,
                         TaskCreationOptions creationOptions)
            : base(function, cancellationTokenSource.Token, creationOptions)
        {
            _cancellationTokenSource = cancellationTokenSource;
        }

        protected Future(Func<object, TResult> function, object state,
                         CancellationTokenSource cancellationTokenSource)
            : base(function, state, cancellationTokenSource.Token)
        {
            _cancellationTokenSource = cancellationTokenSource;
        }

        protected Future(Func<object, TResult> function, object state,
                         CancellationTokenSource cancellationTokenSource,
                         TaskCreationOptions creationOptions)
            : base(function, state, cancellationTokenSource.Token, creationOptions)
        {
            _cancellationTokenSource = cancellationTokenSource;
        }
        #endregion

        #region factory methods
        public static Future<TResult> Create(Func<CancellationToken, Action, TResult> function, Action reportProgress)
        {
            var cts = new CancellationTokenSource();
            return new Future<TResult>(() => function(cts.Token, reportProgress), cts);
        }

        public static Future<TResult> Create(Func<CancellationToken, Action, TResult> function, Action reportProgress, TaskCreationOptions creationOptions)
        {
            var cts = new CancellationTokenSource();
            return new Future<TResult>(() => function(cts.Token, reportProgress), cts, creationOptions);
        }

        public static Future<TResult> Create(Func<CancellationToken, Action, object, TResult> function, Action reportProgress, object state)
        {
            var cts = new CancellationTokenSource();
            return new Future<TResult>(s => function(cts.Token, reportProgress, s), state, cts);
        }

        public static Future<TResult> Create(Func<CancellationToken, Action, object, TResult> function, Action reportProgress, object state, TaskCreationOptions creationOptions)
        {
            var cts = new CancellationTokenSource();
            return new Future<TResult>(s => function(cts.Token, reportProgress, s), state, cts, creationOptions);
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
        /// never throws errors from inner operation
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
                throw Exception ?? new Exception("State was Faulted, without an Exception");
            else if (IsCompleted)
                return Result;

            //compiler does not see this as dead code
            throw new Exception("State must be Faulted or RanToCompletion");
        }
    }

    /// <summary>
    /// Implementation for operations that report progress with a value (not just hearbeats)
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <typeparam name="TProgress"></typeparam>
    public class Future<TResult, TProgress> : Future<TResult>
        where TProgress : class
    {

        #region constructors
        protected Future(Func<TResult> function, CancellationTokenSource cancellationTokenSource)
            : base(function, cancellationTokenSource)
        {
        }

        protected Future(Func<TResult> function, CancellationTokenSource cancellationTokenSource,
                         TaskCreationOptions creationOptions)
            : base(function, cancellationTokenSource, creationOptions)
        {
        }

        protected Future(Func<object, TResult> function, object state,
                         CancellationTokenSource cancellationTokenSource)
            : base(function, state, cancellationTokenSource)
        {
        }

        protected Future(Func<object, TResult> function, object state,
                         CancellationTokenSource cancellationTokenSource,
                         TaskCreationOptions creationOptions)
            : base(function, state, cancellationTokenSource, creationOptions)
        {
        }
        #endregion

        #region factory methods
        public static new Future<TResult, TProgress> Create(Func<CancellationToken, Action<TProgress>, TResult> function, Action<TProgress> reportProgress)
        {
            var cts = new CancellationTokenSource();
            return new Future<TResult, TProgress>(() => function(cts.Token, reportProgress), cts);
        }

        public static new Future<TResult, TProgress> Create(Func<CancellationToken, Action<TProgress>, TResult> function, Action<TProgress> reportProgress, TaskCreationOptions creationOptions)
        {
            var cts = new CancellationTokenSource();
            return new Future<TResult, TProgress>(() => function(cts.Token, reportProgress), cts, creationOptions);
        }

        public static new Future<TResult, TProgress> Create(Func<CancellationToken, Action<TProgress>, object, TResult> function, Action<TProgress> reportProgress, object state)
        {
            var cts = new CancellationTokenSource();
            return new Future<TResult, TProgress>(s => function(cts.Token, reportProgress, s), state, cts);
        }

        public static new Future<TResult, TProgress> Create(Func<CancellationToken, Action<TProgress>, object, TResult> function, Action<TProgress> reportProgress, object state, TaskCreationOptions creationOptions)
        {
            var cts = new CancellationTokenSource();
            return new Future<TResult, TProgress>(s => function(cts.Token, reportProgress, s), state, cts, creationOptions);
        }
        #endregion
    }
}
