using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game.Character.Action
{
    [System.Serializable]
    public class ActionEventShotFX : ActionEventBase
    {
        public override bool IgnoreOnNetCharCtrl => false;

        public string TransformName;
        public Vector3 vOffsetPos;
        public Quaternion qOffsetRot;
        public bool bFollow;
        public bool isLoop;
        public string FxUniqueKey;
        public string FxPrfDir;
        public string FxPrfName;

        public override IActionEventRuntime CreateRuntime(CharacterObject owner, ActionRuntime actionDataRuntime)
        {
            return new ActionEventShotFXRuntime(owner, actionDataRuntime, this);
        }

        public override void GetAssetBundlePackageNames(ref HashSet<string> set_BundlePackageName)
        {
        }

        public override ActionEventBase Clone()
        {
            return new ActionEventShotFX()
            {
                StartTime = this.StartTime,

                TransformName = (string)this.TransformName.Clone(),
                vOffsetPos    = this.vOffsetPos,
                qOffsetRot    = this.qOffsetRot, 
                bFollow       = this.bFollow,
                isLoop = this.isLoop,
                FxUniqueKey = (string)this.FxUniqueKey.Clone(),
                FxPrfDir = (string)this.FxPrfDir.Clone(),
                FxPrfName = (string)this.FxPrfName.Clone(),
            };
        }

#if UNITY_EDITOR
        public override void OnGUI(ActionData actionData, int index)
        {
            base.OnGUI(actionData, index);

            EditorGUILayout.BeginHorizontal();
            {
                TransformName = EditorGUILayout.TextField("TransformName", TransformName);
                vOffsetPos = EditorGUILayout.Vector3Field("offsetPos", vOffsetPos);
                qOffsetRot = Quaternion.Euler(EditorGUILayout.Vector3Field("offsetRot", qOffsetRot.eulerAngles));
                bFollow = EditorGUILayout.Toggle("isFollowTransform", bFollow);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            {
                isLoop = EditorGUILayout.Toggle("isLoop", isLoop);
                EditorGUI.BeginDisabledGroup(!isLoop);
                {
                    FxUniqueKey = EditorGUILayout.TextField("Fx Unique Key", FxUniqueKey);
                }
                EditorGUI.EndDisabledGroup();
            }
            EditorGUILayout.EndHorizontal();
            FxPrfDir = EditorGUILayout.TextField("FxPrfDir", FxPrfDir);
            FxPrfName = EditorGUILayout.TextField("FxPrfName", FxPrfName);
        }
#endif
    }

    public class ActionEventShotFXRuntime : ActionEventRuntimeBase<ActionEventShotFX>
    {
        private Transform tr_Parant = null;
        private string fxName = null;

        public ActionEventShotFXRuntime(CharacterObject owner, ActionRuntime actionDataRuntime, ActionEventShotFX data) : base(owner, actionDataRuntime, data)
        {

        }
        
        public override void Init()
        {
            if (string.IsNullOrWhiteSpace(_eventData.FxPrfDir)
                || string.IsNullOrWhiteSpace(_eventData.FxPrfName))
                return;

            var fxPrfPath = System.IO.Path.Combine(_eventData.FxPrfDir, _eventData.FxPrfName);

            // 이펙트 세팅
            var prf_Fx = Resources.Load<GameObject>(fxPrfPath);
            if (prf_Fx == null)
            {
                Debug.Log($"{fxPrfPath}을 찾을 수 없습니다.");
                return;
            }

            Managers.FX.RegistFX(prf_Fx);
            fxName = prf_Fx.name;

            // 타겟 Transform 찾기
            tr_Parant = this._owner.transform.FindDeep(_eventData.TransformName);
            if (tr_Parant == null)
                tr_Parant = this._owner.transform;
        }

        public override void OnStart(float actionElapsedTime)
        {
            base.OnStart(actionElapsedTime);

            if (fxName == null)
                return;

            var fxObj = Managers.FX.PlayFX(fxName, _eventData.vOffsetPos, _eventData.qOffsetRot, Vector3.one, tr_Parant, 1f, 0f);
            if (fxObj == null)
                return;

            if (_eventData.isLoop)
                _owner.RegistFXObj(_eventData.FxUniqueKey, fxObj);
            
            if (!_eventData.bFollow)
                fxObj.transform.SetParent(null);
        }

        public override void OnFinalize()
        {
            base.OnFinalize();

            if (fxName == null)
                return;
            
            if (Managers.FX != null)
                Managers.FX.RemoveFX(fxName);
        }
    }
}