using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UniGLTF;
using UnityEngine;
using UniVRM10;
using VRM;
using VRMShaders;

namespace NanaCiel
{
    /// <summary>
    /// 廃止、VRMThumbnailPurserを推奨
    /// </summary>
    public static class VRMExtension
    {

        //async Task<Vrm10Instance> LoadAsync(string path)
        //{
        //    var gltfData = new AutoGltfFileParser(path).Parse();
        //    var awaitCaller = new RuntimeOnlyAwaitCaller();
        //    var vrm10Data = await awaitCaller.Run(() => Vrm10Data.Parse(gltfData));
        //    // doMigrate: true で旧バージョンの vrm をロードできます。
        //    // vrm
        //    using (var loader = new Vrm10Importer(vrm10Data))
        //    {
        //        // migrate しても thumbnail は同じ
        //        var thumbnail = await loader.LoadVrmThumbnailAsync();
        //    }
        //}


        /// <summary>
        /// サムネイルのみ取得する
        /// あとは直パースでもしない限り速度誤差なのでとりまこれで
        /// </summary>
        public static async Task<Texture2D> GetThumbnailAsync(string path, CancellationToken cancellation)
        {
            if (string.IsNullOrEmpty(path)) return null;
            if (!File.Exists(path)) return null;

            try
            {
                //https://indie-du.com/entry/2020/11/10/094145
                using (var gltfData = new GlbFileParser(path).Parse())
                //using (var gltfData = new GlbLowLevelParser(string.Empty, File.ReadAllBytes(path)).Parse()) こっちでもクラッシュ
                {
                    //0.X系
                    try
                    {
                        // https://vrm.dev/api/vrm1_load/ マイグレーション方法
                        //Vrm10Data migratedVrm10Data = default;
                        //MigrationData migrationData = default;
                        //using (var migratedGltfData = await awaitCaller.Run(() => Vrm10Data.Migrate(gltfData, out migratedVrm10Data, out migrationData)))
                        //{
                        //}
                        var context = new VRMImporterContext(new VRMData(gltfData));
                        var meta = await context.ReadMetaAsync(new RuntimeOnlyAwaitCaller());
                        var texture = meta.Thumbnail;
                        return texture;
                    }
                    // 1.0系
                    catch
                    {
                        //TODO: 下の処理で一見ロードできたかと思ったら、サムネページ開いた瞬間Unityクラッシュする

                        //var awaitCaller = new RuntimeOnlyAwaitCaller();
                        //var vrm10Data = await awaitCaller.Run(() => Vrm10Data.Parse(gltfData));
                        //using (var loader = new Vrm10Importer(vrm10Data))
                        //{
                        //    // migrate しても thumbnail は同じ
                        //    var thumbnail = await loader.LoadVrmThumbnailAsync();
                        //    Debug.LogWarning("1.0でロードしてサムネを取得");
                        //    return thumbnail;
                        //}
                        return null;
                    }
                }
            }
            catch (NotVrm0Exception)
            {
                Debug.LogWarning("1.0無理だよぉ...");
            }
            catch (System.OperationCanceledException)
            {
                Debug.LogWarning("Thumbnail extraction canceled");
            }
            catch
            {
                Debug.LogWarning("vrm some kind of error");
            }
            return null;
        }
    }
}
