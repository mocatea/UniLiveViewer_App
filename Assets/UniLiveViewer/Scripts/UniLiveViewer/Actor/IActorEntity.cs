using Cysharp.Threading.Tasks;
using System.Threading;
using UniRx;
using UnityEngine;

namespace UniLiveViewer.Actor
{
    public interface IActorEntity
    {
        IReactiveProperty<ActorEntity> ActorEntity();

        /// <summary>
        /// gameObject.activeSelfに連動して扱う
        /// Tick系はactive時のみ更新の方針
        /// </summary>
        /// <returns></returns>
        IReactiveProperty<bool> Active();

        IReactiveProperty<float> RootScalar();

        IReactiveProperty<float> RawRootScalar();

        IReactiveProperty<ActorState> ActorState();

        /// <summary>
        /// デバッグ用
        /// </summary>
        UniTask EditorOnlySetupAsync(Transform firstParent, CancellationToken cancellation);

        UniTask SetupAsync(Transform firstParent, CancellationToken cancellation);

        void Activate(bool isActive);

        void SetRootTransform(Vector3 pos, Quaternion quaternion, Vector3 scale);

        void AddRootScalar(float add);

        void SetState(ActorState setState, Transform overrideTarget);

        void OnTick();

        void Delete();
    }
}
