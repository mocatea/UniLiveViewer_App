using Cysharp.Threading.Tasks;
using System.Threading;
using UniLiveViewer.External;
using UniLiveViewer.External.UnityVMDReader;

namespace UniLiveViewer
{
    public interface IVMDPlayer
    {
        /// <summary>
        /// VMDダンス再生
        /// </summary>
        UniTask<VMD> SetupBaseMotionAsync(VMDSetupInfo info, CancellationToken token);

        /// <summary>
        /// VMD表情再生
        /// </summary>
        UniTask<VMD> SetupExpressionAsync(VMDSetupInfo info, CancellationToken token);

        /// <summary>
        /// 表情更新
        /// </summary>
        void SetFaceUpdate(bool isEnable);

        /// <summary>
        /// 口パク更新
        /// </summary>
        void SetLipUpdate(bool isEnable);

        /// <summary>
        /// つま先IKリセット
        /// </summary>
        void ToeIKReset();

        /// <summary>
        /// ポーズの初期化(Aポーズ)
        /// </summary>
        void InitializePose();

        /// <summary>
        /// 全リセット
        /// </summary>
        void ClearBaseAndSyncData();

        void ClearSyncData();
    }
}