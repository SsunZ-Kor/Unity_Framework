using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Cinemachine;

namespace Game
{
    public partial class GameCamera
    {
        [Header("TPS Components")]
        [SerializeField]
        private CinemachineVirtualCamera _cm_VCam = null;
        [SerializeField]
        private CinemachineCameraOffset _cm_CamOffset = null;

        public CinemachineVirtualCamera VCam => _cm_VCam;

        // Aim
        [NonSerialized]
        public bool UseUpdateAim = true;

        private LayerMask _aimLayerMask;
        public float _aimDist { get; private set; }
        private RaycastHit _aimHitInfo;
        private float fNeckDegreeX = 0f;

        public bool HasAimHitInfo => UseUpdateAim && _aimHitInfo.collider != null;

        public Vector3 AimPoint
        {
            get
            {
                if (HasAimHitInfo)
                    return _aimHitInfo.point;

                return _tr_Neck.position + _tr_Neck.forward * 1000f;
            }
        }

        public Vector3 AimDir => _tr_Neck.forward;

        private void Update_TPS()
        {
            if (UseUpdateAim)
                Physics.Raycast(_tr_Neck.position, _tr_Neck.forward, out _aimHitInfo, _aimDist, _aimLayerMask, QueryTriggerInteraction.Ignore);
        }

        private void LateUpdate_TPS()
        {
            if (TargetChar == null)
                return;

            // Pos Update
            if (_tr_Body.position != TargetChar.CamPos)
            {
                _tr_Body.position = TargetChar.CamPos;
            }

            // Rot Update
            if (TargetChar.WorldUp != _tr_Body.up)
            {
                var vLookRight = Vector3.Cross(_tr_Body.forward, TargetChar.WorldUp);
                var vLookFoward = Vector3.Cross(TargetChar.WorldUp, vLookRight);
                _tr_Body.rotation = Quaternion.LookRotation(vLookFoward, TargetChar.WorldUp);
            }
        }

        public void AddRotationH(float addDegrees)
        {
            var vLookRight = Vector3.Cross(_tr_Body.forward, TargetChar.WorldUp);
            var vLookFoward = Vector3.Cross(TargetChar.WorldUp, vLookRight);
            var newRot = Quaternion.LookRotation(vLookFoward, TargetChar.WorldUp);
            newRot *= Quaternion.Euler(0f, addDegrees, 0f);

            _tr_Body.rotation = newRot;
        }

        public void AddRotationV(float addDegrees)
        {
            fNeckDegreeX = Mathf.Clamp(fNeckDegreeX + addDegrees, -70f, 70f);
            _tr_Neck.localRotation = Quaternion.Euler(fNeckDegreeX, 0f, 0f);
        }

        public void MoveOffsetEffect(CamMoveInfo camMoveInfo, bool bResetPos)
        {
            StopAllCoroutines();
            StartCoroutine(Cor_MoveOffsetEffect(camMoveInfo, bResetPos));
        }

        public IEnumerator Cor_MoveOffsetEffect(CamMoveInfo camMoveInfo, bool bResetPos)
        {
            if (bResetPos)
                _cm_CamOffset.m_Offset = Vector3.zero;

            // offset 위치로 이동
            var originOffset = _cm_CamOffset.m_Offset;
            var elaspedtime = 0f;

            while(elaspedtime < camMoveInfo.ReachTime)
            {
                yield return null;

                _cm_CamOffset.m_Offset = Vector3.Lerp(originOffset, camMoveInfo.AddOffset, elaspedtime / camMoveInfo.ReachTime);
                elaspedtime += Time.deltaTime;
            }

            _cm_CamOffset.m_Offset = camMoveInfo.AddOffset;

            // 대기
            yield return new WaitForSeconds(camMoveInfo.StayTime);

            originOffset = _cm_CamOffset.m_Offset;
            elaspedtime = 0f;

            // 컴백 여부 체크
            if (camMoveInfo.ComebackTime < 0f)
                yield break;

            // 원 위치로 복귀
            while (elaspedtime < camMoveInfo.ComebackTime)
            {
                yield return null;

                _cm_CamOffset.m_Offset = Vector3.Lerp(originOffset, Vector3.zero, elaspedtime / camMoveInfo.ComebackTime);
                elaspedtime += Time.deltaTime;
            }

            _cm_CamOffset.m_Offset = Vector3.zero;
        }
    }
}