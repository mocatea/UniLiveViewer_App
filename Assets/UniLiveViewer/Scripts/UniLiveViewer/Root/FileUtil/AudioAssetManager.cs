using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using UniLiveViewer.SO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using VContainer;

namespace UniLiveViewer
{
    /// <summary>
    /// TODO: エラーハンドリング
    /// </summary>
    public class AudioAssetManager : MonoBehaviour
    {
        const string EXTENSION_MP3 = ".mp3";
        const string EXTENSION_WAV = ".wav";
        const int MAX_STACK = 5;

        public int CurrentPreset => _currentPreset;
        int _currentPreset;

        [SerializeField] List<AudioClip> _stackAudioClips = new List<AudioClip>();
        public IReadOnlyList<string> CustomAudios => _customAudioNames;
        [SerializeField] List<string> _customAudioNames = new List<string>();

        public int CurrentCustom => _currentCustom;
        int _currentCustom;
        string _basePath;

        AudioClipSettings _audioClipSettings;

        [Inject]
        public void Construct(AudioClipSettings audioClipSettings)
        {
            _audioClipSettings = audioClipSettings;
        }

        void Awake()
        {
            _currentPreset = 0;
            _currentCustom = 0;
        }

        async void Start()
        {
            if (SceneManager.GetActiveScene().name == "TitleScene") return;
            if (!RootSystemSettings._isUsedCustomFolders) return;

            // NOTE: 負荷分散で遅延させておく
            await UniTask.Delay(1000);
            _basePath = PathsInfo.GetFullPath(FolderType.BGM) + "/";
            CustomAudioNamesUpdate();
        }

        /// <summary>
        /// カスタム曲名リストの取得
        /// </summary>
        /// <returns></returns>
        void CustomAudioNamesUpdate()
        {
            //初期化
            if (_customAudioNames.Count > 0) _customAudioNames.Clear();
            //フルパス名を追加
            _customAudioNames.AddRange(Directory.GetFiles(_basePath, $"*{EXTENSION_MP3}", SearchOption.TopDirectoryOnly));
            _customAudioNames.AddRange(Directory.GetFiles(_basePath, $"*{EXTENSION_WAV}", SearchOption.TopDirectoryOnly));
        }

        public async UniTask<AudioClip> TryGetCurrentAudioClipAsycn(bool isPreset, CancellationToken cancellation)
        {
            if (isPreset)
            {
                if (_audioClipSettings.AudioBGM.Count == 0) return null;
                return _audioClipSettings.AudioBGM[_currentPreset];
            }
            else
            {
                if (_customAudioNames.Count == 0) return null;
                return await LoadAudioClipAsync(_customAudioNames[_currentCustom], cancellation);
            }
        }

        /// <summary>
        /// 指定カレントのAudioClipを取得する
        /// </summary>
        /// <param name="isPreset"></param>
        /// <param name="addCurrent"></param>
        /// <returns></returns>
        public async UniTask<AudioClip> TryGetAudioClipAsync(CancellationToken cancellation, bool isPreset, int addCurrent)
        {
            cancellation.ThrowIfCancellationRequested();

            if (isPreset)
            {
                if (_audioClipSettings.AudioBGM.Count == 0) return null;
                _currentPreset = IndexNormalization(_currentPreset + addCurrent, _audioClipSettings.AudioBGM.Count);
                return _audioClipSettings.AudioBGM[_currentPreset];
            }
            else
            {
                if (_customAudioNames.Count == 0) return null;
                _currentCustom = IndexNormalization(_currentCustom + addCurrent, _customAudioNames.Count);
                return await LoadAudioClipAsync(_customAudioNames[_currentCustom], cancellation);
            }
        }

        /// <summary>
        /// スタックリストか無ければロードして取得
        /// </summary>
        /// <param name="nextCurrent"></param>
        /// <returns></returns>
        async UniTask<AudioClip> LoadAudioClipAsync(string filePath, CancellationToken cancellation)
        {
            cancellation.ThrowIfCancellationRequested();

            var fileName = Path.GetFileName(filePath);
            var audioClip = _stackAudioClips.FirstOrDefault(x => x.name == fileName);
            if (audioClip) return audioClip;

            audioClip = await TryAudioLoadAsycn(cancellation, filePath);
            if (audioClip is null) return null;
            UpdateStackList(audioClip);
            return audioClip;
        }

        /// <summary>
        /// スタックリストを更新しておく
        /// </summary>
        /// <param name="addAudioClip"></param>
        void UpdateStackList(AudioClip addAudioClip)
        {
            if (_stackAudioClips.Count >= MAX_STACK)
            {
                Destroy(_stackAudioClips[0]);
                _stackAudioClips.RemoveAt(0);
            }
            _stackAudioClips.Add(addAudioClip);
        }

        /// <summary>
        /// 指定pathの曲をロード
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        async UniTask<AudioClip> TryAudioLoadAsycn(CancellationToken cancellation, string filePath)
        {
            var src = $"file://{filePath}";
            var audioType = AudioType.MPEG;
            if (src.Contains(EXTENSION_MP3)) audioType = AudioType.MPEG;
            else if (src.Contains(EXTENSION_WAV)) audioType = AudioType.WAV;

            using (var www = UnityWebRequestMultimedia.GetAudioClip(src, audioType))
            {
                ((DownloadHandlerAudioClip)www.downloadHandler).streamAudio = true;

                await www.SendWebRequest().ToUniTask(cancellationToken: cancellation);

                if (www.result == UnityWebRequest.Result.ConnectionError)
                {
                    Debug.LogError(www.error);
                }
                else
                {
                    var clip = DownloadHandlerAudioClip.GetContent(www);
                    clip.name = Path.GetFileName(src);//ファイル名のみに変える
                    return clip;

                    //Debug.Log("--------------------------------------");
                    //Debug.Log(clip.loadInBackground);
                    //Debug.Log(clip.ambisonic);

                    //Debug.Log(clip.loadType);
                    //Debug.Log(clip.preloadAudioData);

                    //Debug.Log(clip.samples);//サンプル
                    //Debug.Log(clip.channels);//チャンネル
                    //Debug.Log(clip.frequency);//周波数
                }
            }
            return null;
        }

        int IndexNormalization(int nextIndex, int maxIndex)
        {
            if (maxIndex <= nextIndex)
            {
                return nextIndex - maxIndex;
            }
            else if (nextIndex < 0)
            {
                return maxIndex + nextIndex;
            }
            else return nextIndex;
        }
    }
}
