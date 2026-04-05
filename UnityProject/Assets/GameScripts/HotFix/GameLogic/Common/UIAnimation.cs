using System;
using UnityEngine;
using static GameLogic.UIAnimation;
using UnityEngine.Events;
using UnityEngine.UI;
using LitMotion;
using Cysharp.Threading.Tasks;
using LitMotion.Extensions;


#if UNITY_EDITOR
using UnityEditorInternal;
using UnityEditor;
#endif
namespace GameLogic
{
#if UNITY_EDITOR
    #region Editor Inspector
    [CanEditMultipleObjects]
    [CustomEditor(typeof(UIAnimation))]
    class UIAnimationInspector : Editor
    {
        SerializedProperty m_Sequence;
        ReorderableList m_SequenceList;

        GUIContent m_PlayBtnContent;
        GUIContent m_RewindBtnContent;
        GUIContent m_ResetBtnContent;
        private void OnEnable()
        {
            m_PlayBtnContent = EditorGUIUtility.TrIconContent("d_PlayButton@2x", "播放");
            m_RewindBtnContent = EditorGUIUtility.TrIconContent("d_preAudioAutoPlayOff@2x", "倒放");
            m_ResetBtnContent = EditorGUIUtility.TrIconContent("d_preAudioLoopOff@2x", "重置");
            m_Sequence = serializedObject.FindProperty("m_Sequence");
            m_SequenceList = new ReorderableList(serializedObject, m_Sequence);
            m_SequenceList.drawElementCallback = OnDrawSequenceItem;
            m_SequenceList.elementHeightCallback = index =>
            {
                var item = m_Sequence.GetArrayElementAtIndex(index);
                return EditorGUI.GetPropertyHeight(item);
            };
            m_SequenceList.drawHeaderCallback = OnDrawSequenceHeader;
        }

