using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game
{
    public partial class GameCamera
    {
        [System.Serializable]
        public class VCamInfo
        {
            [SerializeField]
            public CinemachineVirtualCamera VCam;
            [SerializeField]
            public GameCameraEnter[] Enters;

            public void Init()
            {
                if (Enters != null)
                {
                    for (int j = 0; j < Enters.Length; ++j)
                    {
                        var enter = Enters[j];
                        if (enter == null)
                            continue;

                        enter.Init(this);
                    }
                }
            }

            public void SetActive_Enter(bool bActive)
            {
                if (Enters == null)
                    return;

                for (int i = 0; i < Enters.Length; ++i)
                {
                    if (Enters[i] == null)
                        continue;

                    Enters[i].gameObject.SetActive(bActive);
                }
            }
        }
        
        [System.Serializable]
        public class CamMoveInfo
        {
            public Vector3 AddOffset;
            public float ReachTime;
            public float StayTime;
            public float ComebackTime;
        }
    }
}
