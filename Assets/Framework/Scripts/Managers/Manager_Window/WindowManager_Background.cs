using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Game
{
    public partial class WindowManager
    {
        public enum LobbyCharMode
        {
            None,
            Lobby,
            FullScreen,
        }

        private IEnumerator _changeCoroutine = null;

        // Background :: Root, 카메라
        public Transform _root_Bg;
        public Camera BGCam { get; private set; }
        // Background :: 인스턴스들
        private Dictionary<BackgroundID, BackgroundBase> _dic_Bg = new Dictionary<BackgroundID, BackgroundBase>();
        // Background :: 현재 FadeOut 중인 인스턴스
        private Dictionary<BackgroundID, BackgroundBase> _dic_PrevBg = new Dictionary<BackgroundID, BackgroundBase>();

        // 현재 Background 정보
        private BackgroundID _currBgId = BackgroundID.BG_None;
        private BackgroundBase _currBg = null;
        public BackgroundBase CurrBg => _currBg;

        private void Init_Background()
        {
            var prf_bgRoot = Resources.Load<GameObject>("System/BgRoot");
            var go_bgRoot = GameObject.Instantiate(prf_bgRoot, null);
            go_bgRoot.name = prf_bgRoot.name;

            _root_Bg = go_bgRoot.transform;
            _root_Bg.Reset();
            DontDestroyOnLoad(_root_Bg);

            // 카메라 세팅
            BGCam = go_bgRoot.GetComponentInChildren<Camera>();
            var camData = BGCam.GetUniversalAdditionalCameraData();
            camData.cameraStack.Add(this._CanvasRoot.worldCamera);
            camData.renderType = CameraRenderType.Base;

            if (SystemInfo.supportsGyroscope)
                Input.gyro.enabled = true;
        }

        private void OnDisable_Background()
        {
            // ManagerBase->OnDisable에서 코루틴 종료함
            // 실행중 루틴만 Null 초기화
            _changeCoroutine = null;
        }
        
        private void OnDestroy_Background()
        {
            if (_root_Bg != null)
                GameObject.DestroyImmediate(_root_Bg.gameObject);
        }

        public BackgroundBase GetBackgroundInst(BackgroundID bgId)
        {
            return _dic_Bg.GetOrNull(bgId);
        }

        public void ChangeBackground(BackgroundID bgId, float crossTime, System.Action endCallback)
        {
            BGCam.enabled = Camera.main == null || _currBgId != BackgroundID.BG_None;
            UpdateCamStack();

            // 현재와 같다면 호출스택 종료
            if (_currBgId == bgId)
            {
                endCallback?.Invoke();
                return;
            }

            // 기존 Cross 코루틴 종료
            if (_changeCoroutine != null)
                StopCoroutine(_changeCoroutine);

            // 기존 배경을 이전 배경 dic에 추가
            if (_currBgId != BackgroundID.BG_None)
            {
                _currBg.transform.localPosition = Vector3.forward * 300f;
                _dic_PrevBg.Add(_currBgId, _currBg);
            }

            // 현재 배경 ID 갱신 후 인스턴스 찾아오기
            _currBgId = bgId;
            _currBg = null;
            
            // Cross 코루틴 실행
            _changeCoroutine = Cor_ChangeBackground(crossTime, endCallback);
            StartCoroutine(_changeCoroutine);
        }

        public void UpdateCamStack()
        {
            if (Camera.allCamerasCount == 0)
                return;

            var allCams = Camera.allCameras;
            Array.Sort(allCams, (x, y) => y.depth.CompareTo(y.depth));

            var baseCamData = allCams[0].GetUniversalAdditionalCameraData();
            baseCamData.renderType = CameraRenderType.Base;
            baseCamData.cameraStack.Clear();

            for (int i = 1; i < allCams.Length; ++i)
            {
                var camData = allCams[i].GetUniversalAdditionalCameraData();
                camData.renderType = CameraRenderType.Overlay;
                baseCamData.cameraStack.Add(allCams[i]);
            }
        }

        private IEnumerator Cor_ChangeBackground(float crossTime, System.Action endCallback)
        {
            if (_currBgId != BackgroundID.BG_None)
            {
                BGCam.enabled = true;

                // FadeOut중인 인스턴스 중에 있는지 찾아본다.
                if (_dic_PrevBg.TryGetValue(_currBgId, out _currBg))
                {
                    // 있다면 제거
                    _dic_PrevBg.Remove(_currBgId);
                }
                else
                {
                    // 없다면 인스턴스 상에 있는지 체크해본다.
                    if (!_dic_Bg.TryGetValue(_currBgId, out _currBg))
                    {
                        // 인스턴스도 없다면 새로 만들어준다.
                        GameObject prf_Bg = Resources.Load<GameObject>($"UI/Prefab_Background/{_currBgId}");
                        if (prf_Bg == null)
                        {
                            _currBgId = BackgroundID.BG_None;
                            _currBg = null;
                        }
                        else
                        {
                            var go_Bg = GameObject.Instantiate(prf_Bg, _root_Bg) as GameObject;
                            _currBg = go_Bg.GetOrAddComponent<BackgroundBase>();
                            _currBg.name = prf_Bg.name;

                            go_Bg.transform.Reset();

                            // 인스턴스 dic에 추가
                            _dic_Bg.Add(_currBgId, _currBg);
                        }
                    }

                    // alpha 재조정
                    if (_currBg != null)
                    {
                        _currBg.gameObject.SetActive(true);
                        _currBg.transform.localPosition = Vector3.forward * 100f;
                        _currBg.Alpha = 0f;
                    }
                }
            }

            var prevBgs = _dic_PrevBg.Values;
            if (_currBg != null)
                Managers.UI.BGCam.orthographicSize = _currBg.CamSize;

            if (crossTime > 0)
            {

                var startTime = Time.realtimeSinceStartup;
                var startCamSize = Managers.UI.BGCam.orthographicSize;
                while (true)
                {
                    var factor = (Time.realtimeSinceStartup - startTime) / crossTime;
                    if (factor >= 1f)
                        break;

                    // 이전 UI fadeOut
                    if (prevBgs != null && prevBgs.Count > 0)
                    {
                        foreach (var prevBg in prevBgs)
                        {
                            var inverseFactor = 1f - factor;
                            if (prevBg.Alpha >= inverseFactor)
                                prevBg.Alpha = Mathf.Clamp01(inverseFactor);
                        }
                    }

                    // 현재 UI fadeIn
                    if (_currBg != null)
                    {
                        if (_currBg.Alpha <= factor)
                            _currBg.Alpha = Mathf.Clamp01(factor);

                        Managers.UI.BGCam.orthographicSize = Mathf.Lerp(startCamSize, _currBg.CamSize, factor);
                    }

                    yield return null;
                }
            }

            if (_currBg != null)
                _currBg.Alpha = 1f;

            // 기존 UI들 리스트에서 삭제
            if (prevBgs != null && prevBgs.Count > 0)
            {
                foreach (var prevBg in prevBgs)
                {
                    prevBg.Alpha = 0f;
                    prevBg.gameObject.SetActive(false);
                }
            }

            _dic_PrevBg.Clear();
            _changeCoroutine = null;

            if (_currBgId == BackgroundID.BG_None && BGCam.clearFlags == CameraClearFlags.Depth)
                BGCam.enabled = false;

            endCallback?.Invoke();
        }

        private void OnClear_Background(bool bDestroy)
        {
            // 기존 Cross 코루틴 종료
            if (_changeCoroutine != null)
            {
                StopCoroutine(_changeCoroutine);
                _changeCoroutine = null;
            }

            // 배경 Object 정리
            var bgs = _dic_Bg.Values;
            if (bDestroy)
            {
                foreach (var bg in bgs)
                    GameObject.Destroy(bg.gameObject);

                _dic_Bg.Clear();
            }
            else
            {
                foreach (var bg in bgs)
                    bg.gameObject.SetActive(false);
            }

            // 변수 초기화
            _dic_PrevBg.Clear();
            _currBgId = BackgroundID.BG_None;
            _currBg = null;
            BGCam.enabled = false;
        }
    }
}