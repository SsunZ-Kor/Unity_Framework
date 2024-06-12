using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game.Character.Action
{
    [System.Serializable]
    public class ActionEventAttack : ActionEventDurationDataBase
    {
        public override bool IgnoreOnNetCharCtrl => true;

        public string AttackColName = null;

        public AttackableObject.AttackData attackData;

        public override IActionEventRuntime CreateRuntime(CharacterObject owner, ActionRuntime actionDataRuntime)
        {
            return new ActionEventAttackRuntime(owner, actionDataRuntime, this);
        }

        public override void GetAssetBundlePackageNames(ref HashSet<string> set_BundlePackageName)
        {
        }

        public override ActionEventBase Clone()
        {
            return new ActionEventAttack()
            {
                StartTime = this.StartTime,
                EndTime = this.EndTime,

                AttackColName = this.AttackColName,
                attackData = attackData.Clone(),
            };
        }

#if UNITY_EDITOR
        [NonSerialized]
        private GameObject _selectedObject = null;
        [NonSerialized]
        private string[] atkColNames;

        public override void OnGUI(ActionData actionData, int index)
        {
            base.OnGUI(actionData, index);

            if (_selectedObject == null || _selectedObject != Selection.activeGameObject)
            {
                _selectedObject = Selection.activeGameObject;
                if (_selectedObject == null)
                {
                    atkColNames = null;
                }
                else
                {
                    var atkCols = _selectedObject.GetComponentsInChildren<CharacterAttackCollider>();
                    if (atkCols == null || atkCols.Length <= 0)
                    {
                        atkColNames = null;
                    }
                    else
                    {
                        atkColNames = atkCols.Select((x) => x.name).ToArray();
                    }
                }
            }

            if (atkColNames == null)
            {
                AttackColName = EditorGUILayout.TextField("AttackCol", AttackColName);
            }
            else
            {
                var selectedIdx = Array.FindIndex(atkColNames, (name) => (name.CompareTo(AttackColName)) == 0);
                var newIdx = EditorGUILayout.Popup("AttackCol", selectedIdx, atkColNames);

                if (!atkColNames.CheckIndex(newIdx))
                    AttackColName = null;
                else
                    AttackColName = atkColNames[newIdx];
            }

            if (attackData == null)
                attackData = new AttackableObject.AttackData();

            attackData.OnGUI();
        }
#endif
    }

    public class ActionEventAttackRuntime : ActionEventRuntimeDurationBase<ActionEventAttack>
    {
        private CharacterAttackCollider atkCol = null;

        public AttackableObject.AttackRuntime atkRuntime = null;

        public ActionEventAttackRuntime(CharacterObject owner, ActionRuntime actionDataRuntime, ActionEventAttack data) : base(owner, actionDataRuntime, data)
        {
        }

        public override void Init()
        {
            if (_eventData.attackData == null || string.IsNullOrWhiteSpace(_eventData.AttackColName))
                return;

            atkCol = _owner.GetCharAtkCol(_eventData.AttackColName);

#if UNITY_EDITOR
            if (atkCol == null)
                Debug.LogError($"ActionEventAttackRuntime->Init :: Not Found AttackCol \"{_eventData.AttackColName}\"");
#endif
            atkRuntime = new AttackableObject.AttackRuntime();
            atkRuntime.Init(_owner, _eventData.attackData);
        }

        public override void OnStart(float actionElapsedTime)
        {
            base.OnStart(actionElapsedTime);

            if (atkCol == null || atkRuntime == null)
                return;

            atkCol.SetCallback((hitObj, hitPos) =>
            {
                if (BattleSceneControllerBase.Instance == null)
                    return;

                BattleSceneControllerBase.Instance.ExcuteHitEffect(
                    this._owner, 
                    hitObj, 
                    this.atkRuntime,
                    atkRuntime.Data.damageWeight / (float)this._actionDataRuntime.Data.eventData.TotalDamageWeight,
                    hitPos);
            });

            atkCol.gameObject.SetActive(true);
        }

        public override void OnEnd()
        {
            base.OnEnd();

            if (atkCol != null)
                atkCol.gameObject.SetActive(false);
        }

        public override void OnFinalize()
        {
            base.OnFinalize();

            if (atkRuntime != null)
            {
                atkRuntime.OnFinalize();
                atkRuntime = null;
            }
        }
    }
}