using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game.Character.Action
{
    [System.Serializable]
    public class ActionEventAnimator : ActionEventBase
    {
        public override bool IgnoreOnNetCharCtrl => false;

        public override bool isStartOnSyncTime => true;

        public string StateName;
        public float FadeTime = 1f;

        public override IActionEventRuntime CreateRuntime(CharacterObject owner, ActionRuntime actionDataRuntime)
        {
            return new ActionEventAnimatorRuntime(owner, actionDataRuntime, this);
        }

        public override void GetAssetBundlePackageNames(ref HashSet<string> set_BundlePackageName)
        {
        }

        public override ActionEventBase Clone()
        {
            return new ActionEventAnimator()
            {
                StartTime = this.StartTime,

                StateName = (string)this.StateName.Clone(),
                FadeTime = FadeTime,
            };
        }

#if UNITY_EDITOR
        public override void OnGUI(ActionData actionData, int index)
        {
            base.OnGUI(actionData, index);

            StateName = EditorGUILayout.TextField("StateName", StateName);
            FadeTime = EditorGUILayout.FloatField("FadeTime", FadeTime);
        }
#endif
    }

    public class ActionEventAnimatorRuntime : ActionEventRuntimeBase<ActionEventAnimator>
    {
        public ActionEventAnimatorRuntime(CharacterObject owner, ActionRuntime actionDataRuntime, ActionEventAnimator data) : base(owner, actionDataRuntime, data)
        {
        }

        public override void Init()
        {
        }

        public override void OnStart(float actionElapsedTime)
        {
            if (string.IsNullOrWhiteSpace(_eventData.StateName))
                return;

            var elapsedTime = actionElapsedTime - _eventData.StartTime;
            var stateInfo = _owner.AnimCtrl.GetCurrentAnimatorStateInfo(0);
            
            if (_owner.ActionCtrl.ActionRuntimeShared.LastAnimStateName == _eventData.StateName)
            {
                if (!stateInfo.loop)
                {
                    _owner.ActionCtrl.ActionRuntimeShared.LastAnimStateName = _eventData.StateName;
                    _owner.AnimCtrl.Play(_eventData.StateName, -1, 0f);
                }
            }
            else
            {
                _owner.ActionCtrl.ActionRuntimeShared.LastAnimStateName = _eventData.StateName;
                _owner.AnimCtrl.CrossFadeInFixedTime(_eventData.StateName, _eventData.FadeTime, -1, elapsedTime);
            }
        }
    }
}