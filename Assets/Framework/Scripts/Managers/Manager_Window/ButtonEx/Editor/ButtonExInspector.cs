using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UI;
using UnityEditorInternal;
using UnityEditor.UI;

namespace Game
{
    [CustomEditor(typeof(ButtonEx))]
    public class ButtonExInspector : Editor//GraphicEditor
    {
        private ReorderableList _roList_Items;
        private ReorderableList _roList_GameObjects;
        private static ButtonEx.ButtonStateType _btnStateType = ButtonEx.ButtonStateType.Normal;
        private static ButtonEx.ItemStateType _itemStateType = ButtonEx.ItemStateType.Normal;

        private static readonly int MaxBtnStateCount = System.Enum.GetValues(typeof(ButtonEx.ButtonStateType)).Length;
        private static readonly int MaxItemStateCount = System.Enum.GetValues(typeof(ButtonEx.ItemStateType)).Length;
        
        //protected override void OnEnable()
        protected void OnEnable()
        {
            //base.OnEnable();

            ButtonEx myTarget = (ButtonEx)target;

            // 사운드 인포 리스트 구성
            if (myTarget.stateSoundClip == null)
                myTarget.stateSoundClip = new AudioClip[MaxItemStateCount];
            else if (myTarget.stateSoundClip.Length < MaxItemStateCount)
                System.Array.Resize(ref myTarget.stateSoundClip, MaxItemStateCount);


            // 애니메이션 인포 리스트 구성
            if (myTarget.stateAnimClip == null)
                myTarget.stateAnimClip = new AnimationClip[MaxItemStateCount];
            else if (myTarget.stateAnimClip.Length < MaxItemStateCount)
                System.Array.Resize(ref myTarget.stateAnimClip, MaxItemStateCount);

            // 애니메이션 인포 리스트 구성

            // 아이템 인포 리스트 구성
            if (myTarget.goInfos == null)
                myTarget.goInfos = new System.Collections.Generic.List<ButtonEx.GameObjectInfo>();
            if (myTarget.itemInfos == null)
                myTarget.itemInfos = new System.Collections.Generic.List<ButtonEx.ItemInfo>();

            _roList_GameObjects = new ReorderableList(myTarget.goInfos, typeof(ButtonEx.GameObjectInfo), true, true, true, true );
            _roList_GameObjects.drawHeaderCallback = (rt) => EditorGUI.LabelField(rt, "GameObjects");
            _roList_GameObjects.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                var goInfo = _roList_GameObjects.list[index] as ButtonEx.GameObjectInfo;

                // 아직 세팅이 안됬다면 세팅한다.
                if (goInfo.enableInfo == null)
                    goInfo.enableInfo = new ButtonEx.GameObjectStateInfo[MaxBtnStateCount * MaxItemStateCount];
                else if (goInfo.enableInfo.Length != MaxBtnStateCount)
                    System.Array.Resize(ref goInfo.enableInfo, MaxBtnStateCount * MaxItemStateCount);

                for (int i = 0; i < goInfo.enableInfo.Length; ++i)
                {
                    if (goInfo.enableInfo[i] == null)
                        goInfo.enableInfo[i] = new ButtonEx.GameObjectStateInfo();
                }

                var enableInfo = goInfo.enableInfo[(int)_btnStateType * MaxItemStateCount + (int)_itemStateType];

                rect.y += 2;

                float width_targetField = rect.width * 0.5f - EditorGUIUtility.singleLineHeight * 2;
                float width_Activity = EditorGUIUtility.singleLineHeight;
                float width_Scale = rect.width * 0.5f;

                var rt_TargetField = new Rect(rect.x, rect.y, width_targetField, EditorGUIUtility.singleLineHeight);
                goInfo.Target = EditorGUI.ObjectField(rt_TargetField, goInfo.Target, typeof(GameObject), true) as GameObject;

                var rt_Activity = new Rect(rt_TargetField.xMax, rect.y, width_Activity, EditorGUIUtility.singleLineHeight);
                enableInfo.Enable = EditorGUI.Toggle(rt_Activity, enableInfo.Enable);

                var rt_ScaleField = new Rect(rt_Activity.xMax, rect.y, width_Scale, EditorGUIUtility.singleLineHeight);
                enableInfo.Scale = EditorGUI.Vector3Field(rt_ScaleField, string.Empty, enableInfo.Scale);
            };


