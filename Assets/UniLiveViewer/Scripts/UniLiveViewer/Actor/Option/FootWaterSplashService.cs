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
        const float SplashReuseDelayTime = 0.25f;
        const float WaterMoveReuseDelayTime = 0.2f;
        const int MaxPoolCount = 8;

        float _actorRootScalar = 1;
        float _actorHeight = 0;
        FootState _lFootState;
        FootState _rFootState;
        readonly Queue<ParticleSystem> _pool = new();

        readonly Transform _parent;
        readonly FootWaterSplashSettings _settings;
        readonly AudioSourceService _audioSourceService;
        readonly FootstepAudioData _footWaterMoveAudioData;
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
            _footWaterMoveAudioData = setting.FootWaterMoveAudioData;
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

        public void SetVolume(float volume)
        {
            _audioSourceService.SetVolume(volume);
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

        public async UniTask OnFixedTickAsync(CancellationToken cancellation)
        {
            if (_lFootState == null || _rFootState == null) return;

            WaterMoeCheck(_lFootState);
            WaterMoeCheck(_rFootState);
            HitCheckAsync(_lFootState, cancellation).Forget();
            HitCheckAsync(_rFootState, cancellation).Forget();

            _lFootState.PrePos = _lFootState.Foot.position;
            _rFootState.PrePos = _rFootState.Foot.position;

            await UniTask.CompletedTask;
        }

        void WaterMoeCheck(FootState foot)
        {
            if (0 < foot.WaterMoveCooldownTime)
            {
                foot.WaterMoveCooldownTime -= Time.deltaTime;
                return;
            }

            var waterLevel = _settings.ReferenceWaterLevel;
            var preY = foot.PrePos.y;
            var nowY = foot.Foot.position.y;

            // 水中に入っていない
            if (waterLevel < preY && waterLevel < nowY)
            {
                return;
            }

            // 水中で移動
            if (preY < waterLevel && nowY < waterLevel)
            {
                // ある程度の動きがない
                if (Vector3.SqrMagnitude(foot.PrePos - foot.Foot.position) < _settings.Distance)
                {
                    return;
                }

                UnderwaterMovement(foot.Foot.position);
                foot.WaterMoveCooldownTime = WaterMoveReuseDelayTime;
            }
        }

        async UniTask HitCheckAsync(FootState foot, CancellationToken cancellation)
        {
            if (0 < foot.SplashCooldownTime)
            {
                foot.SplashCooldownTime -= Time.deltaTime;
                return;
            }

            var waterLevel = _settings.ReferenceWaterLevel;
            var preY = foot.PrePos.y;
            var nowY = foot.Foot.position.y;
            // 水中に入っていない
            if (waterLevel < preY && waterLevel < nowY)
            {
                return;
            }

            // 水中で移動
            if (preY < waterLevel && nowY < waterLevel)
            {
                return;
            }

            // 入水か出水
            var isRaisedFeet = false;
            if (preY < waterLevel && waterLevel < nowY)
            {
                isRaisedFeet = true;
            }
            else if (nowY < waterLevel && waterLevel < preY)
            {
                isRaisedFeet = false;
            }
            else
            {
                return;
            }

            var euler = new Vector3(0, Random.Range(0, 360), 0);
            var pos = foot.Foot.position;
            pos.y = waterLevel;
            SpawnAsync(pos, Quaternion.Euler(euler), isRaisedFeet, cancellation).Forget();
            foot.SplashCooldownTime = SplashReuseDelayTime;

            await UniTask.CompletedTask;
        }

        void UnderwaterMovement(Vector3 pos)
        {
            var index = Random.Range(0, _footWaterMoveAudioData.AudioClip.Count);
            _audioSourceService.PlayOneShot(_footWaterMoveAudioData.AudioClip[index], pos);
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

            if (isRaisedFeet)
            {
                var index = Random.Range(0, _raisedfootAudioData.AudioClip.Count);
                _audioSourceService.PlayOneShot(_raisedfootAudioData.AudioClip[index], pos);
            }
            else
            {
                var index = Random.Range(0, _loweredFeetAudioData.AudioClip.Count);
                _audioSourceService.PlayOneShot(_loweredFeetAudioData.AudioClip[index], pos);
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
            public Vector3 PrePos;
            public float SplashCooldownTime = 0;
            public float WaterMoveCooldownTime = 0;
            public Transform Foot;
        }
    }
}

