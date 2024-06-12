using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Game.Character.Action
{
    [System.Serializable]
    public class ActionEventProjectile : ActionEventBase
    {
        public override bool IgnoreOnNetCharCtrl => true;

        public string TransformName;
        public bool UseAimDirection;
        public Vector3 vOffsetPos;
        public Quaternion qOffsetRot;
        public bool bFollow;
        public string ProjPrfDir;
        public string ProjPrfName;

        public AttackableObject.AttackData attackData;

        public override IActionEventRuntime CreateRuntime(CharacterObject owner, ActionRuntime actionDataRuntime)
        {
            return new ActionEventProjectileRuntime(owner, actionDataRuntime, this);
        }

        public override void GetAssetBundlePackageNames(ref HashSet<string> set_BundlePackageName)
        {
        }

        public override ActionEventBase Clone()
        {
            return new ActionEventProjectile()
            {
                StartTime = this.StartTime,

                TransformName = (string)this.TransformName.Clone(),
                UseAimDirection = this.UseAimDirection,
                vOffsetPos = this.vOffsetPos,
                qOffsetRot = this.qOffsetRot,
                bFollow = this.bFollow,
                ProjPrfDir = (string)this.ProjPrfDir.Clone(),
                ProjPrfName = (string)this.ProjPrfName.Clone(),
                attackData = this.attackData?.Clone(),
            };
        }

#if UNITY_EDITOR
        public override void OnGUI(ActionData actionData, int index)
        {
            base.OnGUI(actionData, index);

            EditorGUILayout.BeginHorizontal();
            {
                TransformName = EditorGUILayout.TextField("TransformName", TransformName);
                UseAimDirection = EditorGUILayout.Toggle("Use Aim Direction", UseAimDirection);
                vOffsetPos = EditorGUILayout.Vector3Field("offsetPos", vOffsetPos);
                qOffsetRot = Quaternion.Euler(EditorGUILayout.Vector3Field("offsetRot", qOffsetRot.eulerAngles));
                bFollow = EditorGUILayout.Toggle("isFollowTransform", bFollow);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            {
                ProjPrfDir = EditorGUILayout.TextField("ProjPrfDir", ProjPrfDir);
                ProjPrfName = EditorGUILayout.TextField("ProjPrfName", ProjPrfName);
            }
            EditorGUILayout.EndHorizontal();

            if (attackData == null)
                attackData = new AttackableObject.AttackData();

            attackData.OnGUI();
        }
#endif
    }

    public class ActionEventProjectileRuntime : ActionEventRuntimeBase<ActionEventProjectile>
    {
        private Transform tr_Parant = null;
        
        private int parentIndex = -1;
        private string projName = null;
        private int projIndex = -1;

        private AttackableObject.AttackRuntime atkRuntime = null;

        public ActionEventProjectileRuntime(CharacterObject owner, ActionRuntime actionDataRuntime, ActionEventProjectile data) : base(owner, actionDataRuntime, data)
        {

        }

        public override void Init()
        {
            if (string.IsNullOrWhiteSpace(_eventData.ProjPrfDir)
                || string.IsNullOrWhiteSpace(_eventData.ProjPrfName))
                return;

            var projPrfPath = System.IO.Path.Combine(_eventData.ProjPrfDir, _eventData.ProjPrfName);

            // 발사체 세팅
            var prf_Proj = Resources.Load<GameObject>(projPrfPath);
            if (prf_Proj == null)
            {
                Debug.Log($"{projPrfPath}을 찾을 수 없습니다.");
                return;
            }

            Managers.FX.RegistFX(prf_Proj);
            projName = prf_Proj.name;
            projIndex = _owner.RegistProj(projName);

            // 타겟 Transform 찾기
            tr_Parant = this._owner.transform.FindDeep(_eventData.TransformName);
            if (_eventData.bFollow)
            {
                if (tr_Parant == null)
                    tr_Parant = _owner.transform;

                parentIndex = _owner.RegistParent(tr_Parant);
            }

            // 공격 정보 세팅
            if (_eventData.attackData != null)
            {
                atkRuntime = new AttackableObject.AttackRuntime();
                atkRuntime.Init(_owner, _eventData.attackData);
            }
        }

        public override void OnStart(float actionElapsedTime)
        {
            base.OnStart(actionElapsedTime);

            if (projName == null)
                return;

            var projObj = Managers.FX.PlayFX(projName, _eventData.vOffsetPos, _eventData.qOffsetRot, Vector3.one, tr_Parant, 1f, 0f) as ProjectileBase;
            if (!_eventData.bFollow)
                projObj.transform.SetParent(null);

            if (_eventData.UseAimDirection)
            {
                var aimPoint = BattleSceneControllerBase.Instance.GameCam.AimPoint;
                projObj.transform.LookAt(aimPoint, projObj.transform.up);
            }

            var projUniqueKey = _owner.GetProjectileUniqueKey();
            projObj.gameObject.layer = this._owner.Layer_Proj;
            projObj.Shot(
                this._owner,
                this._owner.LayerMask_Enemy, 
                (hitObj, releasePos) =>
                {
                    // 타겟을 맞춤
                    if (atkRuntime != null && hitObj != null)
                    {
                        if (BattleSceneControllerBase.Instance != null)
                            BattleSceneController.Instance.ExcuteHitEffect(
                                this._owner, 
                                hitObj, 
                                this.atkRuntime, 
                                atkRuntime.Data.damageWeight / (float)this._actionDataRuntime.Data.eventData.TotalDamageWeight,
                                releasePos);
                    }

                    // 릴리즈를 알림
#if UNITY_EDITOR
                    if (Managers.IsValid && Managers.Net != null)
#endif
                    {
                        if (IntroSceneController.UseUDP)
                            NetProcess.UDP_Send_BattleReleaseProj(projUniqueKey, releasePos);
                        else
                            NetProcess.Send_BattleReleaseProj(projUniqueKey, releasePos);
                    }
                }
                );

            // 발사를 알림
#if UNITY_EDITOR
            if (Managers.IsValid && Managers.Net != null)
#endif
            {
                if (IntroSceneController.UseUDP)
                {
                    NetProcess.UDP_Send_BattleGenerateProj(
                        projIndex,
                        projUniqueKey,
                        projObj.transform.localPosition,
                        projObj.transform.localRotation,
                        parentIndex
                        );
                }
                else
                {
                    NetProcess.Send_BattleGenerateProj(
                        projIndex,
                        projUniqueKey,
                        projObj.transform.localPosition,
                        projObj.transform.localRotation,
                        parentIndex
                        );
                }
            }
        }

        public override void OnFinalize()
        {
            base.OnFinalize();
            
            if (Managers.IsValid)
            {
                if (projName != null)
                    Managers.FX.RemoveFX(projName);
            }

            if (atkRuntime != null)
            {
                atkRuntime.OnFinalize();
                atkRuntime = null;
            }
        }
    }
}