        private void OnDisable()
        {
            (target as UIAnimation).Cancel();
        }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button(m_PlayBtnContent))
                {
                    (target as UIAnimation).Play();
                }
                if (GUILayout.Button(m_RewindBtnContent))
                {
                    (target as UIAnimation).Play(true);
                }
                if (GUILayout.Button(m_ResetBtnContent))
                {
                    (target as UIAnimation).Cancel(true);
                }
                EditorGUILayout.EndHorizontal();
            }

            serializedObject.Update();
            m_SequenceList.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }

        private void OnDrawSequenceHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Animation Sequences");
        }
        private void OnDrawSequenceItem(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = m_Sequence.GetArrayElementAtIndex(index);
            EditorGUI.PropertyField(rect, element, true);
        }
    }

    [CustomPropertyDrawer(typeof(UIAnimationItem))]
    class UIAnimationItemDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var onComplete = property.FindPropertyRelative("OnComplete");
            return EditorGUIUtility.singleLineHeight * 11 + (property.isExpanded ? EditorGUI.GetPropertyHeight(onComplete) : 0);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.indentLevel++;
            var target = property.FindPropertyRelative("Target");
            var addType = property.FindPropertyRelative("AddType");
            var tweenType = property.FindPropertyRelative("AnimationType");
            var toValue = property.FindPropertyRelative("ToValue");
            var useToTarget = property.FindPropertyRelative("UseToTarget");
            var toTarget = property.FindPropertyRelative("ToTarget");
            var fromValue = property.FindPropertyRelative("FromValue");
            var duration = property.FindPropertyRelative("DurationOrSpeed");
            var speedBased = property.FindPropertyRelative("SpeedBased");
            var delay = property.FindPropertyRelative("Delay");
            var customEase = property.FindPropertyRelative("CustomEase");
            var ease = property.FindPropertyRelative("Ease");
            var easeCurve = property.FindPropertyRelative("EaseCurve");
            var loops = property.FindPropertyRelative("Loops");
            var loopType = property.FindPropertyRelative("LoopType");

            var onComplete = property.FindPropertyRelative("OnComplete");

            var lastRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(lastRect, addType);

            lastRect.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(lastRect, target);
            lastRect.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(lastRect, tweenType);
            if (EditorGUI.EndChangeCheck())
            {
                var fixedComType = GetFixedComponentType(target.objectReferenceValue as Component, (DOTweenType)tweenType.enumValueIndex);
                if (fixedComType != null)
                {
                    target.objectReferenceValue = fixedComType;
                    SetValueFromTarget((DOTweenType)tweenType.enumValueIndex, target, fromValue);
                }
            }

            if (target.objectReferenceValue != null && null == GetFixedComponentType(target.objectReferenceValue as Component, (DOTweenType)tweenType.enumValueIndex))
            {
                lastRect.y += EditorGUIUtility.singleLineHeight;
                EditorGUI.HelpBox(lastRect, string.Format("{0}不支持{1}", target.objectReferenceValue == null ? "Target" : target.objectReferenceValue.GetType().Name, tweenType.enumDisplayNames[tweenType.enumValueIndex]), MessageType.Error);
            }
            const float itemWidth = 110;
            const float setBtnWidth = 35;
            //Delay
            lastRect.y += EditorGUIUtility.singleLineHeight;
            var horizontalRect = lastRect;
            horizontalRect.width -= setBtnWidth + itemWidth;
            EditorGUI.PropertyField(horizontalRect, delay);

            //From Value
            lastRect.y += EditorGUIUtility.singleLineHeight;
            horizontalRect = lastRect;
            horizontalRect.width -= setBtnWidth + itemWidth;



            //ToTarget
            lastRect.y += EditorGUIUtility.singleLineHeight;
            var toRect = lastRect;
            toRect.width -= setBtnWidth + itemWidth;

            //To Value
            var dotweenTp = (DOTweenType)tweenType.enumValueIndex;
            switch (dotweenTp)
            {
                case DOTweenType.DOMoveX:
                case DOTweenType.DOMoveY:
                case DOTweenType.DOMoveZ:
                case DOTweenType.DOLocalMoveX:
                case DOTweenType.DOLocalMoveY:
                case DOTweenType.DOLocalMoveZ:
                case DOTweenType.DOAnchorPosX:
                case DOTweenType.DOAnchorPosY:
                case DOTweenType.DOAnchorPosZ:
                case DOTweenType.DOFade:
                case DOTweenType.DOCanvasGroupFade:
                case DOTweenType.DOFillAmount:
                case DOTweenType.DOValue:
                case DOTweenType.DOScaleX:
                case DOTweenType.DOScaleY:
                case DOTweenType.DOScaleZ:
                    {
                        var value = fromValue.vector4Value;
                        value.x = EditorGUI.FloatField(horizontalRect, "From", value.x);
                        fromValue.vector4Value = value;

                        if (!useToTarget.boolValue)
                        {
                            value = toValue.vector4Value;
                            value.x = EditorGUI.FloatField(toRect, "To", value.x);
                            toValue.vector4Value = value;
                        }
                    }
                    break;
                case DOTweenType.DOAnchorPos:
                case DOTweenType.DOFlexibleSize:
                case DOTweenType.DOMinSize:
                case DOTweenType.DOPreferredSize:
                case DOTweenType.DOSizeDelta:
                    {
                        fromValue.vector4Value = EditorGUI.Vector2Field(horizontalRect, "From", fromValue.vector4Value);
                        if (!useToTarget.boolValue)
                            toValue.vector4Value = EditorGUI.Vector2Field(toRect, "To", toValue.vector4Value);
                    }
                    break;
                case DOTweenType.DOMove:
                case DOTweenType.DOLocalMove:
                case DOTweenType.DOAnchorPos3D:
                case DOTweenType.DOScale:
                case DOTweenType.DORotate:
                case DOTweenType.DOLocalRotate:
                    {
                        fromValue.vector4Value = EditorGUI.Vector3Field(horizontalRect, "From", fromValue.vector4Value);
                        if (!useToTarget.boolValue)
                            toValue.vector4Value = EditorGUI.Vector3Field(toRect, "To", toValue.vector4Value);
                    }
                    break;
                case DOTweenType.DOColor:
                    {
                        fromValue.vector4Value = EditorGUI.ColorField(horizontalRect, "From", fromValue.vector4Value);
                        if (!useToTarget.boolValue)
                            toValue.vector4Value = EditorGUI.ColorField(toRect, "To", toValue.vector4Value);
                    }
                    break;
            }
            if (useToTarget.boolValue)
            {
                toTarget.objectReferenceValue = EditorGUI.ObjectField(toRect, "To", toTarget.objectReferenceValue, target.objectReferenceValue != null ? target.objectReferenceValue.GetType() : typeof(Component), true);

                if (toTarget.objectReferenceValue == null)
                {
                    lastRect.y += EditorGUIUtility.singleLineHeight;
                    EditorGUI.HelpBox(lastRect, "To target cannot be null.", MessageType.Error);
                }
            }
            horizontalRect.x += horizontalRect.width;
            horizontalRect.width = setBtnWidth;
            if (GUI.Button(horizontalRect, "Set"))
            {
                SetValueFromTarget(dotweenTp, target, fromValue);
            }

            toRect.x += toRect.width;
            toRect.width = setBtnWidth;
            if (!useToTarget.boolValue && GUI.Button(toRect, "Set"))
            {
                SetValueFromTarget(dotweenTp, target, toValue);
            }
            toRect.x += setBtnWidth;
            toRect.width = itemWidth;
            useToTarget.boolValue = EditorGUI.ToggleLeft(toRect, "ToTarget", useToTarget.boolValue);

            //Duration
            lastRect.y += EditorGUIUtility.singleLineHeight;
            horizontalRect = lastRect;
            horizontalRect.width -= setBtnWidth + itemWidth;
            EditorGUI.PropertyField(horizontalRect, duration);
            horizontalRect.x += setBtnWidth + horizontalRect.width;
            horizontalRect.width = itemWidth;
            speedBased.boolValue = EditorGUI.ToggleLeft(horizontalRect, "Use Speed", speedBased.boolValue);

            //Ease
            lastRect.y += EditorGUIUtility.singleLineHeight;
            horizontalRect = lastRect;
            horizontalRect.width -= setBtnWidth + itemWidth;
            if (customEase.boolValue)
                EditorGUI.PropertyField(horizontalRect, easeCurve);
            else
                EditorGUI.PropertyField(horizontalRect, ease);
            horizontalRect.x += setBtnWidth + horizontalRect.width;
            horizontalRect.width = itemWidth;
            customEase.boolValue = EditorGUI.ToggleLeft(horizontalRect, "Use Curve", customEase.boolValue);

            //Loops
            lastRect.y += EditorGUIUtility.singleLineHeight;
            horizontalRect = lastRect;
            horizontalRect.width -= setBtnWidth + itemWidth;
            EditorGUI.PropertyField(horizontalRect, loops);
            horizontalRect.x += setBtnWidth + horizontalRect.width;
            horizontalRect.width = itemWidth;
            EditorGUI.BeginDisabledGroup(loops.intValue == 1);
            loopType.enumValueIndex = (int)(LoopType)EditorGUI.EnumPopup(horizontalRect, (LoopType)loopType.enumValueIndex);
            EditorGUI.EndDisabledGroup();

            //Events
            lastRect.y += EditorGUIUtility.singleLineHeight;
            property.isExpanded = EditorGUI.Foldout(lastRect, property.isExpanded, "Animation Events");
            if (property.isExpanded)
            {
                //OnComplete
                lastRect.y += EditorGUIUtility.singleLineHeight;
                EditorGUI.PropertyField(lastRect, onComplete);
            }

            EditorGUI.indentLevel--;
            EditorGUI.EndProperty();
        }

        static void SetValueFromTarget(DOTweenType tweenType, SerializedProperty target, SerializedProperty value)
        {
            if (target.objectReferenceValue == null) return;
            var targetCom = target.objectReferenceValue;
            switch (tweenType)
            {
                case DOTweenType.DOMove:
                    {
                        value.vector4Value = (targetCom as Transform).position;
                        break;
                    }
                case DOTweenType.DOMoveX:
                    {
                        var tmpValue = value.vector4Value;
                        tmpValue.x = (targetCom as Transform).position.x;
                        value.vector4Value = tmpValue;
                        break;
                    }
                case DOTweenType.DOMoveY:
                    {
                        var tmpValue = value.vector4Value;
                        tmpValue.x = (targetCom as Transform).position.y;
                        value.vector4Value = tmpValue;
                        break;
                    }
                case DOTweenType.DOMoveZ:
                    {
                        var tmpValue = value.vector4Value;
                        tmpValue.x = (targetCom as Transform).position.z;
                        value.vector4Value = tmpValue;
                        break;
                    }
                case DOTweenType.DOLocalMove:
                    {
                        value.vector4Value = (targetCom as Transform).localPosition;
                        break;
                    }
                case DOTweenType.DOLocalMoveX:
                    {
                        var tmpValue = value.vector4Value;
                        tmpValue.x = (targetCom as Transform).localPosition.x;
                        value.vector4Value = tmpValue;
                        break;
                    }
                case DOTweenType.DOLocalMoveY:
                    {
                        var tmpValue = value.vector4Value;
                        tmpValue.x = (targetCom as Transform).localPosition.y;
                        value.vector4Value = tmpValue;
                        break;
                    }
                case DOTweenType.DOLocalMoveZ:
                    {
                        var tmpValue = value.vector4Value;
                        tmpValue.x = (targetCom as Transform).localPosition.z;
                        value.vector4Value = tmpValue;
                        break;
                    }
                case DOTweenType.DOAnchorPos:
                    {
                        value.vector4Value = (targetCom as RectTransform).anchoredPosition;
                        break;
                    }
                case DOTweenType.DOAnchorPosX:
                    {
                        var tmpValue = value.vector4Value;
                        tmpValue.x = (targetCom as RectTransform).anchoredPosition.x;
                        value.vector4Value = tmpValue;
                        break;
                    }
                case DOTweenType.DOAnchorPosY:
                    {
                        var tmpValue = value.vector4Value;
                        tmpValue.x = (targetCom as RectTransform).anchoredPosition.y;
                        value.vector4Value = tmpValue;
                        break;
                    }
                case DOTweenType.DOAnchorPosZ:
                    {
                        var tmpValue = value.vector4Value;
                        tmpValue.x = (targetCom as RectTransform).anchoredPosition3D.z;
                        value.vector4Value = tmpValue;
                        break;
                    }
                case DOTweenType.DOAnchorPos3D:
                    {
                        value.vector4Value = (targetCom as RectTransform).anchoredPosition3D;
                        break;
                    }
                case DOTweenType.DOColor:
                    {
                        value.vector4Value = (targetCom as UnityEngine.UI.Graphic).color;
                        break;
                    }
                case DOTweenType.DOFade:
                    {
                        var tmpValue = value.vector4Value;
                        tmpValue.x = (targetCom as UnityEngine.UI.Graphic).color.a;
                        value.vector4Value = tmpValue;
                        break;
                    }
                case DOTweenType.DOCanvasGroupFade:
                    {
                        var tmpValue = value.vector4Value;
                        tmpValue.x = (targetCom as UnityEngine.CanvasGroup).alpha;
                        value.vector4Value = tmpValue;
                        break;
                    }
                case DOTweenType.DOValue:
                    {
                        var tmpValue = value.vector4Value;
                        tmpValue.x = (targetCom as UnityEngine.UI.Slider).value;
                        value.vector4Value = tmpValue;
                        break;
                    }
                case DOTweenType.DOSizeDelta:
                    {
                        value.vector4Value = (targetCom as RectTransform).sizeDelta;
                        break;
                    }
                case DOTweenType.DOFillAmount:
                    {
                        var tmpValue = value.vector4Value;
                        tmpValue.x = (targetCom as UnityEngine.UI.Image).fillAmount;
                        value.vector4Value = tmpValue;
                        break;
                    }
                case DOTweenType.DOFlexibleSize:
                    {
                        value.vector4Value = (targetCom as LayoutElement).GetFlexibleSize();
                        break;
                    }
                case DOTweenType.DOMinSize:
                    {
                        value.vector4Value = (targetCom as LayoutElement).GetMinSize();
                        break;
                    }
                case DOTweenType.DOPreferredSize:
                    {
                        value.vector4Value = (targetCom as LayoutElement).GetPreferredSize();
                        break;
                    }
                case DOTweenType.DOScale:
                    {
                        value.vector4Value = (targetCom as Transform).localScale;
                        break;
                    }
                case DOTweenType.DOScaleX:
                    {
                        var tmpValue = value.vector4Value;
                        tmpValue.x = (targetCom as Transform).localScale.x;
                        value.vector4Value = tmpValue;
                        break;
                    }
                case DOTweenType.DOScaleY:
                    {
                        var tmpValue = value.vector4Value;
                        tmpValue.x = (targetCom as Transform).localScale.y;
                        value.vector4Value = tmpValue;
                        break;
                    }
                case DOTweenType.DOScaleZ:
                    {
                        var tmpValue = value.vector4Value;
                        tmpValue.x = (targetCom as Transform).localScale.z;
                        value.vector4Value = tmpValue;
                        break;
                    }
                case DOTweenType.DORotate:
                    {
                        value.vector4Value = (targetCom as Transform).eulerAngles;
                        break;
                    }
                case DOTweenType.DOLocalRotate:
                    {
                        value.vector4Value = (targetCom as Transform).localEulerAngles;
                        break;
                    }
            }
        }

        private static Component GetFixedComponentType(Component com, DOTweenType tweenType)
        {
            if (com == null) return null;
            switch (tweenType)
            {
                case DOTweenType.DOMove:
                case DOTweenType.DOMoveX:
                case DOTweenType.DOMoveY:
                case DOTweenType.DOMoveZ:
                case DOTweenType.DOLocalMove:
                case DOTweenType.DOLocalMoveX:
                case DOTweenType.DOLocalMoveY:
                case DOTweenType.DOLocalMoveZ:
                case DOTweenType.DOScale:
                case DOTweenType.DOScaleX:
                case DOTweenType.DOScaleY:
                case DOTweenType.DOScaleZ:
                    return com.gameObject.GetComponent<Transform>();
                case DOTweenType.DOAnchorPos:
                case DOTweenType.DOAnchorPosX:
                case DOTweenType.DOAnchorPosY:
                case DOTweenType.DOAnchorPosZ:
                case DOTweenType.DOAnchorPos3D:
                case DOTweenType.DOSizeDelta:
                    return com.gameObject.GetComponent<RectTransform>();
                case DOTweenType.DOColor:
                case DOTweenType.DOFade:
                    return com.gameObject.GetComponent<UnityEngine.UI.Graphic>();
                case DOTweenType.DOCanvasGroupFade:
                    return com.gameObject.GetComponent<UnityEngine.CanvasGroup>();
                case DOTweenType.DOFillAmount:
                    return com.gameObject.GetComponent<UnityEngine.UI.Image>();
                case DOTweenType.DOFlexibleSize:
                case DOTweenType.DOMinSize:
                case DOTweenType.DOPreferredSize:
                    return com.gameObject.GetComponent<UnityEngine.UI.LayoutElement>();
                case DOTweenType.DOValue:
                    return com.gameObject.GetComponent<UnityEngine.UI.Slider>();

            }
            return null;
        }
    }
    #endregion
