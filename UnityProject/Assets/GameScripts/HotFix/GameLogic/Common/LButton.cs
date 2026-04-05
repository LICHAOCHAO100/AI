using System;
using System.Diagnostics;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Scripting;
using UnityEngine.UI;

namespace GameLogic
{
    [Preserve]
    public class LButton : Button
    {
        public Text mButtonName;
        public Image mButtonSprite;

        public Action<LButton> OnClick;
        public Action<PointerEventData> mOnPointerClick;
        public Action<PointerEventData> mOnPointerEnter;
        public Action<PointerEventData> mOnPointerExit;
        public Action<PointerEventData> mLongOnPointerUp;

        private Stopwatch m_Stopwatch;
        private bool m_IsPointerDown;

#if UNITY_EDITOR
        protected override void Reset()
        {
            base.Reset();

            if (!mButtonName)
                mButtonName = GetComponentInChildren<Text>();

            if (!mButtonSprite)
                mButtonSprite = GetComponent<Image>();
        }
#endif

        protected override void Awake()
        {
            base.Awake();

            onClick.AddListener(() =>
            {
                OnClick?.Invoke(this);
            });
        }

        public string SetButtonName
        {
            get { return mButtonName.text; }
            set { mButtonName.text = value; }
        }

        public Sprite SetButtonSprite
        {
            get { return mButtonSprite.sprite; }
            set { mButtonSprite.sprite = value; }
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);

            if (interactable)
            {
                mOnPointerEnter?.Invoke(eventData);
            }
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);

            if (interactable)
            {
                mOnPointerExit?.Invoke(eventData);
            }
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);

            if (!IsActive() || !IsInteractable())
                return;

            m_IsPointerDown = true;

            if (m_Stopwatch == null)
                m_Stopwatch = new Stopwatch();

            m_Stopwatch.Restart();
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            if (!IsActive() || !IsInteractable())
            {
                base.OnPointerUp(eventData);
                return;
            }

            if (m_IsPointerDown)
            {
                m_IsPointerDown = false;

                m_Stopwatch.Stop();

                long pressTime = m_Stopwatch.ElapsedMilliseconds;

                if (pressTime > 1500)
                {
                    mLongOnPointerUp?.Invoke(eventData);
                }
                else
                {
                    // 自己触发Click
                    TriggerClick(eventData);
                }
            }

            base.OnPointerUp(eventData);
        }

        void TriggerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            // Unity Button 的事件
            onClick?.Invoke();

            // 自定义点击
            mOnPointerClick?.Invoke(eventData);
        }

        // 禁用Unity默认Click逻辑（避免冲突）
        public override void OnPointerClick(PointerEventData eventData)
        {
        }
    }
}