using Cysharp.Threading.Tasks;
using System.Threading;
using UniLiveViewer.Actor;
using UniLiveViewer.ValueObject;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Menu
{
    /// <summary>
    /// instanceの生成のみを担当
    /// 現状は後からVRMを変更できない仕様とする（いろいろセットアップがめんどくさい）
    /// </summary>
    public class ActorEntityFactory
    {
        int _instanceId = 0;

        /// <summary>
        /// 見えないところで生成
        /// </summary>
        readonly Vector3 InitPos = new Vector3(0, 100, 0);

        readonly ActorLifetimeScopeSetting _setting;

        [Inject]
        public ActorEntityFactory(
            ActorLifetimeScopeSetting setting)
        {
            _setting = setting;
        }

        public async UniTask<ActorLifetimeScope> GenerateFBXAsync(RegisterData data, CancellationToken cancellation)
        {
            var presetIndex = data.Id.Id;
            if (_setting.FBXActorLifetimeScopePrefab.Count <= presetIndex) return null;
            var actorLifetimeScope = _setting.FBXActorLifetimeScopePrefab[presetIndex];
            var instanceID = new InstanceId(_instanceId);
            var installer = new ActorInstaller(data, instanceID);

            using (LifetimeScope.Enqueue(installer))
            {
                var instance = GameObject.Instantiate(actorLifetimeScope, InitPos, Quaternion.identity);
                await instance.BuildAsync(cancellation);
                var option = instance.CreateChildFromPrefab(_setting.OptionLifetimeScope);
                option.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                _instanceId++;

                await UniTask.Yield(cancellation);
                return instance;
            }
        }

        public async UniTask<ActorLifetimeScope> GenerateVRMAsync(RegisterData data, CancellationToken cancellation)
        {
            var instanceID = new InstanceId(_instanceId);
            var installer = new ActorInstaller(data, instanceID);

            using (LifetimeScope.Enqueue(installer))
            {
                var prefab = data.LoadVrmAsMode10 ? _setting.Vrm10ActorLifetimeScopePrefab : _setting.VrmActorLifetimeScopePrefab;
                var instance = GameObject.Instantiate(prefab, InitPos, Quaternion.identity);
                await instance.BuildAsync(cancellation);
                var option = instance.CreateChildFromPrefab(_setting.OptionLifetimeScope);
                option.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                _instanceId++;

                await UniTask.Yield(cancellation);
                return instance;
            }
        }
    }
}
