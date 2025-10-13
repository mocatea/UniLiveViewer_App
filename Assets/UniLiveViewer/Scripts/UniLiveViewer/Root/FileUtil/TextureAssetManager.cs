using Cysharp.Threading.Tasks;
using NanaCiel;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace UniLiveViewer
{
    public class TextureAssetManager
    {
        const int MaxSize = 20;
        public VRMNamesData CurrentVRMNamesDatas => _vrmNamesData;
        VRMNamesData _vrmNamesData;

        public IReadOnlyDictionary<string, Sprite> Thumbnails => _thumbnails;
        Dictionary<string, Sprite> _thumbnails;

        readonly Texture2D _texDummy;

        public TextureAssetManager()
        {
            _texDummy = Resources.Load<Texture2D>("Texture/NoImage");
        }

        public void Start()
        {
            _thumbnails = LoadAllPngAsDict(PathsInfo.GetThumbnailsFolderPath(), true);
        }

        Dictionary<string, Sprite> LoadAllPngAsDict(string folderPath, bool allowOverwrite = true)
        {
            var dict = new Dictionary<string, Sprite>();
            if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath))
            {
                Debug.LogWarning($"[PngLoader] Folder not found: {folderPath}");
                return dict;
            }

            var files = Directory.GetFiles(folderPath, "*.png", SearchOption.TopDirectoryOnly);

            foreach (var path in files)
            {
                try
                {
                    // 拡張子なしのファイル名をキーにする
                    var key = Path.GetFileNameWithoutExtension(path);

                    if (!allowOverwrite && dict.ContainsKey(key)) continue;

                    byte[] bytes = File.ReadAllBytes(path);

                    // 適当な初期サイズで作成（LoadImageでリサイズされる）
                    var tex = new Texture2D(2, 2, TextureFormat.RGBA32, mipChain: false, linear: false);

                    if (!tex.LoadImage(bytes, markNonReadable: true))
                    {
                        Object.Destroy(tex);
                        Debug.LogWarning($"[PngLoader] Failed to load image: {path}");
                        continue;
                    }

                    // 既存の場合は八角形
                    dict[key] = tex.CreateSpriteWithOctagon();
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[PngLoader] Error at {path}: {e.Message}");
                }
            }

            return dict;
        }

        /// <summary>
        /// 暫定
        /// </summary>
        public async UniTask CacheThumbnailsAsync(CancellationToken cancellation)
        {
            var charaFolderPath = PathsInfo.GetFullPath(FolderType.Actor) + "/";

            Texture2D texture = null;
            Sprite sprite = null;

            await UniTask.Delay(100, cancellationToken: cancellation);

            _vrmNamesData = new VRMNamesData(GetVrmNames(charaFolderPath));

            var rawData = _vrmNamesData.RawData;

            for (int i = 0; i < rawData.Length; i++)
            {
                sprite = null;
                texture = null;
                sprite = _thumbnails.FirstOrDefault(x => x.Key == _vrmNamesData.RawData[i]).Value;
                
                if (sprite != null)
                {
                    continue;
                }

                try
                {
                    //VRMファイルからサムネイルを抽出する
                    //texture = await VRMExtension.GetThumbnailAsync(charaFolderPath + rawData[i], cancellation);
                    texture = VRMThumbnailPurser.Parse(charaFolderPath + rawData[i]);

                    if (!texture)
                    {
                        //ダミー画像生成
                        texture = GameObject.Instantiate(_texDummy);
                        var rect = new Rect(0, 0, texture.width, texture.height);
                        var pivot = new Vector2(0.5f, 0.5f);
                        sprite = Sprite.Create(texture, rect, pivot);
                        _thumbnails.Add(rawData[i], sprite);
                        continue;
                    }

                    // 新規はアーチ型
                    sprite = texture.CreateSpriteWithArch();
                    _thumbnails.Add(rawData[i], sprite);
#if UNITY_EDITOR
                    texture = TextureFormatter.Resize(texture);
#elif UNITY_ANDROID
                    // NOTE: この処理Quest1キツイ
                    //texture = TextureFormatter.Resize(texture);
#endif
                    //PNG保存
                    var binary = texture.EncodeToPNG();
                    var path = Path.Combine(PathsInfo.GetThumbnailsFolderPath() + "/", $"{rawData[i]}.png");
                    File.WriteAllBytes(path, binary);

                }
                catch (System.OperationCanceledException)
                {
                    Debug.Log("サムネイルキャッシュ中に中断");
                    throw;
                }
                await UniTask.Yield(PlayerLoopTiming.Update, cancellation);
            }
        }

        /// <summary>
        /// フォルダ内VRMファイル名を取得
        /// </summary>
        /// <returns></returns>
        string[] GetVrmNames(string folderPath)
        {
            string[] result = null;
            try
            {
                //VRMファイルのみ検索
                result = Directory.GetFiles(folderPath, "*.vrm", SearchOption.TopDirectoryOnly);

                //ファイルパスからファイル名の抽出
                for (int i = 0; i < result.Length; i++)
                {
                    result[i] = Path.GetFileName(result[i]);
                }
            }
            catch
            {
                result = null;
            }
            return result;
        }

        /// <summary>
        /// VRMファイルをコピー(download→Actor)
        /// </summary>
        public async UniTask CopyVRMtoActorFolderAsync(string folderPath, CancellationToken cancellation)
        {
            _vrmNamesData = new VRMNamesData(GetVrmNames(folderPath));
            var rawData = _vrmNamesData.RawData;

            try
            {
                string charaFolderPath = PathsInfo.GetFullPath(FolderType.Actor) + "/";
                //ファイルコピー
                for (int i = 0; i < rawData.Length; i++)
                {
                    File.Copy(folderPath + rawData[i], charaFolderPath + rawData[i], true);//上書き保存
                }

                //VRMのサムネイル画像をキャッシュする
                await CacheThumbnailsAsync(cancellation);
            }
            catch
            {
                throw;
            }
        }

        public class VRMNamesData
        {
            /// <summary>
            /// 現在は最大20件
            /// </summary>
            public readonly string[] ClampedData;
            public readonly string[] RawData;

            public VRMNamesData(string[] data)
            {
                RawData = data;
                if (MaxSize < data.Length) ClampedData = data.Take(20).ToArray();
                else ClampedData = data;
            }
        }


        /// <summary>
        /// 実機にて不具合あるので次回
        /// </summary>
        //private async UniTaskVoid CacheThumbnail_test()
        //{

        //    string folderPath_Chara = GetFullPath(FOLDERTYPE.CHARA);
        //    string folderPath_Cache = GetFullPath_ThumbnailCache();
        //    Texture2D texture = null;

        //    if(cacheThumbnails != null && cacheThumbnails.Count > 0)
        //    {
        //        cacheThumbnails.Clear();
        //        cacheThumbnails = new Dictionary<string, Texture2D>();
        //    }

        //    await UniTask.Yield(PlayerLoopTiming.Update,cancellation_token);

        //    //全ファイル名を取得
        //    var vrmNames = GetAllVRMNames();
        //    for (int i = 0; i < vrmNames.Length; i++)
        //    {
        //        if (texture) texture = null;
        //        //キャッシュ画像確認
        //        var isCache = File.Exists(folderPath_Cache + vrmNames[i] + ".png");
        //        if (isCache)
        //        {
        //            //キャッシュ済み画像を流用
        //            texture = GetCacheThumbnail(vrmNames[i]);

        //            //リストに追加
        //            cacheThumbnails.Add(vrmNames[i], texture);
        //        }
        //        else
        //        {
        //            try
        //            {
        //                //VRMファイルからサムネイルを抽出する
        //                texture = await vrmRuntimeLoader.GetThumbnail(folderPath_Chara + vrmNames[i], cancellation_token);

        //                if (texture)
        //                {
        //                    //リサイズ
        //                    texture = ResizeTexture(texture, 256, 256);
        //                    //色情報取得(CPU側の処理では色の情報やらを取得できず灰色画像になるので)
        //                    texture = GetColorInfo(texture);
        //                    //リストに追加
        //                    cacheThumbnails.Add(vrmNames[i], texture);
        //                    //フォルダにも保存
        //                    File.WriteAllBytes(folderPath_Cache + vrmNames[i] + ".png", texture.EncodeToPNG());
        //                }
        //                else
        //                {
        //                    //ダミー画像生成
        //                    texture = new Texture2D(1, 1, TextureFormat.RGB24, false);
        //                    //リストに追加
        //                    cacheThumbnails.Add(vrmNames[i], texture);
        //                    //フォルダにも保存
        //                    File.WriteAllBytes(folderPath_Cache + vrmNames[i] + ".png", texture.EncodeToPNG());
        //                }
        //            }
        //            catch (System.OperationCanceledException)
        //            {
        //                Debug.Log("サムネイルキャッシュ中に中断");
        //                throw;
        //            }
        //        }
        //        await UniTask.Yield(PlayerLoopTiming.Update, cancellation_token);
        //    }

        //    //完了した
        //    onThumbnailCompleted?.Invoke();
        //}

        /// <summary>
        /// テクスチャのリサイズ
        /// </summary>
        /// <param name="srcTexture"></param>
        /// <param name="newWidth"></param>
        /// <param name="newHeight"></param>
        /// <returns></returns>
        //public static Texture2D ResizeTexture(Texture2D srcTexture, int newWidth, int newHeight)
        //{
        //    //指定しないとRGBA32になってしまったので一応
        //    var resizedTexture = new Texture2D(newWidth, newHeight, TextureFormat.ARGB32, false);
        //    Graphics.ConvertTexture(srcTexture, resizedTexture);
        //    return resizedTexture;
        //}

        /// <summary>
        /// シェーダー側で描画してテクスチャに書き込むたぶん（大体コピペ）
        /// デプス系を無効化しないとQuest実機では動かなかったので用意
        /// </summary>
        /// <param name="texture2D"></param>
        /// <returns></returns>
        //public static Texture2D GetColorInfo(Texture2D texture2D)
        //{
        //    //Texture mainTexture = _renderer.material.mainTexture;
        //    //RenderTexture currentRT = RenderTexture.active;
        //    RenderTexture renderTexture = new RenderTexture(texture2D.width, texture2D.height, 0);//第三デプス
        //    //Linear→Gamma
        //    Graphics.Blit(texture2D, renderTexture, _renderer.sharedMaterial);//デプス無効化したマテリアル
        //    //RenderTexture情報→texture2Dへ
        //    RenderTexture.active = renderTexture;
        //    texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        //    texture2D.Apply();

        //    //RenderTexture.ReleaseTemporary(renderTexture);


        //    //Color[] pixels = texture2D.GetPixels();
        //    //RenderTexture.active = currentRT;

        //    return texture2D;
        //}

        /// <summary>
        /// キャッシュしたサムネイルを取得
        /// </summary>
        /// <param name="filePath">.png</param>
        /// <returns></returns>
        //private static Texture2D GetCacheThumbnail(string fileName)
        //{
        //    string filePath = Path.Combine(GetFullPath_ThumbnailCache() + "/", $"{fileName}.png");
        //    byte[] bytes = File.ReadAllBytes(filePath);
        //    Texture2D texture = new Texture2D(64, 64);
        //    texture.LoadImage(bytes);
        //    return texture;
        //}
    }

}
