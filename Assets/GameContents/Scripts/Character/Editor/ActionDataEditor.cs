using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Linq;
using System;
using System.IO;
using System.Text;
using System.Runtime.CompilerServices;

namespace Game.Character.Action
{
    public partial class ActionDataEditor : EditorWindow
    {
        public enum EditorModeType
        { 
            Event,
            Trigger,
            Condition,
            NextAction
        }

        public enum NextActionEditorModeType
        { 
            List,
            Node,
        }

        public const string path_ActionData = "Assets/GameContents/Resources/Data/ActionDatas";

        private SortedDictionary<string, ActionData> _dic_ActionDatas;
        private ReorderableList _roList_ActionData;
        private ActionData _selectedActionData;
        private string _txt_Search;

        private HashSet<string> _set_SelectedActionNames = new HashSet<string>();

        public Rect _windowBounds = Rect.zero;
        public Rect _windowRect = new Rect(int.MaxValue, int.MaxValue, 400, 200);
        private Vector2 _vScrollAction;
        private Vector2 _vScrollGUI;

        private bool _bFold_GUIDetail = false;

        private EditorModeType[] _editorModeTypes;
        private EditorModeType _editorMode = EditorModeType.Event;
        
        private NextActionEditorModeType[] _nextActionEditorModeTypes;
        private NextActionEditorModeType _nextActionEditorMode = NextActionEditorModeType.List;


        [MenuItem("BFGame/Open ActionData Editor", false, 1)]
        public static void ShowWindow()
        {
            var window = EditorWindow.GetWindow(typeof(ActionDataEditor), false, "ActionDataEditor");
            window.minSize = new Vector2(1280, 720);
            window.Show();

        }

        private void OnEnable()
        {
            LoadAll_ActionDatas();

            if (string.IsNullOrWhiteSpace(_txt_Search))
                Refresh_ActionDataList(null);
            else
            {
                var lowerFilter = _txt_Search.ToLower();
                Refresh_ActionDataList((x) =>
                {
                    var result = false;
                    result |= x.Name?.ToLower().Contains(lowerFilter) ?? false;
                    result |= x.Desc?.ToLower().Contains(lowerFilter) ?? false;
                    return result;
                });
            }

            _editorModeTypes = System.Enum.GetValues(typeof(EditorModeType)) as EditorModeType[];
            _nextActionEditorModeTypes = System.Enum.GetValues(typeof(NextActionEditorModeType)) as NextActionEditorModeType[];

            OnEnable_NextActionNode();
        }

        void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            {
                OnGUI_ActionDataList();
                OnGUI_ActionDataInfo();
            }
            EditorGUILayout.EndHorizontal();
        }

