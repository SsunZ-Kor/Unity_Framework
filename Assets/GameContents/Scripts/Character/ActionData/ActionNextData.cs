using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Game.Character.Action
{
    public enum NextActionTimeType
    { 
        All,
        End,
        Custom,
    }

    public enum NextActionSpaceType
    { 
        Air,
        Ground,
        AirAndGround,
    }

    [System.Serializable]
    public class NextAction
    {
        public NextActionTimeType TimeType;
        public float StartTime;

        public bool isEqualLength;
        public float EndTime;

        public bool SyncPrevElapsedTime = false;
        public NextActionSpaceType SpaceType;
        public string NextActionName;

        [System.NonSerialized]
        public ActionData NextActionData;

        public NextAction Clone()
        {
            return new NextAction()
            {
                TimeType = this.TimeType,
                StartTime = this.StartTime,
                isEqualLength = this.isEqualLength,
                EndTime = this.EndTime,

                SyncPrevElapsedTime = this.SyncPrevElapsedTime,
                SpaceType = this.SpaceType,
                NextActionName = (string)NextActionName.Clone(),
            };
        }

        public bool CheckStartTime(float elapsedTime, float length)
        {
            switch (TimeType)
            {
                case NextActionTimeType.All:
                    return true;
                case NextActionTimeType.End:
                    return elapsedTime >= length;
                default:
                    return elapsedTime >= StartTime;
            }
        }

        public bool CheckEndTime(float elapsedTime, float length)
        {
            switch (TimeType)
            {
                case NextActionTimeType.All:
                case NextActionTimeType.End:
                    return elapsedTime >= length;
                default:
                    if (isEqualLength)
                        return elapsedTime >= length;
                    else
                        return elapsedTime >= EndTime;
            }
        }

        public bool CheckTime(float elapsedTime, float length)
        {
            switch (TimeType)
            {
                case NextActionTimeType.All:
                    return true;
                case NextActionTimeType.End:
                    return StartTime <= elapsedTime;
                case NextActionTimeType.Custom:
                    return (StartTime <= elapsedTime) && (elapsedTime <= (isEqualLength ? length : EndTime));
            }

            return false;
        }

        public bool CheckSpace(NextActionSpaceType spaceType)
        {
            return SpaceType == NextActionSpaceType.AirAndGround || SpaceType == spaceType;
        }

        public bool CheckCondition(CharacterObject owner)
        {
            return NextActionData.conditionData.CheckCondition(owner);
        }

        public bool CheckTrigger(CharacterObject owner)
        {
            return NextActionData.triggerData.CheckTrigger(owner);
        }

#if UNITY_EDITOR
        [System.NonSerialized]
        public bool isSelected;

        public void OnGUI(ActionData actionData, int index)
        {
            EditorGUILayout.BeginHorizontal();
            {
                isSelected = EditorGUILayout.Toggle(isSelected, GUILayout.Width(20f));

                EditorGUILayout.BeginVertical(GUILayout.Width(200f));
                {
                    var defaultLabelWidth = EditorGUIUtility.labelWidth;
                    EditorGUIUtility.labelWidth = 100f;
                    {
                        TimeType = (NextActionTimeType)EditorGUILayout.EnumPopup("TimeType", TimeType);
                        EditorGUI.BeginDisabledGroup(TimeType != NextActionTimeType.Custom);
                        {
                            isEqualLength = EditorGUILayout.Toggle("EquelLength", isEqualLength);
                        }
                        EditorGUI.EndDisabledGroup();
                        SpaceType = (NextActionSpaceType)EditorGUILayout.EnumPopup("Space", SpaceType);
                    }
                    EditorGUIUtility.labelWidth = defaultLabelWidth;

                    switch (TimeType)
                    {
                        case NextActionTimeType.All:
                            StartTime = 0;
                            EndTime = actionData.Length;
                            break;
                        case NextActionTimeType.End:
                            StartTime = actionData.Length;
                            EndTime = actionData.Length;
                            break;
                        case NextActionTimeType.Custom:
                            if (isEqualLength)
                                EndTime = actionData.Length;
                            break;
                    }
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical();
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUI.BeginDisabledGroup(TimeType != NextActionTimeType.Custom);
                        {
                            StartTime = EditorGUILayout.Slider(StartTime, 0f, actionData.Length);
                            StartTime = Mathf.Clamp(EditorGUILayout.FloatField(StartTime * 30f, GUILayout.Width(50f)) / 30f, 0f, actionData.Length);
                        }
                        EditorGUI.EndDisabledGroup();
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUI.BeginDisabledGroup(TimeType != NextActionTimeType.Custom || isEqualLength);
                        {
                            EndTime = Mathf.Max(EditorGUILayout.Slider(EndTime, 0f, actionData.Length), StartTime);
                            EndTime = Mathf.Clamp(EditorGUILayout.FloatField(EndTime * 30f, GUILayout.Width(50f)) / 30f, StartTime, actionData.Length);
                        }
                        EditorGUI.EndDisabledGroup();
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    {
                        NextActionName = EditorGUILayout.TextField("NextActionName", NextActionName);
                        SyncPrevElapsedTime = EditorGUILayout.Toggle("SyncPrevElaspedTime", SyncPrevElapsedTime, GUILayout.Width(165f));
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndVertical();

            }
            EditorGUILayout.EndHorizontal();

        }
#endif
    }

    public class ActionNextData : ScriptableObject
    {
        public List<NextAction> list_NextAction = new List<NextAction>();

        // 런타임 중에 사용할 정리된 데이터
        [System.NonSerialized]
        public List<NextAction> list_NextAction_OnUpdate = null;
        [System.NonSerialized]
        public List<NextAction> list_NextAction_OnEnd = null;
        [System.NonSerialized]
        public SortedDictionary<TriggerEventType, List<NextAction>> dic_NextAction_Event = null;

#if UNITY_EDITOR
        public bool isInitNodePositioin = false;
        public Vector2 EditorNodePosition = Vector2.negativeInfinity;
        public Vector2 EditorNodeCenterPosition => EditorNodePosition + (EditorNodeSize * 0.5f);
        public static Vector2 EditorNodeSize = new Vector2(200f, 75f);

        public static List<NextAction> List_CopyedNextAction = new List<NextAction>();

        public void OnGUI_Detail(ActionData actionData)
        {
            EditorGUILayout.BeginHorizontal();
            {
                bool bSelected = false;
                for (int i = 0; i < list_NextAction.Count; ++i)
                {
                    if (list_NextAction[i].isSelected)
                    {
                        bSelected = true;
                        break;
                    }
                }

                EditorGUI.BeginDisabledGroup(!bSelected);
                if (GUILayout.Button("Copy"))
                {
                    List_CopyedNextAction.Clear();
                    for (int i = 0; i < list_NextAction.Count; ++i)
                    {
                        if (list_NextAction[i].isSelected)
                            List_CopyedNextAction.Add(list_NextAction[i].Clone());
                    }
                }
                EditorGUI.EndDisabledGroup();

                EditorGUI.BeginDisabledGroup(List_CopyedNextAction == null || List_CopyedNextAction.Count <= 0);
                if (GUILayout.Button("Paste"))
                {
                    for (int i = 0; i < List_CopyedNextAction.Count; ++i)
                    {
                        var eventData = List_CopyedNextAction[i].Clone();
                    this.list_NextAction.Add(eventData);
                    }
                }
                EditorGUI.EndDisabledGroup();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Select All"))
                {
                    for (int i = 0; i < list_NextAction.Count; ++i)
                        list_NextAction[i].isSelected = true;
                }
                if (GUILayout.Button("Deselect All"))
                {
                    for (int i = 0; i < list_NextAction.Count; ++i)
                        list_NextAction[i].isSelected = false;
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginVertical("Box");
            {
                if (GUILayout.Button("Add NextAction"))
                    list_NextAction.Add(new NextAction());
                
                if (GUILayout.Button("Add Default NextAction"))
                {
                    var newNextAction_Idle = new NextAction()
                    {
                        TimeType = NextActionTimeType.End,
                        NextActionName = "---- IDLE ----",
                        SpaceType = NextActionSpaceType.Ground,
                        isEqualLength = false,
                        SyncPrevElapsedTime = false,
                    };

                    var newNextAction_IdleAir = new NextAction()
                    {
                        TimeType = NextActionTimeType.All,
                        NextActionName = "---- IDLE AIR ----",
                        SpaceType = NextActionSpaceType.Air,
                        isEqualLength = false,
                        SyncPrevElapsedTime = false,
                    };

                    var newNextAction_Move = new NextAction()
                    {
                        TimeType = NextActionTimeType.End,
                        NextActionName = "---- MOVE ----",
                        SpaceType = NextActionSpaceType.Ground,
                        isEqualLength = false,
                        SyncPrevElapsedTime = false,
                    };

                    list_NextAction.Add(newNextAction_Idle);
                    list_NextAction.Add(newNextAction_IdleAir);
                    list_NextAction.Add(newNextAction_Move);
                }

                if (GUILayout.Button("Add Loop"))
                {
                    var newNextAction_Loop = new NextAction()
                    {
                        TimeType = NextActionTimeType.End,
                        NextActionName = (string)actionData.Name.Clone(),
                        SpaceType = NextActionSpaceType.AirAndGround,
                        isEqualLength = false,
                        SyncPrevElapsedTime = false,
                    };

                    list_NextAction.Add(newNextAction_Loop);
                }
            }
            EditorGUILayout.EndVertical();
        }

        public void OnGUI_List(ActionData actionData)
        {
            if (list_NextAction.Count == 0)
            {
                GUILayout.BeginVertical();
                GUILayout.FlexibleSpace();
                GUILayout.EndVertical();
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                return;
            }

            var idx_delete = -1;
            for (int i = 0; i < list_NextAction.Count; ++i)
            {
                EditorGUILayout.BeginHorizontal("Button");
                {
                    EditorGUILayout.BeginVertical();
                    {
                        list_NextAction[i].OnGUI(actionData, i);
                    }
                    EditorGUILayout.EndVertical();

                    var defaultBGColor = GUI.backgroundColor;
                    GUI.backgroundColor = Color.red;
                    if (GUILayout.Button("X", GUILayout.Width(20f)))
                        idx_delete = i;
                    GUI.backgroundColor = defaultBGColor;
                }
                EditorGUILayout.EndHorizontal();
            }

            if (idx_delete >= 0)
                list_NextAction.RemoveAt(idx_delete);
        }
#endif

        public virtual void InitNextAction()
        {
            if (list_NextAction == null || list_NextAction.Count == 0)
                return;

            list_NextAction.Sort((x, y) => {
                var result = x.StartTime.CompareTo(y.StartTime);
                if (result != 0)
                    return result;

                if (x.NextActionData != null && y.NextActionData != null)
                {
                    var xTrigger = x.NextActionData.triggerData;
                    var yTrigger = y.NextActionData.triggerData;

                    // TriggerEvent 사용 여부
                    result = yTrigger.EventType.CompareTo(xTrigger.EventType);
                    if (result != 0)
                        return result;

                    // OnAim 사용 여부
                    result = yTrigger.OnAim.CompareTo(xTrigger.OnAim);
                    if (result != 0)
                        return result;

                    // 조이스틱 사용 유무
                    result = yTrigger.bUseJoystick.CompareTo(xTrigger.bUseJoystick);
                    if (result != 0)
                        return result;

                    result = xTrigger.JoystickPowerTriggerType.CompareTo(yTrigger.JoystickPowerTriggerType);
                    if (result != 0)
                        return result;
                }

                result = x.EndTime.CompareTo(y.EndTime);
                if (result != 0)
                    return result;

                return 0;
            });

            for (int i = 0;i < list_NextAction.Count; ++i)
            {
                var nextAction = list_NextAction[i];
                var actionTrigger = nextAction.NextActionData.triggerData;
                if (actionTrigger.EventType == TriggerEventType.None)
                {

                    if (nextAction.TimeType == NextActionTimeType.End)
                    {
                        if (list_NextAction_OnEnd == null)
                            list_NextAction_OnEnd = new List<NextAction>();

                        list_NextAction_OnEnd.Add(list_NextAction[i]);
                    }
                    else
                    {
                        if (list_NextAction_OnUpdate == null)
                            list_NextAction_OnUpdate = new List<NextAction>();

                        list_NextAction_OnUpdate.Add(list_NextAction[i]);
                    }
                }
                else
                {
                    if (dic_NextAction_Event == null)
                        dic_NextAction_Event = new SortedDictionary<TriggerEventType, List<NextAction>>();

                    var list_NextEventAction = dic_NextAction_Event.GetOrCreate(actionTrigger.EventType);
                    list_NextEventAction.Add(list_NextAction[i]);
                }
            }
        }
    }
}

