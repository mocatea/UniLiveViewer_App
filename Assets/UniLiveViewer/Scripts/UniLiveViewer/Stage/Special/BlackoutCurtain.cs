using UnityEngine;

namespace UniLiveViewer.Stage
{
    /// <summary>
    /// シーン遷移時のPlayerの視界を遮る
    /// </summary>
    public class BlackoutCurtain : MonoBehaviour
    {
        [SerializeField] Animator _animator;

        void Start()
        {
            _animator.Play("Loading");
        }

        public void Opening()
        {
            _animator.Play("Opening");
        }

        public void Closing()
        {
            _animator.Play("Closing");
        }

        /// <summary>
        /// エラーメッセージを表示　←警告やめるので削除予定
        /// </summary>
        public void ShowErrorMessage()
        {
            //Debug.Log("読み込み失敗発生:" + FileReadAndWriteUtility.UserProfile.LanguageCode);

            //if (loadAnimation.gameObject.activeSelf) loadAnimation.gameObject.SetActive(false);

            //int index = FileReadAndWriteUtility.UserProfile.LanguageCode - 1;
            //vmdError[index].gameObject.SetActive(true);
        }
    }
}