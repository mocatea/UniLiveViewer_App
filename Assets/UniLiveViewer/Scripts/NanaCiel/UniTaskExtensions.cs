using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

namespace NanaCiel
{
    static class UniTaskExtensions
    {
        public static async UniTask OnError(this UniTask task)
        {
            try
            {
                await task;
            }
            catch (Exception e) when (!(e is OperationCanceledException))
            {
                Debug.LogError(e.Message);
            }
        }

        public static async UniTask OnError(this UniTask task, Action<Exception> onError)
        {
            try
            {
                await task;
            }
            catch (Exception e) when (!(e is OperationCanceledException))
            {
                onError(e);
            }
        }

        public static async UniTask OnError<T>(this UniTask<T> task)
        {
            try
            {
                await task;
            }
            catch (Exception e) when (!(e is OperationCanceledException))
            {
                Debug.LogError(e.Message);
            }
        }

        public static async UniTask<T> OnError<T>(this UniTask<T> task, Action<Exception> onError)
        {
            try
            {
                await task;
            }
            catch (Exception e) when (!(e is OperationCanceledException))
            {
                onError(e);
            }
            return await task;
        }

        /// <summary>
        /// OperationCanceledExceptionを意図的に無視用
        /// </summary>
        public static async UniTask IgnoreCancellationException(this UniTask task)
        {
            try
            {
                await task;
            }
            catch
            {
                //想定なので握りつぶす
            }
        }
    }
}