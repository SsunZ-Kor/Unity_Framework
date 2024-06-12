using Cinemachine;
using Game;
using Game.Character;
using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Game
{
    public class GameCameraEnter : MonoBehaviour
    {
        public GameCamera.VCamInfo VCamInfo { get; private set; }

        public virtual void Init(GameCamera.VCamInfo vcamInfo)
        {
            VCamInfo = vcamInfo;
            VCamInfo.VCam.Priority = 0;
        }

        private void OnTriggerEnter(Collider other)
        {
            var charObj = other.GetComponent<CharacterObject>();
            if (charObj == null)
                return;

            // 이전 카메라 비활성화
            var currVCam = BattleSceneControllerBase.Instance.GameCam.CrrVCamInfo;
            if (currVCam != null)
            {
                currVCam.VCam.Priority = 0;
                currVCam.SetActive_Enter(true);

                BattleSceneControllerBase.Instance.GameCam.CrrVCamInfo = null;
            }

            // 현재 카메라 활성화
            if (VCamInfo.VCam != null)
            {
                VCamInfo.VCam.LookAt = charObj.Tr_CamPos;
                VCamInfo.VCam.Follow = charObj.Tr_CamPos;

                VCamInfo.VCam.Priority = 1;
                currVCam.SetActive_Enter(false);

                BattleSceneControllerBase.Instance.GameCam.CrrVCamInfo = VCamInfo;
            }
        }
    }
}