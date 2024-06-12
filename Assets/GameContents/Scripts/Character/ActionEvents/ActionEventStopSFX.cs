using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game.Character.Action
{
    [System.Serializable]
    public class ActionEventStopSFX : ActionEventBase
    {
        public override bool IgnoreOnNetCharCtrl => false;

        public string SfxUniqueKey;

        public override IActionEventRuntime CreateRuntime(CharacterObject owner, ActionRuntime actionDataRuntime)
        {
            return new ActionEventStopSFXRuntime(owner, actionDataRuntime, this);
        }

        public override void GetAssetBundlePackageNames(ref HashSet<string> set_BundlePackageName)
        {
        }

        public override ActionEventBase Clone()
        {
            return new ActionEventStopSFX()
            {
                StartTime = this.StartTime,

                SfxUniqueKey = (string)this.SfxUniqueKey.Clone(),
            };
        }

#if UNITY_EDITOR
        public override void OnGUI(ActionData actionData, int index)
        {
            base.OnGUI(actionData, index);

            SfxUniqueKey = EditorGUILayout.TextField("Sfx Unique Key", SfxUniqueKey);
        }
#endif
    }

    public class ActionEventStopSFXRuntime : ActionEventRuntimeBase<ActionEventStopSFX>
    {
        public ActionEventStopSFXRuntime(CharacterObject owner, ActionRuntime actionDataRuntime, ActionEventStopSFX data) : base(owner, actionDataRuntime, data)
        {

        }

        public override void Init()
        {
        }

        public override void OnStart(float actionElapsedTime)
        {
            base.OnStart(actionElapsedTime);

            if (string.IsNullOrWhiteSpace(_eventData.SfxUniqueKey))
            {
                _owner.RemoveAllSfxObj();
            }
            else
            {
                _owner.RemoveSFXObj(_eventData.SfxUniqueKey);
            }
        }
    }
}
