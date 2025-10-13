
namespace UniLiveViewer.Actor
{
    public enum ActorType
    {
        FBX,
        VRM
    }

    public enum ExpressionType
    {
        NULL = 0,
        /// <summary> UV方式 </summary>
        UnityChan,
        /// <summary> UV方式 </summary>
        CandyChan,
        /// <summary> Bone方式 </summary>
        UnityChanSSU,
        /// <summary> Bone方式 </summary>
        UnityChanSD,
        /// <summary> BlendShape方式 </summary>
        VketChan,
        /// <summary> Bone方式 </summary>
        UnityChanKAGURA,
        /// <summary> UV方式、ほとんどいない </summary>
        VRM_UV,
        /// <summary> Bone方式 </summary>
        VRM_Bone,
        /// <summary> BlendShape方式 </summary>
        VRM_BlendShape,
        /// <summary> VRM1.0版 </summary>
        VRM10,
    }

    public enum ActorState
    {
        /// <summary>
        /// 未設定
        /// </summary>
        NULL = -1,
        /// <summary>
        /// メニュー上の人形サイズ
        /// </summary>
        MINIATURE,
        /// <summary>
        /// Playerにホールドされている
        /// </summary>
        HOLD,
        /// <summary>
        /// 召喚陣上
        /// </summary>
        ON_CIRCLE,
        /// <summary>
        /// ステージ上・召喚済み
        /// </summary>
        FIELD,
    }

    public enum ActorCommand
    {
        /// <summary>
        /// RootGameObjectのアクティブ識別用
        /// </summary>
        ACTIVE,
        /// <summary>
        /// RootGameObjectの非アクティブ識別用
        /// </summary>
        INACTIVE,
        /// <summary>
        /// 今はフィールド一掃のみ利用
        /// </summary>
        DELETE,
        //以下一緒にしていいかなぁ
        FACILSYNC_ENEBLE,
        FACILSYNC_DISABLE,
        LIPSYNC_ENEBLE,
        LIPSYNC_DISABLE,
        TIMELINE_PLAY,
        TIMELINE_PAUSE,
        TIMELINE_STOP,
        UPDATE_SHADOW
    }

    public enum ActorOptionCommand
    {
        GUIDE_ANCHOR_ENEBLE,
        GUIDE_ANCHOR_DISABLE,
    }

    public enum LIPTYPE
    {
        A = 0,
        I,
        U,
        E,
        O
    }

    public enum FACIALTYPE
    {
        /// <summary>
        /// 瞬き・寝てる時の目
        /// </summary>
        BLINK = 0,
        /// <summary>
        /// 喜び
        /// </summary>
        JOY,
        /// <summary>
        /// 怒り
        /// </summary>
        ANGRY,
        /// <summary>
        /// 悲しみ
        /// </summary>
        SORROW,
        /// <summary>
        /// 驚き
        /// </summary>
        SUP,
        /// <summary>
        /// 楽しい
        /// </summary>
        FUN,
        /// <summary>
        /// 左目閉じ
        /// </summary>
        WINK_L,
        /// <summary>
        /// 右目閉じ
        /// </summary>
        WINK_R,
    }
}