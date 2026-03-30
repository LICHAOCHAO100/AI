using System;
using System.Diagnostics;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Scripting;
using UnityEngine.UI;

namespace GameLogic
{
    [Preserve]
    public class LToggle : Toggle
    {
        public Text mToggleName;
        public Image mToggleSprite;

        public Action<LToggle, bool> OnValueChanged;
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

            if (!mToggleName)
            {
                mToggleName = GetComponentInChildren<Text>();
            }

            if (!mToggleSprite)
            {
                mToggleSprite = GetComponent<Image>();
            }
        }
#endif

        protected override void Awake()
        {
            base.Awake();

            onValueChanged.AddListener((value) =>
            {
                OnValueChanged?.Invoke(this, value);
            });
        }

        public string SetToggleName
        {
            get
            {
                return mToggleName.text;
            }
            set
            {
                mToggleName.text = value;
            }
        }

        public Sprite SetToggleSprite
        {
            get
            {
                return mToggleSprite.sprite;
            }
            set
            {
                mToggleSprite.sprite = value;
            }
        }
        public void SetToggle(bool value)
        {
            //一般用于首次预制键是打开的状态再打开一次触发
            if (isOn == value)
            {
                onValueChanged?.Invoke(isOn);
                return;
            }
            isOn = value;
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
            {
                return;
            }

            m_IsPointerDown = true;

            if (m_Stopwatch == null)
            {
                m_Stopwatch = new Stopwatch();
            }

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
                    TriggerToggle(eventData);
                }
            }

            base.OnPointerUp(eventData);
        }

        void TriggerToggle(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            base.OnPointerClick(eventData);

            mOnPointerClick?.Invoke(eventData);
        }

        // 禁用外部点击逻辑
        public override void OnPointerClick(PointerEventData eventData)
        {
        }
    }
}