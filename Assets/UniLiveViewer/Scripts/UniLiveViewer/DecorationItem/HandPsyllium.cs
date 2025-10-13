using System.Collections.Generic;
using System.Linq;
using UniLiveViewer;
using UniLiveViewer.Stage;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using UniRx;

// MEMO: 選べるようにしないと邪魔なので音中止
//[RequireComponent(typeof(AudioSource))]
public class HandPsyllium : MonoBehaviour, IItemColorChanger
{
    [SerializeField] Material _material;
    Material _materialInstance;

    [SerializeField] TrailRenderer _trailRenderer;
    [SerializeField] MeshRenderer _meshRenderer;

    //[SerializeField] float _shakeThreshold = 0.1f;  // 揺れを検知するための閾値
    //[SerializeField] float _coolTime = 0.25f;
    //Vector3 _lastPosition;
    //float _timer;
    //bool _canPlaySound = true;

    //AudioHandPsylliumSE _currentAudioHandPsylliumSE;
    //RootAudioSourceService _rootAudioSourceService;
    ////AudioSource _audioSource;
    //List<AudioHandPsylliumDataSet> _audioClipSettings;
    
    void Start()
    {
        //var lifetimeScope = LifetimeScope.Find<StageSceneLifetimeScope>();
        //_rootAudioSourceService = lifetimeScope.Container.Resolve<RootAudioSourceService>();
        //_audioClipSettings = lifetimeScope.Container.Resolve<AudioClipSettings>().AudioHandPsylliumDataSet;
        //_audioSource = GetComponent<AudioSource>();

        //_rootAudioSourceService.SEVolumeRate
        //    .Subscribe(x => _audioSource.volume = x).AddTo(this);

        _materialInstance = Instantiate(_material);
        _meshRenderer.material = _materialInstance;
        //_timer = _coolTime;
    }

    void IItemColorChanger.SetColor(string shaderName, ColorInfo colorInfo)
    {
        var color = colorInfo.ToColor();
        _materialInstance.SetColor(shaderName, color);
        _trailRenderer.material.SetColor("_EmissionColor", color);

        // イベント回すのもアレなので単一破る
        //_currentAudioHandPsylliumSE = colorInfo switch
        //{
        //    ColorInfo.Yellow => AudioHandPsylliumSE.Thunder,
        //    ColorInfo.YellowGreen => AudioHandPsylliumSE.Wind,
        //    ColorInfo.Green => AudioHandPsylliumSE.Wind,
        //    ColorInfo.SkyBlue => AudioHandPsylliumSE.Water,
        //    ColorInfo.Blue => AudioHandPsylliumSE.Water,
        //    ColorInfo.Purple => AudioHandPsylliumSE.Darkness,
        //    ColorInfo.Red => AudioHandPsylliumSE.Flame,
        //    _ => AudioHandPsylliumSE.Default,
        //};
    }

    /*void Update()
    {
        SoundProcess();
        _lastPosition = transform.position;
    }

    void SoundProcess()
    {
        var shakeIntensity = (transform.position - _lastPosition).magnitude;
        // 揺れの強さが閾値を超えた場合にSEを再生
        if (_canPlaySound)
        {
            if (shakeIntensity < _shakeThreshold) return;

            var volume = Mathf.Clamp(shakeIntensity / _shakeThreshold, 0.0f, 1.0f);

            var clip = _audioClipSettings.FirstOrDefault(x => x.AudioType == _currentAudioHandPsylliumSE).AudioClip;
            _audioSource.PlayOneShot(clip);
            _canPlaySound = false;
        }
        else
        {
            if (0 < _timer)
            {
                _timer -= Time.deltaTime;
            }
            else
            {
                _canPlaySound = true;
                _timer = _coolTime;
            }
        }
    }*/
}