using Game.Character;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game.Character.Action
{
    public enum ActionConditionType
    {
        None,
        OverHp,
        OverOrSameHp,
        UnderHp,
        UnderOrSameHp,

        BulletCountOver,
        BulletCountOverOrSame,
        BulletCountUnder,
        BulletCountUnderOrSame,

        DodgeCountOver,
        DodgeCountOverOrSame,
        DodgeCountUnder,
        DodgeCountUnderOrSame,
    }

    [System.Serializable]
    public class ActionCondition
    {
        public ActionConditionType Type;
        public float Value;

        public ActionCondition Clone()
        {
            return new ActionCondition()
            {
                Type = this.Type,
                Value = this.Value,
            };
        }

#if UNITY_EDITOR
        public bool isSelected;

        public void OnGUI(ActionData actionData, int index)
        {
            EditorGUILayout.BeginHorizontal("Box");
            {
                Type = (ActionConditionType)EditorGUILayout.EnumPopup(Type);
                Value = EditorGUILayout.FloatField(Value);
            }
            EditorGUILayout.EndHorizontal();
        }
#endif
    }

    public class ActionConditionData : ScriptableObject
    {
        public List<ActionCondition> list_Conditions = new List<ActionCondition>();

        public bool CheckCondition(CharacterObject owner)
        {
            if (owner == null || list_Conditions == null)
                return true;

            for (int i = 0; i < list_Conditions.Count; ++i)
            {
                var condition = list_Conditions[i];

                switch (condition.Type)
                {
                    case ActionConditionType.OverHp: 
                        { if (owner.StatCtrl.CrrHpFactor <= condition.Value) return false; else break; }
                    case ActionConditionType.OverOrSameHp: 
                        { if (owner.StatCtrl.CrrHpFactor < condition.Value) return false; else break; }
                    case ActionConditionType.UnderHp: 
                        { if (owner.StatCtrl.CrrHpFactor >= condition.Value) return false; else break; }
                    case ActionConditionType.UnderOrSameHp: 
                        { if (owner.StatCtrl.CrrHpFactor > condition.Value) return false; else break; }
                    case ActionConditionType.BulletCountOver: 
                        { if (owner.StatCtrl.CrrBulletCount <= condition.Value) return false; else break; }
                    case ActionConditionType.BulletCountOverOrSame: 
                        { if (owner.StatCtrl.CrrBulletCount < condition.Value) return false; else break; }
                    case ActionConditionType.BulletCountUnder: 
                        { if (owner.StatCtrl.CrrBulletCount >= condition.Value) return false; else break; }
                    case ActionConditionType.BulletCountUnderOrSame:
                        { if (owner.StatCtrl.CrrBulletCount > condition.Value) return false; else break; }
                    case ActionConditionType.DodgeCountOver: 
                        { if (owner.StatCtrl.CrrDodgeCount <= condition.Value) return false; else break; }
                    case ActionConditionType.DodgeCountOverOrSame: 
                        { if (owner.StatCtrl.CrrDodgeCount < condition.Value) return false; else break; }
                    case ActionConditionType.DodgeCountUnder: 
                        { if (owner.StatCtrl.CrrDodgeCount >= condition.Value) return false; else break; }
                    case ActionConditionType.DodgeCountUnderOrSame: 
                        { if (owner.StatCtrl.CrrDodgeCount > condition.Value) return false; else break; }
                }
            }

            return true;
        }

        public ActionConditionData Clone()
        {
            var clone = new ActionConditionData()
            {
                list_Conditions = new List<ActionCondition>(this.list_Conditions.Count),
            };

            for (int i = 0; i < list_Conditions.Count; ++i)
                clone.list_Conditions.Add(list_Conditions[i].Clone());

            return clone;
        }

#if UNITY_EDITOR
        public static List<ActionCondition> List_CopyedActionCondition = new List<ActionCondition>();

        public void OnGUI_Detail(ActionData actionData)
        {
            EditorGUILayout.BeginHorizontal();
            {
                bool bSelected = false;
                for (int i = 0; i < list_Conditions.Count; ++i)
                {
                    if (list_Conditions[i].isSelected)
                    {
                        bSelected = true;
                        break;
                    }
                }

                EditorGUI.BeginDisabledGroup(!bSelected);
                if (GUILayout.Button("Copy"))
                {
                    List_CopyedActionCondition.Clear();
                    for (int i = 0; i < list_Conditions.Count; ++i)
                    {
                        if (list_Conditions[i].isSelected)
                            List_CopyedActionCondition.Add(list_Conditions[i].Clone());
                    }
                }
                EditorGUI.EndDisabledGroup();

                EditorGUI.BeginDisabledGroup(List_CopyedActionCondition == null || List_CopyedActionCondition.Count <= 0);
                if (GUILayout.Button("Paste"))
                {
                    for (int i = 0; i < List_CopyedActionCondition.Count; ++i)
                    {
                        var eventData = List_CopyedActionCondition[i].Clone();
                        this.list_Conditions.Add(eventData);
                    }
                }
                EditorGUI.EndDisabledGroup();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Select All"))
                {
                    for (int i = 0; i < List_CopyedActionCondition.Count; ++i)
                        List_CopyedActionCondition[i].isSelected = true;
                }
                if (GUILayout.Button("Deselect All"))
                {
                    for (int i = 0; i < List_CopyedActionCondition.Count; ++i)
                        List_CopyedActionCondition[i].isSelected = false;
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginVertical("Box");
            {
                if (GUILayout.Button("Add Condition"))
                    list_Conditions.Add(new ActionCondition());
            }
            EditorGUILayout.EndVertical();
        }

        public void OnGUI_List(ActionData actionData)
        {
            if (list_Conditions.Count == 0)
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
            for (int i = 0; i < list_Conditions.Count; ++i)
            {
                EditorGUILayout.BeginHorizontal("Button");
                {
                    EditorGUILayout.BeginVertical();
                    {
                        list_Conditions[i].OnGUI(actionData, i);
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
                list_Conditions.RemoveAt(idx_delete);
        }
#endif
    }
}