        void OnGUI_ActionDataList()
        {
            EditorGUILayout.BeginVertical("Box", GUILayout.Width(300f));
            {
                EditorGUILayout.BeginHorizontal();
                {
                    _txt_Search = EditorGUILayout.TextField(_txt_Search);
                    if (GUILayout.Button("Search"))
                    {
                        if (string.IsNullOrWhiteSpace(_txt_Search))
                            Refresh_ActionDataList(null);
                        else
                        {
                            var lowerFilter = _txt_Search.ToLower();
                            Refresh_ActionDataList((x) =>
                            {
                                var result = false;
                                result |= x.Name?.ToLower().Contains(lowerFilter) ?? false;
                                result |= x.Desc?.ToLower().Contains(lowerFilter) ?? false;
                                return result;
                            });
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.BeginVertical();
                    {
                        EditorGUI.BeginDisabledGroup(_set_SelectedActionNames.Count <= 0);
                        {
                            if (GUILayout.Button("Save All"))
                            {
                                int count = 0;
                                bool isSaved = false;

                                try
                                {
                                    foreach (var actionName in _set_SelectedActionNames)
                                    {
                                        EditorUtility.DisplayProgressBar("Save All ActionData", actionName, ++count / (float)_set_SelectedActionNames.Count);
                                        isSaved |= Save_ActionData(_dic_ActionDatas.GetOrNull(actionName), false);
                                    }
                                }
                                catch (System.Exception e)
                                {
                                    EditorUtility.ClearProgressBar();
                                    throw e;
                                }

                                EditorUtility.ClearProgressBar();
                                _set_SelectedActionNames.Clear();

                                if (isSaved)
                                    AssetDatabase.SaveAssets();
                            }
                        }
                        EditorGUI.EndDisabledGroup();

                        if (GUILayout.Button("Find EventType"))
                        {
                            var menu = new GenericMenu();

                            GenericMenu.MenuFunction2 menuFunction = (userData) =>
                            {
                                var type = userData as Type;

                                Refresh_ActionDataList((x) =>
                                {
                                    for (int i = 0; i < x.eventData.list_ActionEvents.Count; ++i)
                                    {
                                        var actionEvent = x.eventData.list_ActionEvents[i];
                                        if (actionEvent.GetType().Name == type.Name)
                                            return true;
                                    }

                                    return false;
                                });
                            };

                            for (int i = 0; i < ActionEventData.List_EventTypes.Count; ++i)
                                menu.AddItem(new GUIContent(ActionEventData.List_EventTypeNames[i]), false, menuFunction, ActionEventData.List_EventTypes[i]);

                            menu.ShowAsContext();
                        }
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.BeginVertical();
                    {
                        if (GUILayout.Button("Create"))
                        {
                            Create_ActionData(_txt_Search, null);
                            Refresh_ActionDataList(null);
                        }

                        EditorGUI.BeginDisabledGroup(_selectedActionData == null);
                        {
                            if (GUILayout.Button("Create(Copy)"))
                            {
                                Create_ActionData(_txt_Search, _selectedActionData);
                                Refresh_ActionDataList(null);
                            }
                        }
                        EditorGUI.EndDisabledGroup();
                    }
                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.EndHorizontal();


                _vScrollAction = EditorGUILayout.BeginScrollView(_vScrollAction);
                {
                    _roList_ActionData?.DoLayoutList();
                }
                EditorGUILayout.EndScrollView();

                OnGUI_ActionDetailInfo();
                //EditorWindowEx.AutoFlexibleSpace();
            }
            EditorGUILayout.EndVertical();
        }

        void OnGUI_ActionDataInfo()
        {
            EditorGUILayout.BeginVertical();
            {
                EditorGUILayout.BeginHorizontal();
                {
                    var defalutGUIColor = GUI.backgroundColor;

                    for (int i = 0; i < _editorModeTypes.Length; ++i)
                    {
                        var modeType = _editorModeTypes[i];

                        GUI.backgroundColor = _editorMode == modeType ? Color.green : defalutGUIColor;
                        if (GUILayout.Button(modeType.ToString()))
                        {
                            _editorMode = modeType;
                            _vScrollGUI = Vector2.zero;
                        }

                        GUI.backgroundColor = defalutGUIColor;
                    }
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginVertical("Box");
                {
                    if (_selectedActionData == null)
                    {
                        AutoFlexibleSpace();
                    }
                    else
                    {
                        switch (_editorMode)
                        {
                            case EditorModeType.Event:
                                {
                                    _vScrollGUI = EditorGUILayout.BeginScrollView(_vScrollGUI);
                                    _selectedActionData.eventData.OnGUI_List(_selectedActionData);
                                    EditorGUILayout.EndScrollView();
                                }
                                break;
                                case EditorModeType.Trigger:
                                {
                                    _vScrollGUI = EditorGUILayout.BeginScrollView(_vScrollGUI);
                                    _selectedActionData.triggerData.OnGUI();
                                    EditorGUILayout.EndScrollView();
                                }
                                break;
                            case EditorModeType.Condition:
                                {
                                    _vScrollGUI = EditorGUILayout.BeginScrollView(_vScrollGUI);
                                    _selectedActionData.conditionData.OnGUI_List(_selectedActionData);
                                    EditorGUILayout.EndScrollView();
                                }
                                break;
                            case EditorModeType.NextAction:
                                {
                                    EditorGUILayout.BeginHorizontal();
                                    {
                                        var defalutGUIColor = GUI.backgroundColor;

                                        for (int i = 0; i < _nextActionEditorModeTypes.Length; ++i)
                                        {
                                            var nextActionEditorModeType = _nextActionEditorModeTypes[i];

                                            GUI.backgroundColor = _nextActionEditorMode == nextActionEditorModeType ? Color.green : defalutGUIColor;
                                            if (GUILayout.Button(nextActionEditorModeType.ToString()))
                                            {
                                                _nextActionEditorMode = nextActionEditorModeType;
                                                _vScrollGUI = Vector2.zero;

                                                if (_nextActionEditorMode == NextActionEditorModeType.Node)
                                                    RefreshActionSet(true);
                                            }

                                            GUI.backgroundColor = defalutGUIColor;
                                        }
                                    }
                                    EditorGUILayout.EndHorizontal();

                                    switch (_nextActionEditorMode)
                                    {
                                        case NextActionEditorModeType.List:
                                            {
                                                _vScrollGUI = EditorGUILayout.BeginScrollView(_vScrollGUI);
                                                _selectedActionData.nextData.OnGUI_List(_selectedActionData);
                                                EditorGUILayout.EndScrollView();
                                            }
                                            break;
                                        case NextActionEditorModeType.Node:
                                            {
                                                OnGUI_NextActionNode();
                                            }
                                            break;
                                    }
                                }
                                break;
                            default:
                                {
                                    AutoFlexibleSpace();
                                }
                                break;
                        }
                    }
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndVertical();
        }

        void OnGUI_ActionDetailInfo()
        {
            if (_selectedActionData == null)
                return;

            var newRect = EditorGUILayout.BeginVertical();
            {
                _selectedActionData.OnGUI_Detail();

                if (!_bFold_GUIDetail)
                {
                    switch (_editorMode)
                    {
                        case EditorModeType.Event:
                            _selectedActionData.eventData.OnGUI_Detail(_selectedActionData);
                            break;
                        case EditorModeType.Trigger:
                            _selectedActionData.triggerData.OnGUI_Detail(_selectedActionData);
                            break;
                        case EditorModeType.Condition:
                            _selectedActionData.conditionData.OnGUI_Detail(_selectedActionData);
                            break;
                        case EditorModeType.NextAction:
                            if (_nextActionEditorMode == NextActionEditorModeType.List)
                                _selectedActionData.nextData.OnGUI_Detail(_selectedActionData);
                            break;
                    }
                }

                EditorGUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button(_bFold_GUIDetail ? "▲" : "▼", GUILayout.Width(20)))
                    {
                        _bFold_GUIDetail = !_bFold_GUIDetail;
                    }

                    if (GUILayout.Button("Save"))
                    {
                        Save_ActionData(_selectedActionData);
                    }
                    if (GUILayout.Button("Delete"))
                    {
                        Delete_ActionData(_selectedActionData);
                        Refresh_ActionDataList(null);
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();

            //if (Event.current.mousePosition.x < _windowRect.width
            //    && Event.current.mousePosition.y < _windowRect.height )
            //    Event.current.Use();
        }

        void Refresh_ActionDataList(System.Func<ActionData, bool> filter)
        {
            if (_dic_ActionDatas == null)
                return;

            var list_ActionData = _dic_ActionDatas.Values.ToList();
            if (filter != null)
                list_ActionData = list_ActionData.Where(filter).ToList();

            _roList_ActionData = new ReorderableList(list_ActionData, typeof(ActionData), false, false, false, false);
            _roList_ActionData.headerHeight = 0f;
            _roList_ActionData.onSelectCallback = (rolist) =>
            {
                _selectedActionData = rolist.list[rolist.index] as ActionData;
                if (!_set_SelectedActionNames.Contains(_selectedActionData.Name))
                    _set_SelectedActionNames.Add(_selectedActionData.Name);

                if (_editorMode == EditorModeType.NextAction && _nextActionEditorMode == NextActionEditorModeType.Node)
                    RefreshActionSet(false);

                //if (Application.isPlaying && TestBattle.TestBattleController.Instance != null)
                //    TestBattle.TestBattleController.Instance.currSelectedAction = _selectedActionData;
            };

            _roList_ActionData.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                var actionData = _roList_ActionData.list[index] as ActionData;

                rect.y += 2;
                rect.width -= 55;
                rect.height = EditorGUIUtility.singleLineHeight;

                var rect_Copy = rect;
                rect_Copy.x = rect.x + rect.width;
                rect_Copy.width = 55;

                string str_label = null;
                if (string.IsNullOrWhiteSpace(actionData.Desc))
                    str_label = actionData.Name;
                else
                    str_label = $"{actionData.Name} :: {actionData.Desc}";

                var defaultGUIColor = GUI.color;
                if (actionData.eventData.list_ActionEvents.Count == 0)
                    GUI.color = Color.red;

                EditorGUI.LabelField(rect, str_label);
                if (GUI.Button(rect_Copy, "COPY"))
                {
                    TextEditor te = new TextEditor();
                    te.text = actionData.Name;
                    te.SelectAll();
                    te.Copy();
                }
                GUI.color = defaultGUIColor;
            };
        }

        void LoadAll_ActionDatas()
        {
            if (_dic_ActionDatas == null)
                _dic_ActionDatas = new SortedDictionary<string, ActionData>();
            else
                _dic_ActionDatas.Clear();

            _selectedActionData = null;

            var assetPaths = Directory.GetFiles(path_ActionData);
            if (assetPaths == null || assetPaths.Length == 0)
                return;

            float MaxCount = (float)assetPaths.Length;
            try
            {
                for (int i = 0; i < assetPaths.Length; ++i)
                {
                    var assetPath = assetPaths[i];
                    assetPath = assetPath.Replace("\\", "/");
                    EditorUtility.DisplayProgressBar("Load All ActionDatas", assetPath, (i + 1) / MaxCount);

                    if (!assetPath.EndsWith(".asset", StringComparison.OrdinalIgnoreCase)
                        || assetPath.EndsWith("_TRG.asset", StringComparison.OrdinalIgnoreCase)
                        || assetPath.EndsWith("_CDT.asset", StringComparison.OrdinalIgnoreCase)
                        || assetPath.EndsWith("_NXT.asset", StringComparison.OrdinalIgnoreCase))
                        continue;

                    // 액션 데이터 로드
                    var eventData = AssetDatabase.LoadAssetAtPath<ActionEventData>(assetPath) ;
                    if (eventData == null)
                        continue;

                    var newActionData = new ActionData();
                    newActionData.eventData = eventData;

                    // 트리거 데이터 로드
                    var triggerAssetPath = assetPath.Replace(eventData.ActionName, newActionData.triggerFileName);
                    var triggerData = AssetDatabase.LoadAssetAtPath<ActionTriggerData>(triggerAssetPath);
                    if (triggerData == null)
                    {
                        newActionData.triggerData = ScriptableObject.CreateInstance<ActionTriggerData>();
                        AssetDatabase.CreateAsset(newActionData.triggerData, triggerAssetPath);
                    }
                    else
                    {
                        newActionData.triggerData = triggerData;
                    }

                    // 컨디션 데이터 로드
                    var conditionAssetPath = assetPath.Replace(eventData.ActionName, newActionData.conditionFileName);
                    var conditionData = AssetDatabase.LoadAssetAtPath<ActionConditionData>(conditionAssetPath);
                    if (conditionData == null)
                    {
                        newActionData.conditionData = ScriptableObject.CreateInstance<ActionConditionData>();
                        AssetDatabase.CreateAsset(newActionData.conditionData, conditionAssetPath);
                    }
                    else
                    {
                        newActionData.conditionData = conditionData;
                    }

                    // Next 데이터 로드
                    var nextAssetPath = assetPath.Replace(eventData.ActionName, newActionData.nextFileName);
                    var nextData = AssetDatabase.LoadAssetAtPath<ActionNextData>(nextAssetPath);
                    if (nextData == null)
                    {
                        newActionData.nextData = ScriptableObject.CreateInstance<ActionNextData>();
                        AssetDatabase.CreateAsset(newActionData.nextData, nextAssetPath);
                    }
                    else
                    {
                        newActionData.nextData = nextData;
                    }

                    // 로드 된 데이터 세팅
                    _dic_ActionDatas.Add(eventData.ActionName, newActionData);
                }
            }
            catch (System.Exception e)
            {
                EditorUtility.ClearProgressBar();
                throw e;
            }

            EditorUtility.ClearProgressBar();
        }

        bool Save_ActionData(ActionData actionData, bool AssetDatabaseSave = true)
        {
            if (actionData == null)
                return false;
            
            bool bSave = false;

            // Event Data
            var actionDataPath = $"{path_ActionData}/{actionData.Name}.asset";
            actionDataPath = actionDataPath.Replace('\\', '/');

            var loadedEventAsset = AssetDatabase.LoadAssetAtPath<ActionEventData>(actionDataPath);
            if (loadedEventAsset == null)
            {
                AssetDatabase.CreateAsset(actionData.eventData, actionDataPath);
            }
            else
            {
                EditorUtility.SetDirty(loadedEventAsset);
                bSave = true;
            }

            // Trigger Data
            var triggerDataPath = $"{path_ActionData}/{actionData.triggerFileName}.asset";
            triggerDataPath = triggerDataPath.Replace('\\', '/');

            var loadedTriggerAsset = AssetDatabase.LoadAssetAtPath<ActionTriggerData>(triggerDataPath);
            if (loadedTriggerAsset == null)
            {
                AssetDatabase.CreateAsset(actionData.triggerData, triggerDataPath);
            }
            else
            {
                EditorUtility.SetDirty(loadedTriggerAsset);
                bSave = true;
            }

            // Condition Data
            var conditionDataPath = $"{path_ActionData}/{actionData.conditionFileName}.asset";
            conditionDataPath = conditionDataPath.Replace('\\', '/');

            var loadedConditionAsset = AssetDatabase.LoadAssetAtPath<ActionConditionData>(conditionDataPath);
            if (loadedConditionAsset == null)
            {
                AssetDatabase.CreateAsset(actionData.conditionData, conditionDataPath);
            }
            else
            {
                EditorUtility.SetDirty(loadedConditionAsset);
                bSave = true;
            }

            // Next Data
            var nextDataPath = $"{path_ActionData}/{actionData.nextFileName}.asset";
            nextDataPath = nextDataPath.Replace('\\', '/');

            var loadedNextAsset = AssetDatabase.LoadAssetAtPath<ActionNextData>(nextDataPath);
            if (loadedNextAsset == null)
            {
                AssetDatabase.CreateAsset(actionData.nextData, nextDataPath);
            }
            else
            {
                EditorUtility.SetDirty(loadedNextAsset);
                bSave = true;
            }

            if (AssetDatabaseSave && bSave)
                AssetDatabase.SaveAssets();

            return bSave;
        }

        void Delete_ActionData(ActionData actionData)
        {
            if (actionData == null)
                return;

            if (EditorUtility.DisplayDialog("ActionData 삭제", "정말로 삭제 하시겠습니까?\n\n삭제된 데이터는 휴지통으로 이동됩니다.", "확인", "취소"))
            {
                if (_set_SelectedActionNames.Contains(actionData.Name))
                    _set_SelectedActionNames.Remove(actionData.Name);

                if (!_dic_ActionDatas.ContainsKey(actionData.Name))
                    return;

                _dic_ActionDatas.Remove(actionData.Name);

                if (_selectedActionData != null && _selectedActionData == actionData)
                    _selectedActionData = null;

                var actionDataPath = $"{path_ActionData}/{actionData.eventFileName}.asset";
                var triggerDataPath = $"{path_ActionData}/{actionData.triggerFileName}.asset";
                var conditionDataPath = $"{path_ActionData}/{actionData.conditionFileName}.asset";
                var nextDataPath = $"{path_ActionData}/{actionData.nextFileName}.asset";

                AssetDatabase.MoveAssetToTrash(actionDataPath);
                AssetDatabase.MoveAssetToTrash(triggerDataPath);
                AssetDatabase.MoveAssetToTrash(conditionDataPath);
                AssetDatabase.MoveAssetToTrash(nextDataPath);
                AssetDatabase.Refresh();
            }
        }

        void Create_ActionData(string str_Search, ActionData copySrc_ActionDataPair)
        {
            string actionName = str_Search;
            if (string.IsNullOrWhiteSpace(actionName))
            {
                EditorUtility.DisplayDialog("ActionData 생성 실패", "ActionName은 공백 일 수 없습니다.", "확인");
                return;
            }

            if (_dic_ActionDatas.ContainsKey(actionName))
            {
                EditorUtility.DisplayDialog("ActionData 생성 실패", "이미 존재하는 ActionName 입니다.", "확인");
                return;
            }

            ActionData newActionData = new ActionData();
            if (copySrc_ActionDataPair != null)
            {
                newActionData.eventData = Instantiate(copySrc_ActionDataPair.eventData);
                newActionData.triggerData = Instantiate(copySrc_ActionDataPair.triggerData);
                newActionData.conditionData = Instantiate(copySrc_ActionDataPair.conditionData);
                newActionData.nextData = Instantiate(copySrc_ActionDataPair.nextData);
            }
            else
            {
                newActionData.eventData = ScriptableObject.CreateInstance<ActionEventData>();
                newActionData.triggerData = ScriptableObject.CreateInstance<ActionTriggerData>();
                newActionData.conditionData = ScriptableObject.CreateInstance<ActionConditionData>();
                newActionData.nextData = ScriptableObject.CreateInstance<ActionNextData>();
            }

            // 새로 만든다.
            newActionData.eventData.name = actionName;
            newActionData.triggerData.name = $"{actionName}_TRG";

            // 저장
            Save_ActionData(newActionData);

            // 맵에 추가
            _dic_ActionDatas.Add(newActionData.Name, newActionData);

            // 선택
            _selectedActionData = newActionData;
        }

        public static void AutoFlexibleSpace()
        {
            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
    }
}