using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    // 로드할 씬의 넘버와 꼭 맞춰주세요
    public enum SceneID
    {
        Intro = 0,
        Lobby = 1,
        Battle = 2,
    }

    public class SceneManager : ManagerBase
    {
        public enum LoadingState
        {
            None,
            LoadingStart,
            Loading,
            LoadingEnd,
        }


        public const string STR_CONTROLLER_BASE_TAG_NAME = "SceneController";

        public SceneID CurrScene { get; private set; } = SceneID.Intro;
        public LoadingState loadingState { get; private set; } = LoadingState.None;

        public override IEnumerator Init_Async()
        {
            CurrScene = 
                (SceneID)UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;

            yield break;
        }

        public void LoadScene(SceneID sceneId, LoadingID loadingUIId, bool bClearFXObject = true)
        {
            if (loadingState != LoadingState.None)
                return;

            if (CurrScene == sceneId)
                return;

            loadingState = LoadingState.LoadingStart;
            CurrScene = sceneId;

            System.Action func_sceneLoad = () =>
            {
                // 사운드, 이펙트 매니저의 모든 아이템을 회수                
                if (bClearFXObject)
                {
                    Managers.FX.RetrieveAllItems();
                    Managers.SFX.RetrieveAllItems();
                }

                // 씬 종료 이벤트 콜
                var go_sceneController = GameObject.FindGameObjectWithTag(STR_CONTROLLER_BASE_TAG_NAME);
                go_sceneController?.SendMessage("OnEndScene", CurrScene);

                loadingState = LoadingState.Loading;
                UnityEngine.SceneManagement.SceneManager.LoadScene((int)sceneId, UnityEngine.SceneManagement.LoadSceneMode.Single);
            };

            Managers.UI.ShowLoading(loadingUIId, func_sceneLoad);
        }

        /// <summary>
        /// 각 SceneController에서 Init이 종료될 때 호출해주세요
        /// </summary>
        public void OnEndLoading()
        {
            System.Action func_sceneLoadEnd = () =>
            {
                // 씬 시작 이벤트 콜
                var go_sceneController = GameObject.FindGameObjectWithTag(STR_CONTROLLER_BASE_TAG_NAME);
                go_sceneController.SendMessage("OnStartScene");

                loadingState = LoadingState.None;
            };

            loadingState = LoadingState.LoadingEnd;
            Managers.UI.OutLoading(func_sceneLoadEnd);
        }
    }
}