#endif
    public class UIAnimation : MonoBehaviour
    {
        [SerializeField] bool m_PlayOnAwake = false;
        [HideInInspector][SerializeField] UIAnimationItem[] m_Sequence;
        [SerializeField] float m_Delay = 0;
        [SerializeField] bool m_UnscaledTime = true;
        [SerializeField] UnityEvent m_OnComplete = null;

        private MotionHandle m_Tween;
        private IMotionScheduler m_Scheduler;
        private bool m_Rewind = false;
        public bool IsPlaying => (m_Tween != MotionHandle.None) && m_Tween.IsPlaying();
        private void Awake()
        {
            InitTween();
            if (m_PlayOnAwake) Play();
        }
        private void Update()
        {
            if (m_Tween == MotionHandle.None || m_OnComplete == null) return;

            if (m_Tween.IsActive() && m_Tween.CompletedLoops == 1)
            {
                Stop();
                m_OnComplete.Invoke();
            }
        }
        private void OnDisable()
        {
            Stop();
        }
        private void InitTween()
        {
            m_Scheduler = m_UnscaledTime ? MotionScheduler.Update : MotionScheduler.UpdateIgnoreTimeScale;

            foreach (var item in m_Sequence)
            {
                var targetCom = item.Target;
                var resetValue = item.FromValue;
                switch (item.AnimationType)
                {
                    case DOTweenType.DOMove:
                        {
                            (targetCom as Transform).position = resetValue;
                            break;
                        }
                    case DOTweenType.DOMoveX:
                        {
                            (targetCom as Transform).SetPositionX(resetValue.x);
                            break;
                        }
                    case DOTweenType.DOMoveY:
                        {
                            (targetCom as Transform).SetPositionY(resetValue.x);
                            break;
                        }
                    case DOTweenType.DOMoveZ:
                        {
                            (targetCom as Transform).SetPositionZ(resetValue.x);
                            break;
                        }
                    case DOTweenType.DOLocalMove:
                        {
                            (targetCom as Transform).localPosition = resetValue;
                            break;
                        }
                    case DOTweenType.DOLocalMoveX:
                        {
                            (targetCom as Transform).SetLocalPositionX(resetValue.x);
                            break;
                        }
                    case DOTweenType.DOLocalMoveY:
                        {
                            (targetCom as Transform).SetLocalPositionY(resetValue.x);
                            break;
                        }
                    case DOTweenType.DOLocalMoveZ:
                        {
                            (targetCom as Transform).SetLocalPositionZ(resetValue.x);
                            break;
                        }
                    case DOTweenType.DOAnchorPos:
                        {
                            (targetCom as RectTransform).anchoredPosition = resetValue;
                            break;
                        }
                    case DOTweenType.DOAnchorPosX:
                        {
                            (targetCom as RectTransform).SetAnchoredPositionX(resetValue.x);
                            break;
                        }
                    case DOTweenType.DOAnchorPosY:
                        {
                            (targetCom as RectTransform).SetAnchoredPositionY(resetValue.x);
                            break;
                        }
                    case DOTweenType.DOAnchorPosZ:
                        {
                            (targetCom as RectTransform).SetAnchoredPosition3DZ(resetValue.x);
                            break;
                        }
                    case DOTweenType.DOAnchorPos3D:
                        {
                            (targetCom as RectTransform).anchoredPosition3D = resetValue;
                            break;
                        }
                    case DOTweenType.DOColor:
                        {
                            (targetCom as UnityEngine.UI.Graphic).color = resetValue;
                            break;
                        }
                    case DOTweenType.DOFade:
                        {
                            (targetCom as UnityEngine.UI.Graphic).SetColorAlpha(resetValue.x);
                            break;
                        }
                    case DOTweenType.DOCanvasGroupFade:
                        {
                            (targetCom as UnityEngine.CanvasGroup).alpha = resetValue.x;
                            break;
                        }
                    case DOTweenType.DOValue:
                        {
                            (targetCom as UnityEngine.UI.Slider).value = resetValue.x;
                            break;
                        }
                    case DOTweenType.DOSizeDelta:
                        {
                            (targetCom as RectTransform).sizeDelta = resetValue;
                            break;
                        }
                    case DOTweenType.DOFillAmount:
                        {
                            (targetCom as UnityEngine.UI.Image).fillAmount = resetValue.x;
                            break;
                        }
                    case DOTweenType.DOFlexibleSize:
                        {
                            (targetCom as LayoutElement).SetFlexibleSize(resetValue);
                            break;
                        }
                    case DOTweenType.DOMinSize:
                        {
                            (targetCom as LayoutElement).SetMinSize(resetValue);
                            break;
                        }
                    case DOTweenType.DOPreferredSize:
                        {
                            (targetCom as LayoutElement).SetPreferredSize(resetValue);
                            break;
                        }
                    case DOTweenType.DOScale:
                        {
                            (targetCom as Transform).localScale = resetValue;
                            break;
                        }
                    case DOTweenType.DOScaleX:
                        {
                            (targetCom as Transform).SetLocalScaleX(resetValue.x);
                            break;
                        }
                    case DOTweenType.DOScaleY:
                        {
                            (targetCom as Transform).SetLocalScaleY(resetValue.x);
                            break;
                        }
                    case DOTweenType.DOScaleZ:
                        {
                            (targetCom as Transform).SetLocalScaleZ(resetValue.z);
                            break;
                        }
                    case DOTweenType.DORotate:
                        {
                            (targetCom as Transform).eulerAngles = resetValue;
                            break;
                        }
                    case DOTweenType.DOLocalRotate:
                        {
                            (targetCom as Transform).localEulerAngles = resetValue;
                            break;
                        }
                }
            }
        }
        private MotionHandle CreateTween(bool reverse = false)
        {
            if (m_Sequence == null || m_Sequence.Length == 0)
            {
                return MotionHandle.None;
            }
            m_Rewind = reverse;
            var sequence = LitMotion.LSequence.Create().AppendInterval(m_Delay);
            if (reverse)
            {
                for (int i = m_Sequence.Length - 1; i >= 0; i--)
                {
                    var item = m_Sequence[i];
                    var tweener = item.CreateTween(m_Scheduler, reverse);
                    if (tweener == null)
                    {
                        Debug.LogErrorFormat("Tweener is null. Index:{0}, Animation Type:{1}, Component Type:{2}", i, item.AnimationType, item.Target == null ? "null" : item.Target.GetType().Name);
                        continue;
                    }
                    switch (item.AddType)
                    {
                        case AddType.Append:
                            sequence.Append(tweener);
                            break;
                        case AddType.Join:
                            sequence.Join(tweener);
                            break;
                    }
                }
            }
            else
            {
                for (int i = 0; i < m_Sequence.Length; i++)
                {
                    var item = m_Sequence[i];
                    var tweener = item.CreateTween(m_Scheduler, reverse);
                    if (tweener == null)
                    {
                        Debug.LogErrorFormat("Tweener is null. Index:{0}, Animation Type:{1}, Component Type:{2}", i, item.AnimationType, item.Target == null ? "null" : item.Target.GetType().Name);
                        continue;
                    }
                    switch (item.AddType)
                    {
                        case AddType.Append:
                            sequence.Append(tweener);
                            break;
                        case AddType.Join:
                            sequence.Join(tweener);
                            break;
                    }
                }
            }
            return sequence.Run().AddTo(this).Preserve();
        }

        public MotionHandle Play(bool rewind = false, UnityAction completeCallback = null)
        {
            Stop();
            OnComplete(completeCallback);
            m_Tween = CreateTween(rewind);
            return m_Tween;
        }

        public void Cancel(bool reset = false)
        {
            if (m_Tween == MotionHandle.None) return;
            if (reset)
            {
                if (!m_Rewind)
                {
                    var rewind = Play(true);
                    rewind.Complete();
                }
                else
                {
                    m_Tween.TryComplete();
                }
            }
            else
            {
                m_Tween.TryCancel();
            }
        }
        public void OnComplete(UnityAction callback)
        {
            if (callback == null) return;
            m_OnComplete ??= new UnityEvent();
            m_OnComplete.RemoveAllListeners();
            m_OnComplete.AddListener(callback);
        }
        private void Stop()
        {
            if (m_Tween == MotionHandle.None) return;

            m_Tween.Cancel();
            m_Tween = MotionHandle.None;
        }
        public enum DOTweenType
        {
            DOMove,
            DOMoveX,
            DOMoveY,
            DOMoveZ,

            DOLocalMove,
            DOLocalMoveX,
            DOLocalMoveY,
            DOLocalMoveZ,

            DOScale,
            DOScaleX,
            DOScaleY,
            DOScaleZ,

            DORotate,
            DOLocalRotate,

            DOAnchorPos,
            DOAnchorPosX,
            DOAnchorPosY,
            DOAnchorPosZ,
            DOAnchorPos3D,


            DOColor,
            DOFade,
            DOCanvasGroupFade,
            DOFillAmount,
            DOFlexibleSize,
            DOMinSize,
            DOPreferredSize,
            DOSizeDelta,
            DOValue
        }

        [Serializable]
        public class UIAnimationItem
        {
            public AddType AddType = AddType.Append;
            public DOTweenType AnimationType = DOTweenType.DOMove;
            public Component Target = null;
            public Vector4 ToValue = Vector4.zero;

            public bool UseToTarget = false;
            public Component ToTarget = null;

            public Vector4 FromValue = Vector4.zero;
            public bool SpeedBased = false;
            public float DurationOrSpeed = 1;
            public float Delay = 0;
            public bool CustomEase = false;
            public AnimationCurve EaseCurve;
            public Ease Ease = Ease.OutQuad;
            public int Loops = 1;
            public LoopType LoopType = LoopType.Restart;
            public UnityEvent OnComplete = null;

            public MotionHandle CreateTween(IMotionScheduler scheduler, bool reverse)
            {
                MotionHandle result = MotionHandle.None;
                float duration = this.DurationOrSpeed;

                switch (AnimationType)
                {
                    case DOTweenType.DOMove:
                        {
                            var transform = Target as Transform;
                            Vector3 targetValue = UseToTarget ? (ToTarget as Transform).position : ToValue;
                            Vector3 startValue = FromValue;
                            if (reverse)
                            {
                                (targetValue, startValue) = (startValue, targetValue);
                            }
                            transform.position = startValue;
                            if (SpeedBased)
                                duration = Vector3.Distance(targetValue, startValue) / this.DurationOrSpeed;
                            var motion = LMotion.Create(startValue, targetValue, duration);
                            if (CustomEase) motion.WithEase(EaseCurve);
                            else motion.WithEase(Ease);
                            motion.WithDelay(Delay);
                            motion.WithScheduler(scheduler);
                            motion.WithLoops(Loops, LoopType);
                            motion.WithCancelOnError(true);
                            if (OnComplete != null) motion.WithOnComplete(OnComplete.Invoke);
                            result = motion.BindToPosition(transform);
                        }
                        break;
                    case DOTweenType.DOMoveX:
                        {
                            var transform = Target as Transform;
                            var targetValue = UseToTarget ? (ToTarget as Transform).position.x : ToValue.x;
                            var startValue = FromValue.x;
                            if (reverse)
                            {
                                (targetValue, startValue) = (startValue, targetValue);
                            }
                            transform.SetPositionX(startValue);
                            if (SpeedBased)
                                duration = Mathf.Abs(targetValue - startValue) / this.DurationOrSpeed;
                            var motion = LMotion.Create(startValue, targetValue, duration);
                            if (CustomEase) motion.WithEase(EaseCurve);
                            else motion.WithEase(Ease);
                            motion.WithDelay(Delay);
                            motion.WithScheduler(scheduler);
                            motion.WithLoops(Loops, LoopType);
                            motion.WithCancelOnError(true);
                            if (OnComplete != null) motion.WithOnComplete(OnComplete.Invoke);
                            result = motion.BindToPositionX(transform);
                        }
                        break;
                    case DOTweenType.DOMoveY:
                        {
                            var transform = Target as Transform;
                            var targetValue = UseToTarget ? (ToTarget as Transform).position.y : ToValue.x;
                            var startValue = FromValue.x;
                            if (reverse)
                            {
                                (targetValue, startValue) = (startValue, targetValue);
                            }
                            transform.SetPositionY(startValue);
                            if (SpeedBased)
                                duration = Mathf.Abs(targetValue - startValue) / this.DurationOrSpeed;
                            var motion = LMotion.Create(startValue, targetValue, duration);
                            if (CustomEase) motion.WithEase(EaseCurve);
                            else motion.WithEase(Ease);
                            motion.WithDelay(Delay);
                            motion.WithScheduler(scheduler);
                            motion.WithLoops(Loops, LoopType);
                            motion.WithCancelOnError(true);
                            if (OnComplete != null) motion.WithOnComplete(OnComplete.Invoke);
                            result = motion.BindToPositionY(transform);
                        }
                        break;
                    case DOTweenType.DOMoveZ:
                        {
                            var transform = Target as Transform;
                            var targetValue = UseToTarget ? (ToTarget as Transform).position.z : ToValue.x;
                            var startValue = FromValue.x;
                            if (reverse)
                            {
                                (targetValue, startValue) = (startValue, targetValue);
                            }
                            transform.SetPositionZ(startValue);
                            if (SpeedBased)
                                duration = Mathf.Abs(targetValue - startValue) / this.DurationOrSpeed;
                            var motion = LMotion.Create(startValue, targetValue, duration);
                            if (CustomEase) motion.WithEase(EaseCurve);
                            else motion.WithEase(Ease);
                            motion.WithDelay(Delay);
                            motion.WithScheduler(scheduler);
                            motion.WithLoops(Loops, LoopType);
                            motion.WithCancelOnError(true);
                            if (OnComplete != null) motion.WithOnComplete(OnComplete.Invoke);
                            result = motion.BindToPositionZ(transform);
                        }
                        break;
                    case DOTweenType.DOLocalMove:
                        {
                            var transform = Target as Transform;
                            var targetValue = UseToTarget ? (ToTarget as Transform).localPosition : (Vector3)ToValue;
                            var startValue = (Vector3)FromValue;
                            if (reverse)
                            {
                                (targetValue, startValue) = (startValue, targetValue);
                            }
                            transform.localPosition = startValue;
                            if (SpeedBased)
                                duration = Vector3.Distance(targetValue, startValue) / this.DurationOrSpeed;
                            var motion = LMotion.Create(startValue, targetValue, duration);
                            if (CustomEase) motion.WithEase(EaseCurve);
                            else motion.WithEase(Ease);
                            motion.WithDelay(Delay);
                            motion.WithScheduler(scheduler);
                            motion.WithLoops(Loops, LoopType);
                            motion.WithCancelOnError(true);
                            if (OnComplete != null) motion.WithOnComplete(OnComplete.Invoke);
                            result = motion.BindToLocalPosition(transform);
                        }
                        break;
                    case DOTweenType.DOLocalMoveX:
                        {
                            var transform = Target as Transform;
                            var targetValue = UseToTarget ? (ToTarget as Transform).localPosition.x : ToValue.x;
                            var startValue = FromValue.x;
                            if (reverse)
                            {
                                (targetValue, startValue) = (startValue, targetValue);
                            }
                            transform.SetLocalPositionX(startValue);
                            if (SpeedBased)
                                duration = Mathf.Abs(targetValue - startValue) / this.DurationOrSpeed;
                            var motion = LMotion.Create(startValue, targetValue, duration);
                            if (CustomEase) motion.WithEase(EaseCurve);
                            else motion.WithEase(Ease);
                            motion.WithDelay(Delay);
                            motion.WithScheduler(scheduler);
                            motion.WithLoops(Loops, LoopType);
                            motion.WithCancelOnError(true);
                            if (OnComplete != null) motion.WithOnComplete(OnComplete.Invoke);
                            result = motion.BindToLocalPositionX(transform);
                        }
                        break;
                    case DOTweenType.DOLocalMoveY:
                        {
                            var transform = Target as Transform;
                            var targetValue = UseToTarget ? (ToTarget as Transform).localPosition.y : ToValue.x;
                            var startValue = FromValue.x;
                            if (reverse)
                            {
                                (targetValue, startValue) = (startValue, targetValue);
                            }
                            transform.SetLocalPositionY(startValue);
                            if (SpeedBased)
                                duration = Mathf.Abs(targetValue - startValue) / this.DurationOrSpeed;
                            var motion = LMotion.Create(startValue, targetValue, duration);
                            if (CustomEase) motion.WithEase(EaseCurve);
                            else motion.WithEase(Ease);
                            motion.WithDelay(Delay);
                            motion.WithScheduler(scheduler);
                            motion.WithLoops(Loops, LoopType);
                            motion.WithCancelOnError(true);
                            if (OnComplete != null) motion.WithOnComplete(OnComplete.Invoke);
                            result = motion.BindToLocalPositionY(transform);
                        }
                        break;
                    case DOTweenType.DOLocalMoveZ:
                        {
                            var transform = Target as Transform;
                            var targetValue = UseToTarget ? (ToTarget as Transform).localPosition.z : ToValue.x;
                            var startValue = FromValue.x;
                            if (reverse)
                            {
                                (targetValue, startValue) = (startValue, targetValue);
                            }
                            transform.SetLocalPositionZ(startValue);
                            if (SpeedBased)
                                duration = Mathf.Abs(targetValue - startValue) / this.DurationOrSpeed;
                            var motion = LMotion.Create(startValue, targetValue, duration);
                            if (CustomEase) motion.WithEase(EaseCurve);
                            else motion.WithEase(Ease);
                            motion.WithDelay(Delay);
                            motion.WithScheduler(scheduler);
                            motion.WithLoops(Loops, LoopType);
                            motion.WithCancelOnError(true);
                            if (OnComplete != null) motion.WithOnComplete(OnComplete.Invoke);
                            result = motion.BindToLocalPositionZ(transform);
                        }
                        break;
                    case DOTweenType.DOScale:
                        {
                            var com = Target as Transform;
                            var targetValue = UseToTarget ? (ToTarget as Transform).localScale : (Vector3)ToValue;
                            var startValue = (Vector3)FromValue;
                            if (reverse)
                            {
                                (targetValue, startValue) = (startValue, targetValue);
                            }
                            com.localScale = startValue;
                            if (SpeedBased) duration = Vector3.Distance(targetValue, startValue) / this.DurationOrSpeed;
                            var motion = LMotion.Create(startValue, targetValue, duration);
                            if (CustomEase) motion.WithEase(EaseCurve);
                            else motion.WithEase(Ease);
                            motion.WithDelay(Delay);
                            motion.WithScheduler(scheduler);
                            motion.WithLoops(Loops, LoopType);
                            motion.WithCancelOnError(true);
                            if (OnComplete != null) motion.WithOnComplete(OnComplete.Invoke);
                            result = motion.BindToLocalScale(com);
                        }
                        break;
                    case DOTweenType.DOScaleX:
                        {
                            var com = Target as Transform;
                            var targetValue = UseToTarget ? (ToTarget as Transform).localScale.x : ToValue.x;
                            var startValue = FromValue.x;
                            if (reverse)
                            {
                                (targetValue, startValue) = (startValue, targetValue);
                            }
                            com.SetLocalScaleX(startValue);
                            if (SpeedBased)
                                duration = Mathf.Abs(targetValue - startValue) / this.DurationOrSpeed;
                            var motion = LMotion.Create(startValue, targetValue, duration);
                            if (CustomEase) motion.WithEase(EaseCurve);
                            else motion.WithEase(Ease);
                            motion.WithDelay(Delay);
                            motion.WithScheduler(scheduler);
                            motion.WithLoops(Loops, LoopType);
                            motion.WithCancelOnError(true);
                            if (OnComplete != null) motion.WithOnComplete(OnComplete.Invoke);
                            result = motion.BindToLocalScaleX(com);
                        }
                        break;
                    case DOTweenType.DOScaleY:
                        {
                            var com = Target as Transform;
                            var targetValue = UseToTarget ? (ToTarget as Transform).localScale.y : ToValue.x;
                            var startValue = FromValue.x;
                            if (reverse)
                            {
                                (targetValue, startValue) = (startValue, targetValue);
                            }
                            com.SetLocalScaleY(startValue);
                            if (SpeedBased)
                                duration = Mathf.Abs(targetValue - startValue) / this.DurationOrSpeed;
                            var motion = LMotion.Create(startValue, targetValue, duration);
                            if (CustomEase) motion.WithEase(EaseCurve);
                            else motion.WithEase(Ease);
                            motion.WithDelay(Delay);
                            motion.WithScheduler(scheduler);
                            motion.WithLoops(Loops, LoopType);
                            motion.WithCancelOnError(true);
                            if (OnComplete != null) motion.WithOnComplete(OnComplete.Invoke);
                            result = motion.BindToLocalScaleY(com);
                        }
                        break;
                    case DOTweenType.DOScaleZ:
                        {
                            var com = Target as Transform;
                            var targetValue = UseToTarget ? (ToTarget as Transform).localScale.z : ToValue.x;
                            var startValue = FromValue.x;
                            if (reverse)
                            {
                                (targetValue, startValue) = (startValue, targetValue);
                            }
                            com.SetLocalScaleZ(startValue);
                            if (SpeedBased)
                                duration = Mathf.Abs(targetValue - startValue) / this.DurationOrSpeed;
                            var motion = LMotion.Create(startValue, targetValue, duration);
                            if (CustomEase) motion.WithEase(EaseCurve);
                            else motion.WithEase(Ease);
                            motion.WithDelay(Delay);
                            motion.WithScheduler(scheduler);
                            motion.WithLoops(Loops, LoopType);
                            motion.WithCancelOnError(true);
                            if (OnComplete != null) motion.WithOnComplete(OnComplete.Invoke);
                            result = motion.BindToLocalScaleZ(com);
                        }
                        break;
                    case DOTweenType.DORotate:
                        {
                            var com = Target as Transform;
                            var targetValue = UseToTarget ? (ToTarget as Transform).eulerAngles : (Vector3)ToValue;
                            var startValue = (Vector3)FromValue;
                            if (reverse)
                            {
                                (targetValue, startValue) = (startValue, targetValue);
                            }
                            com.eulerAngles = startValue;
                            if (SpeedBased)
                                duration = GetEulerAnglesAngle(targetValue, startValue) / this.DurationOrSpeed;
                            var motion = LMotion.Create(startValue, targetValue, duration);
                            if (CustomEase) motion.WithEase(EaseCurve);
                            else motion.WithEase(Ease);
                            motion.WithDelay(Delay);
                            motion.WithScheduler(scheduler);
                            motion.WithLoops(Loops, LoopType);
                            motion.WithCancelOnError(true);
                            if (OnComplete != null) motion.WithOnComplete(OnComplete.Invoke);
                            result = motion.BindToEulerAngles(com);
                        }
                        break;
                    case DOTweenType.DOLocalRotate:
                        {
                            var com = Target as Transform;
                            var targetValue = UseToTarget ? (ToTarget as Transform).localEulerAngles : (Vector3)ToValue;
                            var startValue = (Vector3)FromValue;
                            if (reverse)
                            {
                                (targetValue, startValue) = (startValue, targetValue);
                            }
                            com.localEulerAngles = startValue;
                            if (SpeedBased)
                                duration = GetEulerAnglesAngle(targetValue, startValue) / this.DurationOrSpeed;
                            var motion = LMotion.Create(startValue, targetValue, duration);
                            if (CustomEase) motion.WithEase(EaseCurve);
                            else motion.WithEase(Ease);
                            motion.WithDelay(Delay);
                            motion.WithScheduler(scheduler);
                            motion.WithLoops(Loops, LoopType);
                            motion.WithCancelOnError(true);
                            if (OnComplete != null) motion.WithOnComplete(OnComplete.Invoke);
                            result = motion.BindToLocalEulerAngles(com);
                        }
                        break;
                    case DOTweenType.DOAnchorPos:
                        {
                            var rectTransform = Target as RectTransform;
                            var targetValue = UseToTarget ? (ToTarget as RectTransform).anchoredPosition : (Vector2)ToValue;
                            var startValue = (Vector2)FromValue;
                            if (reverse)
                            {
                                (targetValue, startValue) = (startValue, targetValue);
                            }
                            rectTransform.anchoredPosition = startValue;
                            if (SpeedBased)
                                duration = Vector2.Distance(targetValue, startValue) / this.DurationOrSpeed;
                            var motion = LMotion.Create(startValue, targetValue, duration);
                            if (CustomEase) motion.WithEase(EaseCurve);
                            else motion.WithEase(Ease);
                            motion.WithDelay(Delay);
                            motion.WithScheduler(scheduler);
                            motion.WithLoops(Loops, LoopType);
                            motion.WithCancelOnError(true);
                            if (OnComplete != null) motion.WithOnComplete(OnComplete.Invoke);
                            result = motion.BindToAnchoredPosition(rectTransform);
                        }
                        break;
                    case DOTweenType.DOAnchorPosX:
                        {
                            var rectTransform = Target as RectTransform;
                            var targetValue = UseToTarget ? (ToTarget as RectTransform).anchoredPosition.x : ToValue.x;
                            var startValue = FromValue.x;
                            if (reverse)
                            {
                                (targetValue, startValue) = (startValue, targetValue);
                            }
                            rectTransform.SetAnchoredPositionX(startValue);
                            if (SpeedBased)
                                duration = Mathf.Abs(targetValue - startValue) / this.DurationOrSpeed;
                            var motion = LMotion.Create(startValue, targetValue, duration);
                            if (CustomEase) motion.WithEase(EaseCurve);
                            else motion.WithEase(Ease);
                            motion.WithDelay(Delay);
                            motion.WithScheduler(scheduler);
                            motion.WithLoops(Loops, LoopType);
                            motion.WithCancelOnError(true);
                            if (OnComplete != null) motion.WithOnComplete(OnComplete.Invoke);
                            result = motion.BindToAnchoredPositionX(rectTransform);
                        }
                        break;
                    case DOTweenType.DOAnchorPosY:
                        {
                            var rectTransform = Target as RectTransform;
                            var targetValue = UseToTarget ? (ToTarget as RectTransform).anchoredPosition.y : ToValue.x;
                            var startValue = FromValue.x;
                            if (reverse)
                            {
                                var swapValue = startValue;
                                startValue = targetValue;
                                targetValue = swapValue;
                            }
                            rectTransform.SetAnchoredPositionY(startValue);
                            if (SpeedBased)
                                duration = Mathf.Abs(targetValue - startValue) / this.DurationOrSpeed;
                            var motion = LMotion.Create(startValue, targetValue, duration);
                            if (CustomEase) motion.WithEase(EaseCurve);
                            else motion.WithEase(Ease);
                            motion.WithDelay(Delay);
                            motion.WithScheduler(scheduler);
                            motion.WithLoops(Loops, LoopType);
                            motion.WithCancelOnError(true);
                            if (OnComplete != null) motion.WithOnComplete(OnComplete.Invoke);
                            result = motion.BindToAnchoredPositionY(rectTransform);
                        }
                        break;
                    case DOTweenType.DOAnchorPosZ:
                        {
                            var rectTransform = Target as RectTransform;
                            var targetValue = UseToTarget ? (ToTarget as RectTransform).anchoredPosition3D.z : ToValue.x;
                            var startValue = FromValue.x;
                            if (reverse)
                            {
                                (targetValue, startValue) = (startValue, targetValue);
                            }
                            rectTransform.SetAnchoredPosition3DZ(startValue);
                            if (SpeedBased)
                                duration = Mathf.Abs(targetValue - startValue) / this.DurationOrSpeed;
                            var motion = LMotion.Create(startValue, targetValue, duration);
                            if (CustomEase) motion.WithEase(EaseCurve);
                            else motion.WithEase(Ease);
                            motion.WithDelay(Delay);
                            motion.WithScheduler(scheduler);
                            motion.WithLoops(Loops, LoopType);
                            motion.WithCancelOnError(true);
                            if (OnComplete != null) motion.WithOnComplete(OnComplete.Invoke);
                            result = motion.BindToAnchoredPosition3DZ(rectTransform);
                        }
                        break;
                    case DOTweenType.DOAnchorPos3D:
                        {
                            var rectTransform = Target as RectTransform;
                            var targetValue = UseToTarget ? (ToTarget as RectTransform).anchoredPosition3D : (Vector3)ToValue;
                            var startValue = (Vector3)FromValue;
                            if (reverse)
                            {
                                (targetValue, startValue) = (startValue, targetValue);
                            }
                            rectTransform.anchoredPosition3D = startValue;
                            if (SpeedBased)
                                duration = Vector3.Distance(targetValue, startValue) / this.DurationOrSpeed;
                            var motion = LMotion.Create(startValue, targetValue, duration);
                            if (CustomEase) motion.WithEase(EaseCurve);
                            else motion.WithEase(Ease);
                            motion.WithDelay(Delay);
                            motion.WithScheduler(scheduler);
                            motion.WithLoops(Loops, LoopType);
                            motion.WithCancelOnError(true);
                            if (OnComplete != null) motion.WithOnComplete(OnComplete.Invoke);
                            result = motion.BindToAnchoredPosition3D(rectTransform);
                        }
                        break;
                    case DOTweenType.DOSizeDelta:
                        {
                            var rectTransform = Target as RectTransform;
                            var targetValue = UseToTarget ? (ToTarget as RectTransform).sizeDelta : (Vector2)ToValue;
                            var startValue = (Vector2)FromValue;
                            if (reverse)
                            {
                                (targetValue, startValue) = (startValue, targetValue);
                            }
                            rectTransform.sizeDelta = startValue;
                            if (SpeedBased)
                                duration = Vector2.Distance(targetValue, startValue) / this.DurationOrSpeed;
                            var motion = LMotion.Create(startValue, targetValue, duration);
                            if (CustomEase) motion.WithEase(EaseCurve);
                            else motion.WithEase(Ease);
                            motion.WithDelay(Delay);
                            motion.WithScheduler(scheduler);
                            motion.WithLoops(Loops, LoopType);
                            motion.WithCancelOnError(true);
                            if (OnComplete != null) motion.WithOnComplete(OnComplete.Invoke);
                            result = motion.BindToSizeDelta(rectTransform);
                        }
                        break;
                    case DOTweenType.DOColor:
                        {
                            var com = Target as UnityEngine.UI.Graphic;
                            var targetValue = UseToTarget ? (ToTarget as UnityEngine.UI.Graphic).color : (Color)ToValue;
                            var startValue = (Color)FromValue;
                            if (reverse)
                            {
                                (targetValue, startValue) = (startValue, targetValue);
                            }
                            com.color = startValue;
                            if (SpeedBased)
                                duration = Vector4.Distance(targetValue, startValue) / this.DurationOrSpeed;
                            var motion = LMotion.Create(startValue, targetValue, duration);
                            if (CustomEase) motion.WithEase(EaseCurve);
                            else motion.WithEase(Ease);
                            motion.WithDelay(Delay);
                            motion.WithScheduler(scheduler);
                            motion.WithLoops(Loops, LoopType);
                            motion.WithCancelOnError(true);
                            if (OnComplete != null) motion.WithOnComplete(OnComplete.Invoke);
                            result = motion.BindToColor(com);
                        }
                        break;
                    case DOTweenType.DOFade:
                        {
                            var com = Target as UnityEngine.UI.Graphic;
                            var targetValue = UseToTarget ? (ToTarget as UnityEngine.UI.Graphic).color.a : ToValue.x;
                            var startValue = FromValue.x;
                            if (reverse)
                            {
                                (targetValue, startValue) = (startValue, targetValue);
                            }
                            com.SetColorAlpha(startValue);
                            if (SpeedBased)
                                duration = Mathf.Abs(targetValue - startValue) / this.DurationOrSpeed;
                            var motion = LMotion.Create(startValue, targetValue, duration);
                            if (CustomEase) motion.WithEase(EaseCurve);
                            else motion.WithEase(Ease);
                            motion.WithDelay(Delay);
                            motion.WithScheduler(scheduler);
                            motion.WithLoops(Loops, LoopType);
                            motion.WithCancelOnError(true);
                            if (OnComplete != null) motion.WithOnComplete(OnComplete.Invoke);
                            result = motion.BindToColorA(com);
                        }
                        break;
                    case DOTweenType.DOCanvasGroupFade:
                        {
                            var com = Target as UnityEngine.CanvasGroup;
                            var targetValue = UseToTarget ? (ToTarget as UnityEngine.CanvasGroup).alpha : ToValue.x;
                            var startValue = FromValue.x;
                            if (reverse)
                            {
                                (targetValue, startValue) = (startValue, targetValue);
                            }
                            com.alpha = startValue;
                            if (SpeedBased)
                                duration = Mathf.Abs(targetValue - startValue) / this.DurationOrSpeed;
                            var motion = LMotion.Create(startValue, targetValue, duration);
                            if (CustomEase) motion.WithEase(EaseCurve);
                            else motion.WithEase(Ease);
                            motion.WithDelay(Delay);
                            motion.WithScheduler(scheduler);
                            motion.WithLoops(Loops, LoopType);
                            motion.WithCancelOnError(true);
                            if (OnComplete != null) motion.WithOnComplete(OnComplete.Invoke);
                            result = motion.BindToAlpha(com);
                        }
                        break;
                    case DOTweenType.DOValue:
                        {
                            var com = Target as UnityEngine.UI.Slider;
                            var targetValue = UseToTarget ? (ToTarget as UnityEngine.UI.Slider).value : ToValue.x;
                            var startValue = FromValue.x;
                            if (reverse)
                            {
                                (targetValue, startValue) = (startValue, targetValue);
                            }
                            com.value = startValue;
                            if (SpeedBased)
                                duration = Mathf.Abs(targetValue - startValue) / this.DurationOrSpeed;
                            var motion = LMotion.Create(startValue, targetValue, duration);
                            if (CustomEase) motion.WithEase(EaseCurve);
                            else motion.WithEase(Ease);
                            motion.WithDelay(Delay);
                            motion.WithScheduler(scheduler);
                            motion.WithLoops(Loops, LoopType);
                            motion.WithCancelOnError(true);
                            if (OnComplete != null) motion.WithOnComplete(OnComplete.Invoke);
                            result = motion.Bind(progress => com.value = progress);
                        }
                        break;

                    case DOTweenType.DOFillAmount:
                        {
                            var com = Target as UnityEngine.UI.Image;
                            var targetValue = UseToTarget ? (ToTarget as UnityEngine.UI.Image).fillAmount : ToValue.x;
                            var startValue = FromValue.x;
                            if (reverse)
                            {
                                (targetValue, startValue) = (startValue, targetValue);
                            }
                            com.fillAmount = startValue;
                            if (SpeedBased)
                                duration = Mathf.Abs(targetValue - startValue) / this.DurationOrSpeed;
                            var motion = LMotion.Create(startValue, targetValue, duration);
                            if (CustomEase) motion.WithEase(EaseCurve);
                            else motion.WithEase(Ease);
                            motion.WithDelay(Delay);
                            motion.WithScheduler(scheduler);
                            motion.WithLoops(Loops, LoopType);
                            motion.WithCancelOnError(true);
                            if (OnComplete != null) motion.WithOnComplete(OnComplete.Invoke);
                            result = motion.BindToFillAmount(com);
                        }
                        break;
                    case DOTweenType.DOFlexibleSize:
                        {
                            var com = Target as LayoutElement;
                            var targetValue = UseToTarget ? (ToTarget as LayoutElement).GetFlexibleSize() : (Vector2)ToValue;
                            var startValue = (Vector2)FromValue;
                            if (reverse)
                            {
                                (targetValue, startValue) = (startValue, targetValue);
                            }
                            com.SetFlexibleSize(startValue);
                            if (SpeedBased)
                                duration = Vector2.Distance(targetValue, startValue) / this.DurationOrSpeed;
                            var motion = LMotion.Create(startValue, targetValue, duration);
                            if (CustomEase) motion.WithEase(EaseCurve);
                            else motion.WithEase(Ease);
                            motion.WithDelay(Delay);
                            motion.WithScheduler(scheduler);
                            motion.WithLoops(Loops, LoopType);
                            motion.WithCancelOnError(true);
                            if (OnComplete != null) motion.WithOnComplete(OnComplete.Invoke);
                            result = motion.Bind(v => com.SetFlexibleSize(v));
                        }
                        break;
                    case DOTweenType.DOMinSize:
                        {
                            var com = Target as LayoutElement;
                            var targetValue = UseToTarget ? (ToTarget as LayoutElement).GetMinSize() : (Vector2)ToValue;
                            var startValue = (Vector2)FromValue;
                            if (reverse)
                            {
                                (targetValue, startValue) = (startValue, targetValue);
                            }
                            com.SetMinSize(startValue);
                            if (SpeedBased)
                                duration = Vector2.Distance(targetValue, startValue) / this.DurationOrSpeed;
                            var motion = LMotion.Create(startValue, targetValue, duration);
                            if (CustomEase) motion.WithEase(EaseCurve);
                            else motion.WithEase(Ease);
                            motion.WithDelay(Delay);
                            motion.WithScheduler(scheduler);
                            motion.WithLoops(Loops, LoopType);
                            motion.WithCancelOnError(true);
                            if (OnComplete != null) motion.WithOnComplete(OnComplete.Invoke);
                            result = motion.Bind(v => com.SetMinSize(v));
                        }
                        break;
                    case DOTweenType.DOPreferredSize:
                        {
                            var com = Target as LayoutElement;
                            var targetValue = UseToTarget ? (ToTarget as LayoutElement).GetPreferredSize() : (Vector2)ToValue;
                            var startValue = (Vector2)FromValue;
                            if (reverse)
                            {
                                (targetValue, startValue) = (startValue, targetValue);
                            }
                            com.SetPreferredSize(startValue);
                            if (SpeedBased)
                                duration = Vector2.Distance(targetValue, startValue) / this.DurationOrSpeed;
                            var motion = LMotion.Create(startValue, targetValue, duration);
                            if (CustomEase) motion.WithEase(EaseCurve);
                            else motion.WithEase(Ease);
                            motion.WithDelay(Delay);
                            motion.WithScheduler(scheduler);
                            motion.WithLoops(Loops, LoopType);
                            motion.WithCancelOnError(true);
                            if (OnComplete != null) motion.WithOnComplete(OnComplete.Invoke);
                            result = motion.Bind(v => com.SetPreferredSize(v));
                        }
                        break;
                }
                result.AddTo(Target);
                return result;
            }
            public static float GetEulerAnglesAngle(Vector3 euler1, Vector3 euler2)
            {
                // 计算差值
                Vector3 delta = euler2 - euler1;
                delta.x = Mathf.DeltaAngle(euler1.x, euler2.x);
                delta.y = Mathf.DeltaAngle(euler1.y, euler2.y);
                delta.z = Mathf.DeltaAngle(euler1.z, euler2.z);

                float angle = Mathf.Sqrt(delta.x * delta.x + delta.y * delta.y + delta.z * delta.z);
                return (angle + 360) % 360;
            }

            public void Reset()
            {
                throw new NotImplementedException();
            }
        }
        public enum AddType
        {
            Append,
            Join
        }
    }
}