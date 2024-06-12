using Cinemachine;
using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game
{
    public class World : MonoBehaviour
    {
        [System.Serializable]
        public class SponeInfo
        {
            [SerializeField]
            private Transform[] tr_Spones = null;

            public Transform GetSponePoint(int index)
            {
                if (tr_Spones == null || !tr_Spones.CheckIndex(index))
                    return null;

                return tr_Spones[index];
            }
        }


        // 중력
        [SerializeField]
        private SponeInfo[] sponeInfos = null;
        [SerializeField]
        private Vector3 _baseGravity = new Vector3(0f, -30f, 0f);
        public Vector3 BaseGravity => _baseGravity;
        private GravityFeildBase[] _gravityFeilds;

        // 카메라
        [SerializeField]
        private CinemachineVirtualCamera _startVCam; // Null일 경우 TPS 카메라
        [SerializeField]
        private List<GameCamera.VCamInfo> _list_VCamInfo;

        public CinemachineVirtualCamera StartVCam => _startVCam;
        public List<GameCamera.VCamInfo> List_VCamInfo => _list_VCamInfo;

        private void Awake()
        {
            // 중력장 초기화
            _gravityFeilds = this.GetComponentsInChildren<GravityFeildBase>(true);
            for (int i = 0; i < _gravityFeilds.Length; ++i)
            {
                _gravityFeilds[i].Init(this, i);
            }
        }

        public Transform GetSponeInfo(int teamNo, int index)
        {
            var teamIdx = teamNo - 1;
            if (sponeInfos == null || !sponeInfos.CheckIndex(teamIdx))
                return null;

            var sponeInfo = sponeInfos[teamIdx];
            if (sponeInfo == null)
                return null;

            return sponeInfo.GetSponePoint(index);
        }

        public GravityFeildBase GetGravityFeild(int idx)
        {
            if (_gravityFeilds == null || !_gravityFeilds.CheckIndex(idx))
                return null;

            return _gravityFeilds[idx];
        }
    }
}
