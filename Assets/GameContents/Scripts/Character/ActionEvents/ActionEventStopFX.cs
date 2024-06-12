using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game.Character.Action
{
    [System.Serializable]
    public class ActionEventStopFX : ActionEventBase
    {
        public override bool IgnoreOnNetCharCtrl => false;

        public string FxUniqueKey;

        public override IActionEventRuntime CreateRuntime(CharacterObject owner, ActionRuntime actionDataRuntime)
        {
            return new ActionEventStopFXRuntime(owner, actionDataRuntime, this);
        }

        public override void GetAssetBundlePackageNames(ref HashSet<string> set_BundlePackageName)
        {
        }
        
        public override ActionEventBase Clone()
        {
            return new ActionEventStopFX()
            {
                StartTime = this.StartTime,

                FxUniqueKey  = (string)this.FxUniqueKey.Clone(),
            };
        }

#if UNITY_EDITOR
        public override void OnGUI(ActionData actionData, int index)
        {
            base.OnGUI(actionData, index);

            FxUniqueKey = EditorGUILayout.TextField("Fx Unique Key", FxUniqueKey);
        }
#endif
    }

    public class ActionEventStopFXRuntime : ActionEventRuntimeBase<ActionEventStopFX>
    {
        public ActionEventStopFXRuntime(CharacterObject owner, ActionRuntime actionDataRuntime, ActionEventStopFX data) : base(owner, actionDataRuntime, data)
        {

        }

        public override void Init()
        {
        }

        public override void OnStart(float actionElapsedTime)
        {
            base.OnStart(actionElapsedTime);

            if (string.IsNullOrWhiteSpace(_eventData.FxUniqueKey))
            {
                _owner.RemoveAllFxObj();
            }
            else
            {
                _owner.RemoveFXObj(_eventData.FxUniqueKey);
            }
        }
    }
}