            _roList_Items = new ReorderableList( myTarget.itemInfos, typeof(ButtonEx.ItemInfo), true, true, true, true);
            _roList_Items.drawHeaderCallback = (rt) => EditorGUI.LabelField(rt, "Graphics");
            _roList_Items.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                // 드로우 할 아이템 인포
                var ItemInfo = _roList_Items.list[index] as ButtonEx.ItemInfo;

                // 아직 세팅이 안됬다면 세팅한다.
                // 아직 세팅이 안됬다면 세팅한다.
                if (ItemInfo.stateInfo == null)
                    ItemInfo.stateInfo = new ButtonEx.ItemStateInfo[MaxBtnStateCount * MaxItemStateCount];
                else if (ItemInfo.stateInfo.Length != MaxBtnStateCount)
                    System.Array.Resize(ref ItemInfo.stateInfo, MaxBtnStateCount * MaxItemStateCount);

                for (int i = 0; i < ItemInfo.stateInfo.Length; ++i)
                {
                    if (ItemInfo.stateInfo[i] == null)
                        ItemInfo.stateInfo[i] = new ButtonEx.ItemStateInfo();
                }

                var ItemStateInfo = ItemInfo.stateInfo[(int)_btnStateType * MaxItemStateCount + (int)_itemStateType];

                rect.y += 2;

                float width_targetField = rect.width / 3f - EditorGUIUtility.singleLineHeight;
                float width_Activity = EditorGUIUtility.singleLineHeight;
                float width_Color = rect.width / 3f;
                float width_Scale = rect.width / 3f;

                var rt_TargetField = new Rect(rect.x, rect.y, width_targetField, EditorGUIUtility.singleLineHeight);
                ItemInfo.Target = EditorGUI.ObjectField(rt_TargetField, ItemInfo.Target, typeof(Graphic), true) as Graphic;

                var rt_Activity = new Rect(rt_TargetField.xMax, rect.y, width_Activity, EditorGUIUtility.singleLineHeight);
                ItemStateInfo.Enable = EditorGUI.Toggle(rt_Activity, ItemStateInfo.Enable);

                var rt_Color = new Rect(rt_Activity.xMax, rect.y, width_Color, EditorGUIUtility.singleLineHeight);
                ItemStateInfo.Color = EditorGUI.ColorField(rt_Color, ItemStateInfo.Color);

