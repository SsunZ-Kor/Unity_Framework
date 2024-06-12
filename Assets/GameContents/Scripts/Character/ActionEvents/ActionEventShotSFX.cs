using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Game.Character.Action
{
    [System.Serializable]
    public class ActionEventShotSFX : ActionEventBase
    {
        public override bool IgnoreOnNetCharCtrl => false;

        public string TransformName;
        public Vector3 vOffsetPos;
        public Quaternion qOffsetRot;
        public bool bFollow;
        public bool isLoop;
        public string SfxUniqueKey;
        public string SfxAudioClipDir;
        public string SfxAudioClipName;

        public override IActionEventRuntime CreateRuntime(CharacterObject owner, ActionRuntime actionDataRuntime)
        {
            return new ActionEventShotSFXRuntime(owner, actionDataRuntime, this);
        }

        public override void GetAssetBundlePackageNames(ref HashSet<string> set_BundlePackageName)
        {
        }

        public override ActionEventBase Clone()
        {
            return new ActionEventShotSFX()
            {
                StartTime = this.StartTime,

                TransformName = (string)this.TransformName.Clone(),
                vOffsetPos = this.vOffsetPos,
                qOffsetRot = this.qOffsetRot,
                bFollow = this.bFollow,
                isLoop = this.isLoop,
                SfxUniqueKey = (string)this.SfxUniqueKey.Clone(),
                SfxAudioClipDir = (string)this.SfxAudioClipDir.Clone(),
                SfxAudioClipName = (string)this.SfxAudioClipName.Clone(),
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
                    SfxUniqueKey = EditorGUILayout.TextField("Sfx Unique Key", SfxUniqueKey);
                }
                EditorGUI.EndDisabledGroup();
            }
            EditorGUILayout.EndHorizontal();

            SfxAudioClipDir = EditorGUILayout.TextField("SfxAudioClipDir", SfxAudioClipDir);
            SfxAudioClipName = EditorGUILayout.TextField("SfxAudioClipName", SfxAudioClipName);
        }
#endif
    }

    public class ActionEventShotSFXRuntime : ActionEventRuntimeBase<ActionEventShotSFX>
    {
        private Transform tr_Parant = null;
        private AudioClip sfxClip = null;

        public ActionEventShotSFXRuntime(CharacterObject owner, ActionRuntime actionDataRuntime, ActionEventShotSFX data) : base(owner, actionDataRuntime, data)
        {

        }

        public override void Init()
        {
            if (string.IsNullOrWhiteSpace(_eventData.SfxAudioClipDir)
                || string.IsNullOrWhiteSpace(_eventData.SfxAudioClipName))
                return;

            var sfxAudioClipPath = System.IO.Path.Combine(_eventData.SfxAudioClipDir, _eventData.SfxAudioClipName);

            // 오디오 클립 세팅
            sfxClip = Resources.Load<AudioClip>(sfxAudioClipPath);
            if (sfxClip == null)
            {
                Debug.Log($"{sfxAudioClipPath}을 찾을 수 없습니다.");
                return;
            }

            // 타겟 Transform 찾기
            tr_Parant = this._owner.transform.FindDeep(_eventData.TransformName);
            if (tr_Parant == null)
                tr_Parant = this._owner.transform;
        }

        public override void OnStart(float actionElapsedTime)
        {
            base.OnStart(actionElapsedTime);

            if (sfxClip == null)
                return;

            var sfxObj = Managers.SFX.PlaySFX(
                _eventData.vOffsetPos,
                _eventData.qOffsetRot,
                Vector3.one, tr_Parant,
                sfxClip,
                SFXType._3D,
                _eventData.isLoop,
                0f);

            if (sfxObj == null)
                return;

            _owner.RegistSFXObj(_eventData.SfxUniqueKey, sfxObj);

            if (!_eventData.bFollow)
                sfxObj.transform.SetParent(null);
        }

        public override void OnFinalize()
        {
            base.OnFinalize();
        }
    }
}
