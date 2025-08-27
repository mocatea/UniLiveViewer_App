using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UniLiveViewer.SO;
using UnityEngine;
using VContainer;

namespace UniLiveViewer.Actor.Option
{
    public class FootWaterSplashService
    {
        const int AudioMilliseconds = 1500;//およそ
        const float ReuseDelayTime = 0.25f;
        const int MaxPoolCount = 8;

        float _actorRootScalar = 1;
        float _actorHeight = 0;
        FootState _lFootState;
        FootState _rFootState;
        readonly Queue<ParticleSystem> _pool = new();

        readonly Transform _parent;
        readonly FootWaterSplashSettings _settings;
        readonly AudioSourceService _audioSourceService;
        readonly FootstepAudioData _raisedfootAudioData;
        readonly FootstepAudioData _loweredFeetAudioData;

        [Inject]
        public FootWaterSplashService(
            ActorOptionLifetimeScope actorOptionLifetimeScope,
            FootWaterSplashSettings settings,
            AudioSourceService audioSourceService,
            AudioClipSettings setting)
        {
            _parent = actorOptionLifetimeScope.transform;
            _settings = settings;
            _audioSourceService = audioSourceService;
            _raisedfootAudioData = setting.RaisedFootWaterSplashAudioData;
            _loweredFeetAudioData = setting.LoweredFeetWaterSplashAudioData;
        }

        public void OnChangeActorEntity(ActorEntity actorEntity)
        {
            if (actorEntity == null)
            {
                _lFootState = null;
                _rFootState = null;
                return;
            }
            Setup(actorEntity);
        }

        void Setup(ActorEntity actorEntity)
        {
            _lFootState = new();
            _rFootState = new();
            _pool.Clear();

            var map = actorEntity.BoneMap;
            _lFootState.Foot = map[HumanBodyBones.LeftFoot];
            _rFootState.Foot = map[HumanBodyBones.RightFoot];

            _actorHeight = actorEntity.Height;

            for (int i = 0; i < MaxPoolCount; i++)
            {
                var ps = GameObject.Instantiate(_settings.WaterSplashPrefab, _parent).GetComponent<ParticleSystem>();
                ps.gameObject.SetActive(false);
                ps.transform.localScale = CalculateParticleSize;
                _pool.Enqueue(ps);
            }
        }

        public void OnChangeRootScalar(float rootScalar)
        {
            _actorRootScalar = rootScalar;

            foreach (var item in _pool)
            {
                item.transform.localScale = CalculateParticleSize;
            }
        }

        Vector3 CalculateParticleSize => Vector3.one * 0.25f * (_actorHeight / 1.5f) * _actorRootScalar;

        public async UniTask OnLateTickAsync(CancellationToken cancellation)
        {
            if (_lFootState == null || _rFootState == null) return;
            
            HitCheckAsync(_lFootState, cancellation).Forget();
            HitCheckAsync(_rFootState, cancellation).Forget();

            _lFootState.PreHeigth = _lFootState.Foot.position.y;
            _rFootState.PreHeigth = _rFootState.Foot.position.y;

            await UniTask.CompletedTask;
        }

        async UniTask HitCheckAsync(FootState foot, CancellationToken cancellation)
        {
            if (foot.ReuseCooldownTime < 0)
            {
                var y = foot.Foot.position.y;
                if (y < 0.18f || 0.2f < y) return; // 水面高さ
                if (Mathf.Abs(foot.PreHeigth - y) < 0.002f) return; // 一定以上の勢い(0.005f～ほぼでない)

                var isRaisedFeet = foot.PreHeigth < y;

                var euler = new Vector3(0, Random.Range(0, 360), 0);
                SpawnAsync(foot.Foot.position, Quaternion.Euler(euler), isRaisedFeet, cancellation).Forget();
                foot.ReuseCooldownTime = ReuseDelayTime;
            }
            else
            {
                foot.ReuseCooldownTime -= Time.deltaTime;
            }

            await UniTask.CompletedTask;
        }

        public async UniTask SpawnAsync(Vector3 pos, Quaternion rot, bool isRaisedFeet, CancellationToken cancellation)
        {
            var ps = _pool.Dequeue();
            var t = ps.transform;
            t.SetPositionAndRotation(pos, rot);

            // 完全初期化してから再生
            ps.gameObject.SetActive(true);
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ps.Clear(true);

            var main = ps.main;
            main.loop = false;          // 返却管理を簡単に
            ps.Play(true);

            _audioSourceService.transform.position = pos;
            if (isRaisedFeet)
            {
                var index = Random.Range(0, _raisedfootAudioData.AudioClip.Count);
                _audioSourceService.PlayOneShot(_raisedfootAudioData.AudioClip[index]);
            }
            else
            {
                var index = Random.Range(0, _loweredFeetAudioData.AudioClip.Count);
                _audioSourceService.PlayOneShot(_loweredFeetAudioData.AudioClip[index]);
            }
            await UniTask.Delay(AudioMilliseconds, cancellationToken: cancellation);
            Despawn(ps);
        }

        void Despawn(ParticleSystem ps)
        {
            ps.gameObject.SetActive(false);
            _pool.Enqueue(ps);
        }

        class FootState
        {
            public float PreHeigth;
            public float ReuseCooldownTime = 0;
            public Transform Foot;
        }
    }
}