                var rt_Scale = new Rect(rt_Color.xMax, rect.y, width_Scale, EditorGUIUtility.singleLineHeight);
                ItemStateInfo.Scale = EditorGUI.Vector3Field(rt_Scale, string.Empty, ItemStateInfo.Scale);
            };
        }
        public override void OnInspectorGUI()
        {
            base.serializedObject.Update();
            //EditorGUILayout.PropertyField(base.m_Script, new GUILayoutOption[0]);
            EditorGUILayout.BeginVertical("box");
            {
                EditorGUILayout.LabelField("Button Base");
                EditorGUILayout.PropertyField(this.serializedObject.FindProperty("_anim"), true);
                EditorGUILayout.PropertyField(this.serializedObject.FindProperty("_uiTxt_Buttons"), true);
            }
            EditorGUILayout.EndVertical();

            ButtonEx myTarget = (ButtonEx)target;

            if (Application.isPlaying)
            {
                EditorGUILayout.LabelField("CurrState", myTarget.ButtonState.ToString());
            }


            // 편집할 스테이트 선택
            EditorGUILayout.BeginVertical("Box");
            {
                EditorGUILayout.LabelField("Button State");

                EditorGUILayout.BeginHorizontal();
                {
                    var defaultBgColor = GUI.backgroundColor;
                    for (int i = 0; i < MaxBtnStateCount; ++i)
                    {
                        var btnStateType = (ButtonEx.ButtonStateType)i;

                        if (btnStateType == _btnStateType)
                            GUI.backgroundColor = Color.green;
                        else
                            GUI.backgroundColor = defaultBgColor;

                        if (GUILayout.Button(btnStateType.ToString()))
                            _btnStateType = btnStateType;
                    }

                    GUI.backgroundColor = defaultBgColor;
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                {
                    var defaultBgColor = GUI.backgroundColor;
                    for (int i = 0; i < MaxItemStateCount; ++i)
                    {
                        var itemStateType = (ButtonEx.ItemStateType)i;

                        if (itemStateType == _itemStateType)
                            GUI.backgroundColor = Color.green;
                        else
                            GUI.backgroundColor = defaultBgColor;

                        if (GUILayout.Button(itemStateType.ToString()))
                            _itemStateType = itemStateType;
                    }

                    GUI.backgroundColor = defaultBgColor;
                }
                EditorGUILayout.EndHorizontal();

                myTarget.stateAnimClip[(int)_itemStateType] = EditorGUILayout.ObjectField("StateAnim", myTarget.stateAnimClip[(int)_itemStateType], typeof(AnimationClip), false) as AnimationClip;
                // 사운드 클립 에디트
                myTarget.stateSoundClip[(int)_itemStateType] = EditorGUILayout.ObjectField("StateSound", myTarget.stateSoundClip[(int)_itemStateType], typeof(AudioClip), false) as AudioClip;

                // 아이템 리스트 에디트
                _roList_GameObjects?.DoLayoutList();
                _roList_Items?.DoLayoutList();
            }
            EditorGUILayout.EndVertical();

            // 클릭 콜백 에디트
            EditorGUILayout.BeginVertical("box");
            {
                var prop_UseOnPress = serializedObject.FindProperty("bUseOnClick");
                EditorGUILayout.PropertyField(prop_UseOnPress);
                if (myTarget.bUseOnClick)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("_audioClip_OnClick"));
                    var prop_OnClick = serializedObject.FindProperty("onClick");
                    EditorGUILayout.PropertyField(prop_OnClick);
                }
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("box");
            {
                var prop_UseOnPress = serializedObject.FindProperty("bUseOnHold");
                EditorGUILayout.PropertyField(prop_UseOnPress);
                if (myTarget.bUseOnHold)
                {
                    myTarget.bUseOnPress = false;

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("_audioClip_OnHold"));
                    EditorGUILayout.BeginVertical("box");
                    {
                        EditorGUILayout.PropertyField(this.serializedObject.FindProperty("holdTime"), true);
                        EditorGUILayout.BeginHorizontal();
                        {
                            EditorGUILayout.PropertyField(this.serializedObject.FindProperty("holdEffectTime"), true);
                            EditorGUILayout.PropertyField(this.serializedObject.FindProperty("prf_Effect"), true);
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUILayout.EndVertical();

                    var prop_OnHold = serializedObject.FindProperty("onHold");
                    EditorGUILayout.PropertyField(prop_OnHold);
                }
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("box");
            {
                var prop_UseOnPress = serializedObject.FindProperty("bUseOnPress");
                EditorGUILayout.PropertyField(prop_UseOnPress);
                if (myTarget.bUseOnPress)
                {
                    myTarget.bUseOnHold = false;
            
                    //EditorGUILayout.PropertyField(serializedObject.FindProperty("_audioClip_OnPress"));
            
                    var prop_OnPressStart = serializedObject.FindProperty("onPressStart");
                    var prop_OnPressed = serializedObject.FindProperty("onPressed");
                    var prop_OnPressEnd = serializedObject.FindProperty("onPressEnd");
                    EditorGUILayout.PropertyField(prop_OnPressStart);
                    EditorGUILayout.PropertyField(prop_OnPressed);
                    EditorGUILayout.PropertyField(prop_OnPressEnd);
                }
            }
            EditorGUILayout.EndVertical();


            myTarget.goInfos = _roList_GameObjects.list as List<ButtonEx.GameObjectInfo>;
            myTarget.itemInfos = _roList_Items.list as List<ButtonEx.ItemInfo>;

            EditorUtility.SetDirty(target);
            serializedObject.ApplyModifiedProperties();

        }
    }
}