using Game;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

namespace Game
{
    public partial class GameCamera : MonoBehaviour
    {
        [Header("Default Components")]
        [SerializeField]
        private CinemachineBrain _cm_Brain = null;

        [SerializeField]
        private Camera cam = null;

        [NonSerialized]
        public VCamInfo CrrVCamInfo = null;

        [SerializeField]
        private Transform _tr_Body = null;
        [SerializeField]
        private Transform _tr_Neck = null;
        [SerializeField]
        private Transform _tr_Head = null;

        public CinemachineVirtualCamera CrrVCam => _cm_Brain.ActiveVirtualCamera as CinemachineVirtualCamera;
        public Vector3 CamForward => CrrVCam.transform.forward;
        public Quaternion CamRotation => CrrVCam.transform.rotation;

        [NonSerialized]
        public Character.CharacterObject TargetChar = null;

        private Vector3 CamOffset
        {
            get
            {
                var vResult = _tr_Neck.localPosition;
                vResult.z = _tr_Head.localPosition.z;

                return vResult;
            }
            set
            {
                _tr_Neck.localPosition = new Vector3(value.x, value.y);
                _tr_Head.localPosition = new Vector3(0f, 0f, value.z);
            }
        }

        public void Start()
        {
            this.cam.transform.SetParent(null);
        }

        private void Update()
        {
            Update_TPS();
        }

        private void LateUpdate()
        {
            LateUpdate_TPS();
        }

        private void OnDestroy()
        {
            if (this.cam != null && this.cam.transform.parent != this.transform)
                GameObject.Destroy(this.cam.gameObject);
        }

        public void Init(CinemachineVirtualCamera _start_VCam, List<VCamInfo> _list_VCamInfo)
        {
            // 카메라 초기화
            if (_start_VCam == null)
                _start_VCam = this.VCam;

            bool bNotFoundStartCam = true;

            if (_list_VCamInfo != null)
            {
                for (int i = 0; i < _list_VCamInfo.Count; ++i)
                {
                    var vcamInfo = _list_VCamInfo[i];
                    if (vcamInfo == null)
                        continue;

                    if (vcamInfo.VCam == null)
                        vcamInfo.VCam = this.VCam;

                    vcamInfo.Init();

                    if (vcamInfo.VCam == _start_VCam)
                    {
                        bNotFoundStartCam = false;
                        vcamInfo.VCam.Priority = 1;
                        vcamInfo.SetActive_Enter(false);
                        CrrVCamInfo = vcamInfo;
                    }
                    else
                    {
                        vcamInfo.VCam.Priority = 0;
                        vcamInfo.SetActive_Enter(true);
                    }
                }
            }

            if (bNotFoundStartCam)
            {
                var newVCamInfo = new VCamInfo();
                newVCamInfo.VCam = this.VCam;
                newVCamInfo.VCam.Priority = 1;
                
                CrrVCamInfo = newVCamInfo;
            }
        }
        
        public void SetTarget(Character.CharacterObject target, int layerMask_OnAim, float fDist_OnAim)
        {
            TargetChar = target;
            _cm_Brain.m_WorldUpOverride = target.transform;

            _aimLayerMask = layerMask_OnAim;
            _aimDist = fDist_OnAim;
        }
    }
}
