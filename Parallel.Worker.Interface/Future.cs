using System;
using System.Collections.Generic;
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
        public static Future Create(Action action)
        {
            return new Future(action, new CancellationTokenSource());
        }

        public static Future Create(Action action, TaskCreationOptions creationOptions)
        {
            return new Future(action, new CancellationTokenSource(), creationOptions);
        }

        public static Future Create(Action<object> action, object state)
        {
            return new Future(action, state, new CancellationTokenSource());
        }

        public static Future Create(Action<object> action, object state, TaskCreationOptions creationOptions)
        {
            return new Future(action, state, new CancellationTokenSource(), creationOptions);
        }
        #endregion

        public void Cancel()
        {
            _cancellationTokenSource.Cancel();
        }

        public static void CancelAll(IEnumerable<Future> futures)
        {
            foreach (Future f in futures)
            {
                f.Cancel();
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
        public static Future<TResult> Create(Func<TResult> function)
        {
            return new Future<TResult>(function, new CancellationTokenSource());
        }

        public static Future<TResult> Create(Func<TResult> function, TaskCreationOptions creationOptions)
        {
            return new Future<TResult>(function, new CancellationTokenSource(), creationOptions);
        }

        public static Future<TResult> Create(Func<object, TResult> function, object state)
        {
            return new Future<TResult>(function, state, new CancellationTokenSource());
        }

        public static Future<TResult> Create(Func<object, TResult> function, object state, TaskCreationOptions creationOptions)
        {
            return new Future<TResult>(function, state, new CancellationTokenSource(), creationOptions);
        }
        #endregion

        public void Cancel()
        {
            _cancellationTokenSource.Cancel();
        }

        public static void CancelAll(IEnumerable<Future<TResult>> futures)
        {
            foreach (Future<TResult> f in futures)
            {
                f.Cancel();
            }
        }
    }
}
