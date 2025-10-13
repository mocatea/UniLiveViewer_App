using Cysharp.Threading.Tasks;
using UnityEngine;

namespace UniLiveViewer
{
    /// <summary>
    /// 一旦Rxしない
    /// </summary>
    public class Button_Switch : Button_Base
    {
        /// <summary>
        /// クリック演出
        /// </summary>
        protected override async UniTaskVoid ClickDirecting()
        {
            //何度も押せないように物理判定を消す
            myRb.isKinematic = true;
            myCol.enabled = false;

            //反転
            isEnable = !isEnable;

            //座標初期化
            collisionChecker.transform.localPosition = Vector3.zero;

            //振動処理
            if (collisionChecker.IsTouchL) ControllerVibration.Execute(OVRInput.Controller.LTouch, 1, 1, 0.1f);
            else ControllerVibration.Execute(OVRInput.Controller.RTouch, 1, 1f, 0.1f);

            await UniTask.Delay((int)(delayTime * 1000), cancellationToken: cancellation_token);

            //張り付かないように少し前進
            collisionChecker.transform.position -= collisionChecker.transform.forward * 0.001f;
            //物理演算を再開
            myRb.isKinematic = false;

            //連続で触れないように一定時間後に触れられるようにする
            await UniTask.Delay(200, cancellationToken: cancellation_token);
            myCol.enabled = true;
        }
    }

}