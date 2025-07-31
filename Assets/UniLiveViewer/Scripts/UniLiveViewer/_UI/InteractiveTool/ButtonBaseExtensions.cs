using System;
using UniRx;

namespace UniLiveViewer
{
    public static class ButtonBaseRxExtensions
    {
        public static IObservable<Button_Base> OnTriggerAsObservable(this Button_Base button)
        {
            return Observable.FromEvent<Action<Button_Base>, Button_Base>(
                handler => handler.Invoke,
                h => button.onTrigger += h,
                h => button.onTrigger -= h
            );
        }
    }
}