using MessagePipe;
using System;
using System.Collections.Generic;
using System.Linq;
using UniLiveViewer.Actor;
using UniLiveViewer.MessagePipe;
using UniLiveViewer.OVRCustom;
using UniRx;
using UnityEngine;
using VContainer;

namespace UniLiveViewer.Player
{
    public class PlayerHandsService
    {
        readonly IPublisher<AllActorOptionMessage> _allPublisher;
        readonly List<OVRGrabber_UniLiveViewer> _ovrGrabbers;

        [Inject]
        public PlayerHandsService(
            IPublisher<AllActorOptionMessage> actorOperationPublisher,
            List<OVRGrabber_UniLiveViewer> ovrGrabbers)
        {
            _allPublisher = actorOperationPublisher;
            _ovrGrabbers = ovrGrabbers;
        }

        public IReadOnlyReactiveProperty<OVRGrabbableCustom> GrabbedObj(PlayerHandType targetHandType)
        {
            return _ovrGrabbers.FirstOrDefault(x => x.HandType == targetHandType).GrabbedObj;
        }

        public IObservable<PlayerHandActionState> HandActionStateAsObservable(PlayerHandType targetHandType)
        {
            return _ovrGrabbers.FirstOrDefault(x => x.HandType == targetHandType).HandActionStateAsObservable;
        }

        public OVRGrabber_UniLiveViewer GetOVRGrabber(PlayerHandType targetHandType)
        {
            return _ovrGrabbers.FirstOrDefault(x => x.HandType == targetHandType);
        }

        /// <summary>
        /// 両手が空か
        /// </summary>
        /// <returns></returns>
        public bool IsHandsFree()
        {
            for (int i = 0; i < _ovrGrabbers.Count; i++)
            {
                if (!_ovrGrabbers[i].GrabbedObj.Value) continue;
                return false;
            }
            return true;
        }

        /// <summary>
        /// どちらかの手でスライダーを掴んでいるか
        /// </summary>
        public bool IsGrabbingSliderWithHands()
        {
            for (int i = 0; i < _ovrGrabbers.Count; i++)
            {
                if (!_ovrGrabbers[i].GrabbedObj.Value) continue;
                if (!_ovrGrabbers[i].GrabbedObj.Value.gameObject.CompareTag(Constants.TagGrabSliderVolume)) continue;
                return true;
            }
            return false;
        }

        public void OnClickActionButton(PlayerHandType playerHandType)
        {
            // 魔法陣と十字の表示をスイッチ
            var hand = _ovrGrabbers.FirstOrDefault(x => x.HandType == playerHandType);
            if (hand.HandState.Value != PlayerHandState.GRABBED_ITEM)
            {
                hand.SelectorChangeEnabled();
                var isSummonCircle = IsSummonCircleExist();
                SendFuideAnchorMessage(isSummonCircle);
            }

            if (hand.HandState.Value == PlayerHandState.CHARA_ONCIRCLE)
            {
                // 未対応 StateManagerの120行目くらい
            }
            else if (hand.HandState.Value == PlayerHandState.GRABBED_ITEM)
            {
                hand.ItemDecoratorService.OnChangeActionButton();
            }
        }

        public void OnClickTriggerButton(PlayerHandType playerHandType)
        {
            var hand = _ovrGrabbers.FirstOrDefault(x => x.HandType == playerHandType);
            if (hand.HandState.Value == PlayerHandState.GRABBED_ITEM)
            {
                hand.ItemDecoratorService.OnClickTriggerButton();
            }
        }

        public void OnClickStickLeft(PlayerHandType playerHandType)
        {
            var hand = _ovrGrabbers.FirstOrDefault(x => x.HandType == playerHandType);
            if (hand.HandState.Value == PlayerHandState.CHARA_ONCIRCLE)
            {
                hand.AddEulerAnglesGroundPointer(new Vector3(0, +15, 0));
            }
        }

        public void OnClickStickRight(PlayerHandType playerHandType)
        {
            var hand = _ovrGrabbers.FirstOrDefault(x => x.HandType == playerHandType);
            if (hand.HandState.Value == PlayerHandState.CHARA_ONCIRCLE)
            {
                hand.AddEulerAnglesGroundPointer(new Vector3(0, -15, 0));
            }
        }

        /// <summary>
        /// 召喚完了時を想定してガイド非表示
        /// </summary>
        public void IfNeededDeleteGuide()
        {
            var isSummonCircle = IsSummonCircleExist();
            SendFuideAnchorMessage(isSummonCircle);
        }

        /// <summary>
        /// どちらかの手の召喚陣が出現しているか
        /// </summary>
        /// <returns></returns>
        bool IsSummonCircleExist()
        {
            for (int i = 0; i < _ovrGrabbers.Count; i++)
            {
                if (!_ovrGrabbers[i].IsSummonCircle) continue;
                return true;
            }
            return false;
        }

        void SendFuideAnchorMessage(bool isSummonCircle)
        {
            var command = isSummonCircle ? ActorOptionCommand.GUIDE_ANCHOR_ENEBLE : ActorOptionCommand.GUIDE_ANCHOR_DISABLE;
            var fieldMessage = new AllActorOptionMessage(ActorState.FIELD, command);
            _allPublisher.Publish(fieldMessage);

            // 掴んでいる対象向け（TODO:本当はinstanceIDでやるべきだがリファクタが先）
            _allPublisher.Publish(new AllActorOptionMessage(ActorState.ON_CIRCLE, ActorOptionCommand.GUIDE_ANCHOR_ENEBLE));
            _allPublisher.Publish(new AllActorOptionMessage(ActorState.HOLD, ActorOptionCommand.GUIDE_ANCHOR_DISABLE));
            // MEMO: 掴みながらUIは消せないのでminiatureは不要
        }

        public void OnChangeStickInput(PlayerHandType playerHandType, Vector2 v)
        {
            var hand = _ovrGrabbers.FirstOrDefault(x => x.HandType == playerHandType);
            if (hand.HandState.Value == PlayerHandState.GRABBED_ITEM)
            {
                hand.ItemDecoratorService.OnChangeStickInput(v);
            }
        }
    }
}