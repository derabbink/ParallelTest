using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;

namespace Parallel.Worker.Interface.Rx
{
    internal static class OperationProgressHelper
    {
        /// <summary>
        /// Filters by operationId
        /// </summary>
        /// <param name="source"></param>
        /// <param name="id">operationId to filter on</param>
        /// <returns></returns>
        internal static IObservable<OperationProgress> ForOperation(this IObservable<OperationProgress> source, Guid id)
        {
            return source.Where(op => op.OperationId == id);
        }

        /// <summary>
        /// Combines OperationStarted and OperationCompleted into a uniformly typed sequenece
        /// that will produces an error for a failed operation.
        /// Blindly passes on operationId
        /// </summary>
        /// <param name="started"></param>
        /// <param name="completed"></param>
        /// <returns></returns>
        internal static IObservable<OperationProgress> Merge(this IObservable<OperationStarted> started,
                                                             IObservable<OperationCompleted> completed)
        {
            return started.TranslateStarted()
                          .Concat(completed.TranslateOperationResult());
        }

        /// <summary>
        /// Casts to more generic type
        /// </summary>
        /// <param name="started"></param>
        /// <returns></returns>
        private static IObservable<OperationProgress> TranslateStarted(this IObservable<OperationStarted> started)
        {
            return started.Select(m => m as OperationProgress);
        }

        /// <summary>
        /// Translates successful/unsuccessful operation results
        /// into messages/errors respectively.
        /// Blindly passes on operationId
        /// </summary>
        /// <param name="completed"></param>
        /// <returns></returns>
        private static IObservable<OperationCompletedSuccess> TranslateOperationResult(this IObservable<OperationCompleted> completed)
        {

            return completed.TranslateOperationError().Amb(completed.TranslateOperationSuccess());
        }

        /// <summary>
        /// Translates unsuccessful OperationResult.
        /// Produces error after incoming error result.
        /// Blindly passes on operationId
        /// </summary>
        /// <param name="completed"></param>
        /// <returns>observable sequence that will produce an error</returns>
        private static IObservable<OperationCompletedSuccess> TranslateOperationError(
            this IObservable<OperationCompleted> completed)
        {
            return
                completed.Where(or => or.Result.State == OperationResultState.Error)
                         .Select(or => new OperationCompletedError(or.Result.Exception, or.OperationId));
        }

        /// <summary>
        /// Translates successful OperationResults.
        /// Blindly passes on operationId
        /// </summary>
        /// <param name="completed"></param>
        /// <returns></returns>
        private static IObservable<OperationCompletedSuccess> TranslateOperationSuccess(
            this IObservable<OperationCompleted> completed)
        {
            return
                completed.Where(or => or.Result.State == OperationResultState.Success)
                         .Select(or => new OperationCompletedSuccess(or.Result.Value, or.OperationId));
        }
    }
}
