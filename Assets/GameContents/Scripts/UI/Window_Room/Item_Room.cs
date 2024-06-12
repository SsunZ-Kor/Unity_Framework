using BubbleFighter.Network.Protocol;
using GameAnvil;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class Item_Room : MonoBehaviour
    {
        [SerializeField]
        private Text _uiTxt_NickName = null;

        [SerializeField]
        private GameObject _root_Empty = null;
        [SerializeField]
        private GameObject _root_Contents = null;

        [SerializeField]
        private GameObject _mark_Ready = null; // 준비 완료
        [SerializeField]
        private GameObject _mark_Wait = null;  // 준비 미완료
        [SerializeField]
        private GameObject _mark_Master = null;

        public ST_GameRoomUser userInfo { get; private set; }

        public void SetInfo(ST_GameRoomUser user, bool isMaster)
        {
            userInfo = user;
            if (userInfo == null)
            {
                _root_Empty.SetActive(true);
                _root_Contents.SetActive(false);
                return;
            }

            _uiTxt_NickName.text = user.UserName;

            _root_Empty.SetActive(false);
            _root_Contents.SetActive(true);

            _mark_Master.gameObject.SetActive(isMaster);
            
            if (isMaster)
            {
                _mark_Ready.gameObject.SetActive(false);
                _mark_Wait.gameObject.SetActive(false);
                return;
            }

            bool bReady = userInfo.UserState == ENUM_USER_STATE.UserStateReady;
            _mark_Ready.gameObject.SetActive(bReady);
            _mark_Wait.gameObject.SetActive(!bReady);
        }

        public void SetReady()
        {
            userInfo.UserState = ENUM_USER_STATE.UserStateReady;
            _mark_Ready.gameObject.SetActive(true);
            _mark_Wait.gameObject.SetActive(false);
        }
    }
}
