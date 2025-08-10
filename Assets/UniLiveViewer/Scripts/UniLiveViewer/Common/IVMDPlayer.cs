using Cysharp.Threading.Tasks;
using System.Threading;
using UniLiveViewer.External;
using UniLiveViewer.External.UnityVMDReader;

namespace UniLiveViewer
{
    public interface IVMDPlayer
    {
        /// <summary>
        /// ボーンアニメーションを再生
        /// </summary>
        UniTask<VMD> PlayMotionAsync(VMDSetupInfo info, CancellationToken token);

        /// <summary>
        /// 表情アニメーションを再生
        /// </summary>
        UniTask<VMD> PlayExpressionAsync(VMDSetupInfo info, CancellationToken token);

        /// <summary>
        /// アニメーションを停止(ポーズとほぼ同義)
        /// </summary>
        void Stop();

        bool IsPlaying();

        /// <summary>
        /// ボーンアニメーションを再開(Playより軽い)
        /// </summary>
        UniTask ReplayMotionAsync(CancellationToken token);
        /// <summary>
        /// 表情アニメーションを再開(Playより軽い)
        /// </summary>
        UniTask ReplayExpressionAsync(CancellationToken token);

        /// <summary>
        /// 表情更新するか設定
        /// </summary>
        void SetUpdatingFaceSync(bool isEnable);

        /// <summary>
        /// 口パク更新するか設定
        /// </summary>
        void SetUpdatingLipSync(bool isEnable);

        /// <summary>
        /// つま先IKリセット
        /// MEMO: OnLateTickから常時呼ばれる
        /// </summary>
        void ToeIKReset();

        /// <summary>
        /// ポーズの初期化(Aポーズ)
        /// </summary>
        void InitializePose();

        /// <summary>
        /// ボーンと表情アニメーションデータをクリア
        /// </summary>
        void ClearMotionAndExpressionData();

        /// <summary>
        /// 表情アニメーションデータをクリア
        /// </summary>
        void ClearExpressionData();
    }
}