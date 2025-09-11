using UnityEngine;

namespace UniLiveViewer
{
    public static class Constants
    {
        //レイヤー
        public static int LayerNoDefault = LayerMask.NameToLayer("Default");
        public static int LayerNoVirtualHead = LayerMask.NameToLayer("VirtualHead");
        public static int LayerNoIgnoreRaycats = LayerMask.NameToLayer("Ignore Raycast");
        public static int LayerNoUI = LayerMask.NameToLayer("UI");
        public static int LayerNoFieldObject = LayerMask.NameToLayer("FieldObject");
        public static int LayerNoGrabObject = LayerMask.NameToLayer("GrabObject");
        public static int LayerActorFace = LayerMask.NameToLayer("ActorFace");

        //レイヤーマスク(Raycast系はこっち)
        public static int LayerMaskDefault = LayerMask.GetMask("Default");
        public static int LayerMaskVirtualHead = LayerMask.GetMask("VirtualHead");
        public static int LayerMaskStageFloor = LayerMask.GetMask("Stage_Floor");
        public static int LayerMaskFieldObject = LayerMask.GetMask("FieldObject");

        //タグ
        public static readonly string TagItemMaterial = "ItemMaterial";
        public static readonly string TagGrabChara = "Grab_Chara";
        public static readonly string TagGrabSliderVolume = "Grab_Slider_Volume";

        //一括ボタンカラー(仮)
        public static readonly Color btnColor_Ena_sky = new Color(0, 1, 1, 1);
        public static readonly Color btnColor_Dis = new Color(0.4f, 0.4f, 0.4f, 1);
    }
}
