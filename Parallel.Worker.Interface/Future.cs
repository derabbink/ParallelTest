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
    public class Future : Task, IProgress
    {
        private readonly CancellationTokenSource _cancellationTokenSource;

        public event EventHandler<ProgressEventArgs> ProgressChanged;

        #region constructors
        protected Future(Action action, CancellationTokenSource cancellationTokenSource, Progress progress)
            : base(action, cancellationTokenSource.Token)
        {
            progress.ProgressChanged += (_, args) => OnProgress();
            _cancellationTokenSource = cancellationTokenSource;
        }

        protected Future(Action action, CancellationTokenSource cancellationTokenSource, Progress progress,
                                  TaskCreationOptions creationOptions)
            : base(action, cancellationTokenSource.Token, creationOptions)
        {
            progress.ProgressChanged += (_, args) => OnProgress();
            _cancellationTokenSource = cancellationTokenSource;
        }

        protected Future(Action<object> action, object state, CancellationTokenSource cancellationTokenSource,
                                  Progress progress)
            : base(action, state, cancellationTokenSource.Token)
        {
            progress.ProgressChanged += (_, args) => OnProgress();
            _cancellationTokenSource = cancellationTokenSource;
        }

        protected Future(Action<object> action, object state, CancellationTokenSource cancellationTokenSource,
                                  Progress progress,
                                  TaskCreationOptions creationOptions)
            : base(action, state, cancellationTokenSource.Token, creationOptions)
        {
            progress.ProgressChanged += (_, args) => OnProgress();
            _cancellationTokenSource = cancellationTokenSource;
        }
        #endregion

        #region factory methods
        public static Future Create(Action<CancellationToken, IProgress> action)
        {
            var cts = new CancellationTokenSource();
            var progress = new Progress();
            return new Future(() => action(cts.Token, progress), cts, progress);
        }

        public static Future Create(Action<CancellationToken, IProgress> action, TaskCreationOptions creationOptions)
        {
            var cts = new CancellationTokenSource();
            var progress = new Progress();
            return new Future(() => action(cts.Token, progress), cts, progress, creationOptions);
        }

        public static Future Create(Action<CancellationToken, IProgress, object> action, object state)
        {
            var cts = new CancellationTokenSource();
            var progress = new Progress();
            return new Future(s => action(cts.Token, progress, s), state, cts, progress);
        }

        public static Future Create(Action<CancellationToken, IProgress, object> action, object state, TaskCreationOptions creationOptions)
        {
            var cts = new CancellationTokenSource();
            var progress = new Progress();
            return new Future(s => action(cts.Token, progress, s), state, cts, progress, creationOptions);
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

        public void OnProgress()
        {
            ProgressChanged.Raise(this, new ProgressEventArgs());
        }
    }

    /// <summary>
    /// Implementation for operations that report progress with a value (not just hearbeats).
    /// Name must be different from `Future&lt;TProgress&gt;` since that clashes with `Future&lt;TResult&gt;`
    /// </summary>
    /// <typeparam name="TProgress"></typeparam>
    public class FutureProgress<TProgress> : Future, IProgress<TProgress>
        where TProgress : class
    {
        public new event EventHandler<ProgressEventArgs<TProgress>> ProgressChanged;

        #region constructors
        protected FutureProgress(Action action, CancellationTokenSource cancellationTokenSource,
                                          Progress<TProgress> progress)
            : base(action, cancellationTokenSource, progress)
        {
            progress.ProgressChanged += (_, args) => OnProgress();
        }

        protected FutureProgress(Action action, CancellationTokenSource cancellationTokenSource,
                                          Progress<TProgress> progress,
                                          TaskCreationOptions creationOptions)
            : base(action, cancellationTokenSource, progress, creationOptions)
        {
            progress.ProgressChanged += (_, args) => OnProgress();
        }

        protected FutureProgress(Action<object> action, object state,
                                          CancellationTokenSource cancellationTokenSource,
                                          Progress<TProgress> progress)
            : base(action, state, cancellationTokenSource, progress)
        {
            progress.ProgressChanged += (_, args) => OnProgress();
        }

        protected FutureProgress(Action<object> action, object state,
                                          CancellationTokenSource cancellationTokenSource,
                                          Progress<TProgress> progress,
                                          TaskCreationOptions creationOptions)
            : base(action, state, cancellationTokenSource, progress, creationOptions)
        {
            progress.ProgressChanged += (_, args) => OnProgress();
        }
        #endregion

        #region factory methods
        public static FutureProgress<TProgress> Create(Action<CancellationToken, IProgress<TProgress>> action)
        {
            var cts = new CancellationTokenSource();
            var progress = new Progress<TProgress>();
            return new FutureProgress<TProgress>(() => action(cts.Token, progress), cts, progress);
        }

        public static FutureProgress<TProgress> Create(Action<CancellationToken, IProgress<TProgress>> action, TaskCreationOptions creationOptions)
        {
            var cts = new CancellationTokenSource();
            var progress = new Progress<TProgress>();
            return new FutureProgress<TProgress>(() => action(cts.Token, progress), cts, progress, creationOptions);
        }

        public static FutureProgress<TProgress> Create(Action<CancellationToken, IProgress<TProgress>, object> action, object state)
        {
            var cts = new CancellationTokenSource();
            var progress = new Progress<TProgress>();
            return new FutureProgress<TProgress>(s => action(cts.Token, progress, s), state, cts, progress);
        }

        public static FutureProgress<TProgress> Create(Action<CancellationToken, IProgress<TProgress>, object> action, object state, TaskCreationOptions creationOptions)
        {
            var cts = new CancellationTokenSource();
            var progress = new Progress<TProgress>();
            return new FutureProgress<TProgress>(s => action(cts.Token, progress, s), state, cts, progress, creationOptions);
        }
        #endregion

        public void OnProgress(TProgress value)
        {
            ProgressChanged.Raise(this, new ProgressEventArgs<TProgress>(value));
        }
    }

    public class Future<TResult> : Task<TResult>, IProgress
    {
        private readonly CancellationTokenSource _cancellationTokenSource;

        public event EventHandler<ProgressEventArgs> ProgressChanged;

        #region constructors
        protected Future(Func<TResult> function, CancellationTokenSource cancellationTokenSource,
                                  Progress progress)
            : base(function, cancellationTokenSource.Token)
        {
            progress.ProgressChanged += (_, args) => OnProgress();
            _cancellationTokenSource = cancellationTokenSource;
        }

        protected Future(Func<TResult> function, CancellationTokenSource cancellationTokenSource,
                                  Progress progress,
                                  TaskCreationOptions creationOptions)
            : base(function, cancellationTokenSource.Token, creationOptions)
        {
            progress.ProgressChanged += (_, args) => OnProgress();
            _cancellationTokenSource = cancellationTokenSource;
        }

        protected Future(Func<object, TResult> function, object state,
                                  CancellationTokenSource cancellationTokenSource, Progress progress)
            : base(function, state, cancellationTokenSource.Token)
        {
            progress.ProgressChanged += (_, args) => OnProgress();
            _cancellationTokenSource = cancellationTokenSource;
        }

        protected Future(Func<object, TResult> function, object state,
                                  CancellationTokenSource cancellationTokenSource, Progress progress,
                                  TaskCreationOptions creationOptions)
            : base(function, state, cancellationTokenSource.Token, creationOptions)
        {
            progress.ProgressChanged += (_, args) => OnProgress();
            _cancellationTokenSource = cancellationTokenSource;
        }
        #endregion

        #region factory methods
        public static Future<TResult> Create(Func<CancellationToken, IProgress, TResult> function)
        {
            var cts = new CancellationTokenSource();
            var progress = new Progress();
            return new Future<TResult>(() => function(cts.Token, progress), cts, progress);
        }

        public static Future<TResult> Create(Func<CancellationToken, IProgress, TResult> function, TaskCreationOptions creationOptions)
        {
            var cts = new CancellationTokenSource();
            var progress = new Progress();
            return new Future<TResult>(() => function(cts.Token, progress), cts, progress, creationOptions);
        }

        public static Future<TResult> Create(Func<CancellationToken, IProgress, object, TResult> function, object state)
        {
            var cts = new CancellationTokenSource();
            var progress = new Progress();
            return new Future<TResult>(s => function(cts.Token, progress, s), state, cts, progress);
        }

        public static Future<TResult> Create(Func<CancellationToken, IProgress, object, TResult> function, object state, TaskCreationOptions creationOptions)
        {
            var cts = new CancellationTokenSource();
            var progress = new Progress();
            return new Future<TResult>(s => function(cts.Token, progress, s), state, cts, progress, creationOptions);
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

        public void OnProgress()
        {
            ProgressChanged.Raise(this, new ProgressEventArgs());
        }
    }

    /// <summary>
    /// Implementation for operations that report progress with a value (not just hearbeats)
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <typeparam name="TProgress"></typeparam>
    public class Future<TResult, TProgress> : Future<TResult>, IProgress<TProgress>
        where TProgress : class
    {
        public new event EventHandler<ProgressEventArgs<TProgress>> ProgressChanged;

        #region constructors
        protected Future(Func<TResult> function, CancellationTokenSource cancellationTokenSource,
                                  Progress<TProgress> progress)
            : base(function, cancellationTokenSource, progress)
        {
            progress.ProgressChanged += (_, arg) => OnProgress(arg.Value);
        }

        protected Future(Func<TResult> function, CancellationTokenSource cancellationTokenSource,
                                  Progress<TProgress> progress,
                                  TaskCreationOptions creationOptions)
            : base(function, cancellationTokenSource, progress, creationOptions)
        {
            progress.ProgressChanged += (_, arg) => OnProgress(arg.Value);
        }

        protected Future(Func<object, TResult> function, object state,
                                  CancellationTokenSource cancellationTokenSource, Progress<TProgress> progress)
            : base(function, state, cancellationTokenSource, progress)
        {
            progress.ProgressChanged += (_, arg) => OnProgress(arg.Value);
        }

        protected Future(Func<object, TResult> function, object state,
                                  CancellationTokenSource cancellationTokenSource, Progress<TProgress> progress,
                                  TaskCreationOptions creationOptions)
            : base(function, state, cancellationTokenSource, progress, creationOptions)
        {
            progress.ProgressChanged += (_, arg) => OnProgress(arg.Value);
        }
        #endregion

        #region factory methods
        public static new Future<TResult, TProgress> Create(Func<CancellationToken, IProgress<TProgress>, TResult> function)
        {
            var cts = new CancellationTokenSource();
            var progress = new Progress<TProgress>();
            return new Future<TResult, TProgress>(() => function(cts.Token, progress), cts, progress);
        }

        public static new Future<TResult, TProgress> Create(Func<CancellationToken, IProgress<TProgress>, TResult> function, TaskCreationOptions creationOptions)
        {
            var cts = new CancellationTokenSource();
            var progress = new Progress<TProgress>();
            return new Future<TResult, TProgress>(() => function(cts.Token, progress), cts, progress, creationOptions);
        }

        public static new Future<TResult, TProgress> Create(Func<CancellationToken, IProgress<TProgress>, object, TResult> function, object state)
        {
            var cts = new CancellationTokenSource();
            var progress = new Progress<TProgress>();
            return new Future<TResult, TProgress>(s => function(cts.Token, progress, s), state, cts, progress);
        }

        public static new Future<TResult, TProgress> Create(Func<CancellationToken, IProgress<TProgress>, object, TResult> function, object state, TaskCreationOptions creationOptions)
        {
            var cts = new CancellationTokenSource();
            var progress = new Progress<TProgress>();
            return new Future<TResult, TProgress>(s => function(cts.Token, progress, s), state, cts, progress, creationOptions);
        }
        #endregion

        public void OnProgress(TProgress value)
        {
            ProgressChanged.Raise(this, new ProgressEventArgs<TProgress>(value));
        }
    }
}
