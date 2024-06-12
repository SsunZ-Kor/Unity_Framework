using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game.Character.Action
{
    [System.Serializable]
    public class ActionEventAddStat : ActionEventBase
    {
        public enum StatType
        { 
            Dodge,
            Bullet,
        }

        public override bool IgnoreOnNetCharCtrl => true;

        public StatType Stat;
        public int Value;

        public override IActionEventRuntime CreateRuntime(CharacterObject owner, ActionRuntime actionDataRuntime)
        {
            return new ActionEventAddStatRuntime(owner, actionDataRuntime, this);
        }

        public override void GetAssetBundlePackageNames(ref HashSet<string> set_BundlePackageName)
        {
        }

        public override ActionEventBase Clone()
        {
            return new ActionEventAddStat()
            {
                StartTime = this.StartTime,

                Stat = this.Stat,
                Value = this.Value,
            };
        }

#if UNITY_EDITOR
        public override void OnGUI(ActionData actionData, int index)
        {
            base.OnGUI(actionData, index);

            EditorGUILayout.BeginHorizontal();
            {
                Stat = (StatType)EditorGUILayout.EnumPopup("StatType", Stat);
                Value = EditorGUILayout.IntField(Value);
            }
            EditorGUILayout.EndHorizontal();
        }
#endif
    }

    public class ActionEventAddStatRuntime : ActionEventRuntimeBase<ActionEventAddStat>
    {
        public ActionEventAddStatRuntime(CharacterObject owner, ActionRuntime actionDataRuntime, ActionEventAddStat data) : base(owner, actionDataRuntime, data)
        {

        }

        public override void Init()
        {
        }

        public override void OnStart(float actionElapsedTime)
        {
            base.OnStart(actionElapsedTime);

            switch (_eventData.Stat)
            {
                case ActionEventAddStat.StatType.Bullet:
                    _owner.StatCtrl.AddBulletCount(_eventData.Value);
                    break;
                case ActionEventAddStat.StatType.Dodge:
                    _owner.StatCtrl.AddDodgeCount(_eventData.Value);
                    break;
            }
        }
    }
}