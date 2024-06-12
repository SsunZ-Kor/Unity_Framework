using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public partial class WindowManager
    {
        private readonly Dictionary<LoadingID, LoadingBase> _dic_LoadingInsts = new Dictionary<LoadingID, LoadingBase>();
        private LoadingBase _currShowLoadingUI = null;

        public bool IsLoading { get { return _currShowLoadingUI != null; } }

        private LoadingBase GetLoading(LoadingID loadingId)
        {
            LoadingBase result = null;
            if (_dic_LoadingInsts.TryGetValue(loadingId, out result))
            {
                if (result == null)
                    _dic_LoadingInsts.Remove(loadingId);
            }

            if (result == null)
            {
                var prf_Loading = Resources.Load($"UI/Prefab_Loading/{loadingId}");
                if (prf_Loading == null)
                    throw new System.Exception(string.Format("로딩 프리펩을 찾을 수 없습니다 : {0}", loadingId));

                var go_Loading = GameObject.Instantiate(prf_Loading, this._rttr_holderForLoading) as GameObject;

                result = go_Loading.GetComponent(typeof(LoadingBase)) as LoadingBase;
                this._dic_LoadingInsts.Add(loadingId, result);
            }

            var rectTransform = result.transform as RectTransform;
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.localScale = Vector3.one;
            rectTransform.localRotation = Quaternion.identity;

            return result;
        }

        public void ShowLoading(LoadingID loadingId, System.Action endCallback)
        {
            if (IsLoading)
                throw new System.Exception("로딩 중에 로딩이 호출되었습니다.");

            _currShowLoadingUI = GetLoading(loadingId);
            _currShowLoadingUI.PlayForward(endCallback);
        }

        public void OutLoading(System.Action endCallback)
        {
            if (!IsLoading)
            {
                endCallback?.Invoke();
                return;
            }

            var disableLoadingUI = _currShowLoadingUI;
            _currShowLoadingUI = null;

            disableLoadingUI.PlayBack(endCallback);
        }
    }
}