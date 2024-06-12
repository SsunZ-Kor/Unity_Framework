using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game.Character.Action
{
    [System.Serializable]
    public class ActionEventData : ScriptableObject, ISerializationCallbackReceiver
    {
        public string ActionName => this.name;
        public string ActionDesc;
        public string ActionSetName;

        public float Length = 1f;
        public float TotalDamageWeight;

        [System.NonSerialized]
        public List<ActionEventBase> list_ActionEvents = new List<ActionEventBase>();

        public List<ActionEventAnimator> list_ActionEventAnimator = new List<ActionEventAnimator>();
        public List<ActionEventAttack> list_ActionEventAttack = new List<ActionEventAttack>();
        public List<ActionEventShotFX> list_ActionEventShotFX = new List<ActionEventShotFX>();
        public List<ActionEventStopFX> list_ActionEventStopFX = new List<ActionEventStopFX>();
        public List<ActionEventLookAt> list_ActionEventLookAt = new List<ActionEventLookAt>();
        public List<ActionEventMove> list_ActionEventMove = new List<ActionEventMove>();
        public List<ActionEventMoveByVelocity> list_ActionEventMoveByVelocity = new List<ActionEventMoveByVelocity>();
        public List<ActionEventProjectile> list_ActionEventProjectile = new List<ActionEventProjectile>();
        public List<ActionEventShotSFX> list_ActionEventShotSFX = new List<ActionEventShotSFX>();
        public List<ActionEventStopSFX> list_ActionEventStopSFX = new List<ActionEventStopSFX>();
        public List<ActionEventCamMove> list_ActionEventCamMove = new List<ActionEventCamMove>();
        public List<ActionEventAddStat> list_ActionEventAddStat = new List<ActionEventAddStat>();


        public void GetAssetBundlePackageNames(ref HashSet<string> set_BundlePackageName)
        {
            if (set_BundlePackageName == null)
                set_BundlePackageName = new HashSet<string>();

            for (int i = 0; i < list_ActionEvents.Count; ++i)
            {
                var actionEvent = list_ActionEvents[i];
                if (actionEvent == null)
                    continue;

                actionEvent.GetAssetBundlePackageNames(ref set_BundlePackageName);
            }
        }

        public void OnBeforeSerialize()
        {
            list_ActionEventAnimator.Clear();
            list_ActionEventAttack.Clear();
            list_ActionEventShotFX.Clear();
            list_ActionEventStopFX.Clear();
            list_ActionEventLookAt.Clear();
            list_ActionEventMove.Clear();
            list_ActionEventMoveByVelocity.Clear();
            list_ActionEventProjectile.Clear();
            list_ActionEventShotSFX.Clear();
            list_ActionEventStopSFX.Clear();
            list_ActionEventCamMove.Clear();
            list_ActionEventAddStat.Clear();

            TotalDamageWeight = 0f;

            for (int i = 0; i < list_ActionEvents.Count; ++i)
            {
                // 각 저장소에 분류
                var actionEvent = list_ActionEvents[i];
                if      (actionEvent is ActionEventAnimator)       list_ActionEventAnimator      .Add(actionEvent as ActionEventAnimator)      ;
                else if (actionEvent is ActionEventAttack)         list_ActionEventAttack        .Add(actionEvent as ActionEventAttack)        ;
                else if (actionEvent is ActionEventShotFX)         list_ActionEventShotFX        .Add(actionEvent as ActionEventShotFX)        ;
                else if (actionEvent is ActionEventStopFX)         list_ActionEventStopFX        .Add(actionEvent as ActionEventStopFX)        ;
                else if (actionEvent is ActionEventLookAt)         list_ActionEventLookAt        .Add(actionEvent as ActionEventLookAt)        ;
                else if (actionEvent is ActionEventMove)           list_ActionEventMove          .Add(actionEvent as ActionEventMove)          ;
                else if (actionEvent is ActionEventMoveByVelocity) list_ActionEventMoveByVelocity.Add(actionEvent as ActionEventMoveByVelocity);
                else if (actionEvent is ActionEventProjectile)     list_ActionEventProjectile    .Add(actionEvent as ActionEventProjectile)    ;
                else if (actionEvent is ActionEventShotSFX)        list_ActionEventShotSFX       .Add(actionEvent as ActionEventShotSFX)       ;
                else if (actionEvent is ActionEventStopSFX)        list_ActionEventStopSFX       .Add(actionEvent as ActionEventStopSFX)       ;
                else if (actionEvent is ActionEventCamMove)        list_ActionEventCamMove       .Add(actionEvent as ActionEventCamMove)       ;
                else if (actionEvent is ActionEventAddStat)        list_ActionEventAddStat       .Add(actionEvent as ActionEventAddStat)       ;

                // TotalDamageWeight 계산
                 if (actionEvent is ActionEventAttack)
                    TotalDamageWeight += (actionEvent as ActionEventAttack)    .attackData.damageWeight;
                else if (actionEvent is ActionEventProjectile) 
                    TotalDamageWeight += (actionEvent as ActionEventProjectile).attackData.damageWeight;
            }
        }

        public void OnAfterDeserialize()
        {
            var totalEventCount = 0;

            totalEventCount += list_ActionEventAnimator      .Count;
            totalEventCount += list_ActionEventAttack        .Count;
            totalEventCount += list_ActionEventShotFX        .Count;
            totalEventCount += list_ActionEventStopFX        .Count;
            totalEventCount += list_ActionEventLookAt        .Count;
            totalEventCount += list_ActionEventMove          .Count;
            totalEventCount += list_ActionEventMoveByVelocity.Count;
            totalEventCount += list_ActionEventProjectile    .Count;
            totalEventCount += list_ActionEventShotSFX       .Count;
            totalEventCount += list_ActionEventStopSFX       .Count;
            totalEventCount += list_ActionEventCamMove       .Count;
            totalEventCount += list_ActionEventAddStat       .Count;

            list_ActionEvents.Clear();
            list_ActionEvents.Capacity = totalEventCount;

            for (int i = 0; i < list_ActionEventAnimator.Count; ++i)
                list_ActionEvents.Add(list_ActionEventAnimator[i]);
            for (int i = 0; i < list_ActionEventAttack.Count; ++i)
                list_ActionEvents.Add(list_ActionEventAttack[i]);
            for (int i = 0; i < list_ActionEventShotFX.Count; ++i)
                list_ActionEvents.Add(list_ActionEventShotFX[i]);
            for (int i = 0; i < list_ActionEventStopFX.Count; ++i)
                list_ActionEvents.Add(list_ActionEventStopFX[i]);
            for (int i = 0; i < list_ActionEventLookAt.Count; ++i)
                list_ActionEvents.Add(list_ActionEventLookAt[i]);
            for (int i = 0; i < list_ActionEventMove.Count; ++i)
                list_ActionEvents.Add(list_ActionEventMove[i]);
            for (int i = 0; i < list_ActionEventMoveByVelocity.Count; ++i)
                list_ActionEvents.Add(list_ActionEventMoveByVelocity[i]);
            for (int i = 0; i < list_ActionEventProjectile.Count; ++i)
                list_ActionEvents.Add(list_ActionEventProjectile[i]);
            for (int i = 0; i < list_ActionEventShotSFX.Count; ++i)
                list_ActionEvents.Add(list_ActionEventShotSFX[i]);
            for (int i = 0; i < list_ActionEventStopSFX.Count; ++i)
                list_ActionEvents.Add(list_ActionEventStopSFX[i]);
            for (int i = 0; i < list_ActionEventCamMove.Count; ++i)
                list_ActionEvents.Add(list_ActionEventCamMove[i]);
            for (int i = 0; i < list_ActionEventAddStat.Count; ++i)
                list_ActionEvents.Add(list_ActionEventAddStat[i]);

            SortActionEvents();
        }

        public virtual void SortActionEvents()
        {
            list_ActionEvents.Sort((x, y) => {
                var result = x.StartTime.CompareTo(y.StartTime);
                if (result != 0)
                    return result;

                var xEx = x as ActionEventDurationDataBase;
                var yEx = y as ActionEventDurationDataBase;

                if (xEx != null && yEx != null)
                {
                    result = xEx.EndTime.CompareTo(yEx.EndTime);
                    if (result != 0)
                        return result;

                    return x.GetType().Name.CompareTo(y.GetType().Name);
                }
                else if (xEx == null && yEx == null)
                {
                    return x.GetType().Name.CompareTo(y.GetType().Name);
                }
                else if (xEx == null)
                {
                    return -1;
                }
                else if (yEx == null)
                {
                    return 1;
                }

                return 0;
            });
        }

#if UNITY_EDITOR
        public static List<ActionEventBase> List_CopyedActionEvents = new List<ActionEventBase>();

        private static List<Type> _list_EventTypes = null;
        private static List<string> _list_EventTypeNames = null;
        public static List<Type> List_EventTypes 
        {
            get
            {
                if (_list_EventTypes == null)
                {
                    _list_EventTypes = new List<Type>();

                    var type_ActionEventDataBase = typeof(ActionEventBase);
                    foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        foreach (var t in a.GetTypes())
                        {
                            if (t.IsSubclassOf(type_ActionEventDataBase) && !t.Name.Contains("Base"))
                            {
                                _list_EventTypes.Add(t);
                            }
                        }
                    }
                }

                return _list_EventTypes;
            }
        }
        public static List<string> List_EventTypeNames
        {
            get
            {
                if (_list_EventTypeNames == null)
                {
                    _list_EventTypeNames = new List<string>();

                    var type_ActionEventDataBase = typeof(ActionEventBase);
                    foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        foreach (var t in a.GetTypes())
                        {
                            if (t.IsSubclassOf(type_ActionEventDataBase) && !t.Name.Contains("Base"))
                            {
                                _list_EventTypeNames.Add(t.Name.Replace("ActionEvent", "").Replace("Data", ""));
                            }
                        }
                    }
                }

                return _list_EventTypeNames;
            }
        }

        public void OnGUI_Detail(ActionData actionData)
        {
            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Sort"))
                {
                    SortActionEvents();
                }

                bool bSelected = false;
                for (int i = 0; i < list_ActionEvents.Count; ++i)
                {
                    if (list_ActionEvents[i].isSelected)
                    {
                        bSelected = true;
                        break;
                    }
                }

                EditorGUI.BeginDisabledGroup(!bSelected);
                if (GUILayout.Button("Copy"))
                {
                    List_CopyedActionEvents.Clear();
                    for (int i = 0; i < list_ActionEvents.Count; ++i)
                    {
                        if (list_ActionEvents[i].isSelected)
                            List_CopyedActionEvents.Add(list_ActionEvents[i].Clone());
                    }
                }
                EditorGUI.EndDisabledGroup();

                EditorGUI.BeginDisabledGroup(List_CopyedActionEvents == null || List_CopyedActionEvents.Count <= 0);
                if (GUILayout.Button("Paste"))
                {
                    for (int i = 0; i < List_CopyedActionEvents.Count; ++i)
                    {
                        var eventData = List_CopyedActionEvents[i].Clone();
                        this.list_ActionEvents.Add(eventData);
                    }
                }
                EditorGUI.EndDisabledGroup();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Select All"))
                {
                    for (int i = 0; i < list_ActionEvents.Count; ++i)
                        list_ActionEvents[i].isSelected = true;
                }
                if (GUILayout.Button("Deselect All"))
                {
                    for (int i = 0; i < list_ActionEvents.Count; ++i)
                        list_ActionEvents[i].isSelected = false;
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginVertical("Box");
            {
                for (int i = 0; i < List_EventTypes.Count; ++i)
                {
                    if (GUILayout.Button($"Add {List_EventTypeNames[i]}"))
                    {
                        var newEventData = Activator.CreateInstance(List_EventTypes[i]) as ActionEventBase;
                        list_ActionEvents.Add(newEventData);
                        break;
                    }
                }
            }
            EditorGUILayout.EndVertical();
        }

        public void OnGUI_List(ActionData actionData)
        {
            if (list_ActionEvents.Count == 0)
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
            string prevEventData = null;
            for (int i = 0; i < list_ActionEvents.Count; ++i)
            {
                var eventData = list_ActionEvents[i];

                if (prevEventData != eventData.GetType().Name)
                {
                    prevEventData = eventData.GetType().Name;
                    EditorGUILayout.LabelField(prevEventData.Replace("ActionEvent", string.Empty).Replace("Data", string.Empty), EditorStyles.boldLabel);
                }

                EditorGUILayout.BeginHorizontal("Button");
                {
                    EditorGUILayout.BeginVertical();
                    {
                        list_ActionEvents[i].OnGUI(actionData, i);
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
                list_ActionEvents.RemoveAt(idx_delete);
        }
#endif
    }
}
