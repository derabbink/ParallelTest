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
    public class Future : Task, IAwaitable, ICancelable, IProgressEventSource
    {
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly Progress _progress;

        #region constructors
        protected Future(Action action, CancellationTokenSource cancellationTokenSource, Progress progress)
            : base(action, cancellationTokenSource.Token)
        {
            _cancellationTokenSource = cancellationTokenSource;
            _progress = progress;
        }

        protected Future(Action action, CancellationTokenSource cancellationTokenSource,
                         Progress progress,
                         TaskCreationOptions creationOptions)
            : base(action, cancellationTokenSource.Token, creationOptions)
        {
            _cancellationTokenSource = cancellationTokenSource;
            _progress = progress;
        }

        protected Future(Action<object> action, object state, CancellationTokenSource cancellationTokenSource,
                         Progress progress)
            : base(action, state, cancellationTokenSource.Token)
        {
            _cancellationTokenSource = cancellationTokenSource;
            _progress = progress;
        }

        protected Future(Action<object> action, object state, CancellationTokenSource cancellationTokenSource,
                         Progress progress,
                         TaskCreationOptions creationOptions)
            : base(action, state, cancellationTokenSource.Token, creationOptions)
        {
            _cancellationTokenSource = cancellationTokenSource;
            _progress = progress;
        }
        #endregion

        #region factory methods
        public static Future Create(Action<CancellationToken, Action> action)
        {
            var cts = new CancellationTokenSource();
            Progress progress = new Progress();
            return new Future(() => action(cts.Token, progress.Report), cts, progress);
        }

        public static Future Create(Action<CancellationToken, Action> action, TaskCreationOptions creationOptions)
        {
            var cts = new CancellationTokenSource();
            Progress progress = new Progress();
            return new Future(() => action(cts.Token, progress.Report), cts, progress, creationOptions);
        }

        public static Future Create(Action<CancellationToken, Action, object> action, object state)
        {
            var cts = new CancellationTokenSource();
            Progress progress = new Progress();
            return new Future(s => action(cts.Token, progress.Report, s), state, cts, progress);
        }

        public static Future Create(Action<CancellationToken, Action, object> action, object state, TaskCreationOptions creationOptions)
        {
            var cts = new CancellationTokenSource();
            Progress progress = new Progress();
            return new Future(s => action(cts.Token, progress.Report, s), state, cts, progress, creationOptions);
        }
        #endregion

        #region properties
        /// <summary>
        /// Returns true if IsCompleted or IsFaulted
        /// </summary>
        public bool IsDone {
            get { return IsCompleted || IsFaulted; }
        }
        #endregion

        #region operations
        public void Cancel()
        {
            _cancellationTokenSource.Cancel();
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

        #region event subscription
        public void SubscribeProgress(EventHandler<ProgressEventArgs> handler)
        {
            _progress.ProgressChanged += handler;
        }

        public void UnsubscribeProgress(EventHandler<ProgressEventArgs> handler)
        {
            _progress.ProgressChanged -= handler;
        }
        #endregion
        #endregion

        #region static helper methods
        public static void WaitAll(IEnumerable<Future> futures)
        {
            IList<Exception> exceptions = new List<Exception>();
            foreach (Future f in futures)
                try
                {
                    f.Wait();
                }
                catch (Exception e)
                {
                    exceptions.Add(e);
                }
            if (exceptions.Any())
                throw new AggregateException(exceptions);
        }

        public static void CancelAll(IEnumerable<Future> futures)
        {
            IList<Exception> exceptions = new List<Exception>();
            foreach (Future f in futures)
                try
                {
                    f.Cancel();
                }
                catch (Exception e)
                {
                    exceptions.Add(e);
                }
            if (exceptions.Any())
                throw new AggregateException(exceptions);
        }
        #endregion
    }

    /// <summary>
    /// Implementation for operations that report progress with a value (not just hearbeats).
    /// Name must be different from `Future&lt;TProgress&gt;` since that clashes with `Future&lt;TResult&gt;`
    /// </summary>
    /// <typeparam name="TProgress"></typeparam>
    public class FutureProgress<TProgress> : Future, IProgressEventSource<TProgress>
        where TProgress : class
    {
        private readonly Progress<TProgress> _progress;

        #region constructors
        protected FutureProgress(Action action, CancellationTokenSource cancellationTokenSource,
                                 Progress<TProgress> progress)
            : base(action, cancellationTokenSource, progress)
        {
            _progress = progress;
        }

        protected FutureProgress(Action action, CancellationTokenSource cancellationTokenSource,
                                 Progress<TProgress> progress,
                                 TaskCreationOptions creationOptions)
            : base(action, cancellationTokenSource, progress, creationOptions)
        {
            _progress = progress;
        }

        protected FutureProgress(Action<object> action, object state,
                                 CancellationTokenSource cancellationTokenSource, Progress<TProgress> progress)
            : base(action, state, cancellationTokenSource, progress)
        {
            _progress = progress;
        }

        protected FutureProgress(Action<object> action, object state,
                                 CancellationTokenSource cancellationTokenSource, Progress<TProgress> progress,
                                 TaskCreationOptions creationOptions)
            : base(action, state, cancellationTokenSource, progress, creationOptions)
        {
            _progress = progress;
        }
        #endregion

        #region factory methods
        public static FutureProgress<TProgress> Create(Action<CancellationToken, Action<TProgress>> action)
        {
            var cts = new CancellationTokenSource();
            Progress<TProgress> progress = new Progress<TProgress>();
            return new FutureProgress<TProgress>(() => action(cts.Token, progress.Report), cts, progress);
        }

        public static FutureProgress<TProgress> Create(Action<CancellationToken, Action<TProgress>> action, TaskCreationOptions creationOptions)
        {
            var cts = new CancellationTokenSource();
            Progress<TProgress> progress = new Progress<TProgress>();
            return new FutureProgress<TProgress>(() => action(cts.Token, progress.Report), cts, progress, creationOptions);
        }

        public static FutureProgress<TProgress> Create(Action<CancellationToken, Action<TProgress>, object> action, object state)
        {
            var cts = new CancellationTokenSource();
            Progress<TProgress> progress = new Progress<TProgress>();
            return new FutureProgress<TProgress>(s => action(cts.Token, progress.Report, s), state, cts, progress);
        }

        public static FutureProgress<TProgress> Create(Action<CancellationToken, Action<TProgress>, object> action, object state, TaskCreationOptions creationOptions)
        {
            var cts = new CancellationTokenSource();
            Progress<TProgress> progress = new Progress<TProgress>();
            return new FutureProgress<TProgress>(s => action(cts.Token, progress.Report, s), state, cts, progress, creationOptions);
        }
        #endregion

        #region operations
        #region event subscription
        //hide method
        private new void SubscribeProgress(EventHandler<ProgressEventArgs> handler)
        {
            throw new NotImplementedException();
        }

        //hide method
        private new void UnsubscribeProgress(EventHandler<ProgressEventArgs> handler)
        {
            throw new NotImplementedException();
        }

        public void SubscribeProgress(EventHandler<ProgressEventArgs<TProgress>> handler)
        {
            _progress.ProgressChanged += handler;
        }

        public void UnsubscribeProgress(EventHandler<ProgressEventArgs<TProgress>> handler)
        {
            _progress.ProgressChanged -= handler;
        }
        #endregion
        #endregion
    }

    public class Future<TResult> : Task<TResult>, IAwaitable, ICancelable, IProgressEventSource
    {
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly Progress _progress;

        #region constructors
        protected Future(Func<TResult> function, CancellationTokenSource cancellationTokenSource, Progress progress)
            : base(function, cancellationTokenSource.Token)
        {
            _cancellationTokenSource = cancellationTokenSource;
            _progress = progress;
        }

        protected Future(Func<TResult> function, CancellationTokenSource cancellationTokenSource, Progress progress,
                         TaskCreationOptions creationOptions)
            : base(function, cancellationTokenSource.Token, creationOptions)
        {
            _cancellationTokenSource = cancellationTokenSource;
            _progress = progress;
        }

        protected Future(Func<object, TResult> function, object state,
                         CancellationTokenSource cancellationTokenSource, Progress progress)
            : base(function, state, cancellationTokenSource.Token)
        {
            _cancellationTokenSource = cancellationTokenSource;
            _progress = progress;
        }

        protected Future(Func<object, TResult> function, object state,
                         CancellationTokenSource cancellationTokenSource, Progress progress,
                         TaskCreationOptions creationOptions)
            : base(function, state, cancellationTokenSource.Token, creationOptions)
        {
            _cancellationTokenSource = cancellationTokenSource;
            _progress = progress;
        }
        #endregion

        #region factory methods
        public static Future<TResult> Create(Func<CancellationToken, Action, TResult> function)
        {
            var cts = new CancellationTokenSource();
            Progress progress = new Progress();
            return new Future<TResult>(() => function(cts.Token, progress.Report), cts, progress);
        }

        public static Future<TResult> Create(Func<CancellationToken, Action, TResult> function, TaskCreationOptions creationOptions)
        {
            var cts = new CancellationTokenSource();
            Progress progress = new Progress();
            return new Future<TResult>(() => function(cts.Token, progress.Report), cts, progress, creationOptions);
        }

        public static Future<TResult> Create(Func<CancellationToken, Action, object, TResult> function, object state)
        {
            var cts = new CancellationTokenSource();
            Progress progress = new Progress();
            return new Future<TResult>(s => function(cts.Token, progress.Report, s), state, cts, progress);
        }

        public static Future<TResult> Create(Func<CancellationToken, Action, object, TResult> function, object state, TaskCreationOptions creationOptions)
        {
            var cts = new CancellationTokenSource();
            Progress progress = new Progress();
            return new Future<TResult>(s => function(cts.Token, progress.Report, s), state, cts, progress, creationOptions);
        }
        #endregion

        #region properties
        /// <summary>
        /// Returns true if IsCompleted or IsFaulted
        /// </summary>
        public bool IsDone
        {
            get { return IsCompleted || IsFaulted; }
        }
        #endregion

        #region operations
        public void Cancel()
        {
            _cancellationTokenSource.Cancel();
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

        #region event subscription
        public void SubscribeProgress(EventHandler<ProgressEventArgs> handler)
        {
            _progress.ProgressChanged += handler;
        }

        public void UnsubscribeProgress(EventHandler<ProgressEventArgs> handler)
        {
            _progress.ProgressChanged -= handler;
        }
        #endregion
        #endregion

        #region static helper methods
        public static void WaitAll(IEnumerable<Future<TResult>> futures)
        {
            IList<Exception> exceptions = new List<Exception>();
            foreach (Future<TResult> f in futures)
                try
                {
                    f.Wait();
                }
                catch (Exception e)
                {
                    exceptions.Add(e);
                }
            if (exceptions.Any())
                throw new AggregateException(exceptions);
        }

        public static void CancelAll(IEnumerable<Future<TResult>> futures)
        {
            IList<Exception> exceptions = new List<Exception>();
            foreach (Future<TResult> f in futures)
                try
                {
                    f.Cancel();
                }
                catch (Exception e)
                {
                    exceptions.Add(e);
                }
            if (exceptions.Any())
                throw new AggregateException(exceptions);
        }
        #endregion
    }

    /// <summary>
    /// Implementation for operations that report progress with a value (not just hearbeats)
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <typeparam name="TProgress"></typeparam>
    public class Future<TResult, TProgress> : Future<TResult>, IProgressEventSource<TProgress>
        where TProgress : class
    {
        private readonly Progress<TProgress> _progress;

        #region constructors
        protected Future(Func<TResult> function, CancellationTokenSource cancellationTokenSource,
                         Progress<TProgress> progress)
            : base(function, cancellationTokenSource, progress)
        {
            _progress = progress;
        }

        protected Future(Func<TResult> function, CancellationTokenSource cancellationTokenSource,
                         Progress<TProgress> progress,
                         TaskCreationOptions creationOptions)
            : base(function, cancellationTokenSource, progress, creationOptions)
        {
            _progress = progress;
        }

        protected Future(Func<object, TResult> function, object state,
                         CancellationTokenSource cancellationTokenSource, Progress<TProgress> progress)
            : base(function, state, cancellationTokenSource, progress)
        {
            _progress = progress;
        }

        protected Future(Func<object, TResult> function, object state,
                         CancellationTokenSource cancellationTokenSource, Progress<TProgress> progress,
                         TaskCreationOptions creationOptions)
            : base(function, state, cancellationTokenSource, progress, creationOptions)
        {
            _progress = progress;
        }
        #endregion

        #region factory methods
        public static new Future<TResult, TProgress> Create(Func<CancellationToken, Action<TProgress>, TResult> function)
        {
            var cts = new CancellationTokenSource();
            Progress<TProgress> progress = new Progress<TProgress>();
            return new Future<TResult, TProgress>(() => function(cts.Token, progress.Report), cts, progress);
        }

        public static new Future<TResult, TProgress> Create(Func<CancellationToken, Action<TProgress>, TResult> function, TaskCreationOptions creationOptions)
        {
            var cts = new CancellationTokenSource();
            Progress<TProgress> progress = new Progress<TProgress>();
            return new Future<TResult, TProgress>(() => function(cts.Token, progress.Report), cts, progress, creationOptions);
        }

        public static new Future<TResult, TProgress> Create(Func<CancellationToken, Action<TProgress>, object, TResult> function, object state)
        {
            var cts = new CancellationTokenSource();
            Progress<TProgress> progress = new Progress<TProgress>();
            return new Future<TResult, TProgress>(s => function(cts.Token, progress.Report, s), state, cts, progress);
        }

        public static new Future<TResult, TProgress> Create(Func<CancellationToken, Action<TProgress>, object, TResult> function, object state, TaskCreationOptions creationOptions)
        {
            var cts = new CancellationTokenSource();
            Progress<TProgress> progress = new Progress<TProgress>();
            return new Future<TResult, TProgress>(s => function(cts.Token, progress.Report, s), state, cts, progress, creationOptions);
        }
        #endregion

        #region operations
        #region event subscription
        //hide method
        private new void SubscribeProgress(EventHandler<ProgressEventArgs> handler)
        {
            throw new NotImplementedException();
        }

        //hide method
        private new void UnsubscribeProgress(EventHandler<ProgressEventArgs> handler)
        {
            throw new NotImplementedException();
        }

        public void SubscribeProgress(EventHandler<ProgressEventArgs<TProgress>> handler)
        {
            _progress.ProgressChanged += handler;
        }

        public void UnsubscribeProgress(EventHandler<ProgressEventArgs<TProgress>> handler)
        {
            _progress.ProgressChanged -= handler;
        }
        #endregion
        #endregion
    }
}
