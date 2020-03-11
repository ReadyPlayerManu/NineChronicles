using System;
using System.Collections.Generic;
using Assets.SimpleLocalization;
using JetBrains.Annotations;
using Nekoyume.Game.Controller;
using Nekoyume.Game.VFX;
using Nekoyume.State;
using Nekoyume.UI.Module.Common;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Nekoyume.UI.Module
{
    public class DailyBonus : AlphaAnimateModule
    {
        [SerializeField]
        private SliderAnimator sliderAnimator = null;

        [SerializeField]
        private TextMeshProUGUI text = null;

        [SerializeField]
        private Button additiveGroupButton = null;

        [SerializeField]
        private CanvasGroup additiveGroupCanvas = null;

        [SerializeField]
        private RectTransform tooltipArea = null;

        [SerializeField]
        private Transform boxImageTransform = null;

        [SerializeField]
        private Animator animator;

        [SerializeField, CanBeNull]
        private ActionPoint actionPoint = null;

        private readonly List<IDisposable> _disposables = new List<IDisposable>();
        private long _currentBlockIndex;
        private long _rewardReceivedBlockIndex;
        private bool _isFull;

        private static readonly int IsFull = Animator.StringToHash("IsFull");
        private static readonly int Reward = Animator.StringToHash("GetReward");

        #region Mono

        private void Awake()
        {
            sliderAnimator.OnSliderChange.Subscribe(_ => OnSliderChange()).AddTo(gameObject);
            sliderAnimator.SetMaxValue(GameConfig.DailyRewardInterval);
            sliderAnimator.SetValue(0f, false);

            additiveGroupButton.OnClickAsObservable().Subscribe(_ => GetReward()).AddTo(gameObject);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            additiveGroupCanvas.alpha = 0f;

            if (!(States.Instance.CurrentAvatarState is null))
            {
                SetBlockIndex(Game.Game.instance.Agent.BlockIndex, false);
                SetRewardReceivedBlockIndex(States.Instance.CurrentAvatarState.dailyRewardReceivedIndex, false);
            }

            Game.Game.instance.Agent.BlockIndexSubject.ObserveOnMainThread()
                .Subscribe(x => SetBlockIndex(x, true))
                .AddTo(_disposables);
            ReactiveAvatarState.DailyRewardReceivedIndex
                .Subscribe(x => SetRewardReceivedBlockIndex(x, true))
                .AddTo(_disposables);
        }

        protected override void OnDisable()
        {
            sliderAnimator.Stop();
            _disposables.DisposeAllAndClear();
            base.OnDisable();
        }

        #endregion

        private void SetBlockIndex(long blockIndex, bool useAnimation)
        {
            if (_currentBlockIndex == blockIndex)
                return;

            _currentBlockIndex = blockIndex;
            UpdateSlider(useAnimation);
        }

        private void SetRewardReceivedBlockIndex(long rewardReceivedBlockIndex, bool useAnimation)
        {
            if (_rewardReceivedBlockIndex == rewardReceivedBlockIndex)
                return;

            _rewardReceivedBlockIndex = rewardReceivedBlockIndex;
            UpdateSlider(useAnimation);
        }

        private void UpdateSlider(bool useAnimation)
        {
            var endValue = Math.Min(
                Math.Max(0, _currentBlockIndex - _rewardReceivedBlockIndex),
                GameConfig.DailyRewardInterval);

            sliderAnimator.SetValue(endValue, useAnimation);
        }

        private void OnSliderChange()
        {
            text.text = $"{(int) sliderAnimator.Value} / {sliderAnimator.MaxValue}";

            if (_isFull == sliderAnimator.IsFull)
                return;

            _isFull = sliderAnimator.IsFull;
            additiveGroupCanvas.alpha = _isFull ? 1f : 0f;
            additiveGroupCanvas.interactable = _isFull;
            additiveGroupButton.interactable = _isFull;
            animator.SetBool(IsFull, _isFull);
        }

        private void GetReward()
        {
            Notification.Push(Nekoyume.Model.Mail.MailType.System,
                LocalizationManager.Localize("UI_RECEIVING_DAILY_REWARD"));

            Game.Game.instance.ActionManager.DailyReward().Subscribe(_ =>
            {
                Notification.Push(Nekoyume.Model.Mail.MailType.System,
                    LocalizationManager.Localize("UI_RECEIVED_DAILY_REWARD"));
            });

            _isFull = false;
            additiveGroupCanvas.alpha = 0;
            additiveGroupCanvas.interactable = _isFull;
            additiveGroupButton.interactable = _isFull;
            animator.SetBool(IsFull, _isFull);
            animator.StopPlayback();
            animator.SetTrigger(Reward);
            VFXController.instance.Create<ItemMoveVFX>(boxImageTransform.position);

            if (!(actionPoint is null))
            {
                ItemMoveAnimation.Show(actionPoint.Image.sprite,
                    boxImageTransform.position,
                    actionPoint.Image.transform.position,
                    true,
                    1f,
                    0.8f);
            }
        }

        public void ShowTooltip()
        {
            Widget.Find<VanilaTooltip>()
                .Show("UI_PROSPERITY_DEGREE", "UI_PROSPERITY_DEGREE_DESCRIPTION", tooltipArea.position);
        }

        public void HideTooltip()
        {
            Widget.Find<VanilaTooltip>().Close();
        }
    }
}
