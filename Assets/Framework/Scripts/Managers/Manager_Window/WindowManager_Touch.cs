using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public partial class WindowManager
    {
        public bool bTouchEffectEnable = true;

        private string _name_TouchEffect = null;

        private void Awake_Touch()
        {
            var prf_TouchEffect = Resources.Load<GameObject>("System/touch_click");

            if (prf_TouchEffect != null)
            {
                _name_TouchEffect = prf_TouchEffect.name;
                Managers.FX.RegistFX(prf_TouchEffect);
            }
        }

        private void OnDestroy_Touch()
        {
            if (Managers.IsValid && Managers.FX != null)
                Managers.FX.RemoveFX(_name_TouchEffect);
        }

        private void Update_Touch()
        {
            if (!Managers.IsValid || Managers.FX == null)
                return;

#if UNITY_EDITOR
            if (bTouchEffectEnable && Input.GetMouseButtonDown(0))
            {
                var canvasPos = ConvertScreenToCanvasPoint(Input.mousePosition);

                Managers.FX.PlayFX(
                    _name_TouchEffect,
                    canvasPos,
                    Quaternion.identity,
                    Vector3.one,
                    _rttr_CanvasRoot,
                    1f,
                    0f);
            }

#else
            if (bTouchEffectEnable)
            {
                if (Input.touches != null && Input.touches.Length > 0)
                {
                    for (int i = 0; i < Input.touches.Length; ++i)
                    {
                        if (Input.touches[i].phase == TouchPhase.Began)
                        {
                            var canvasPos = ConvertScreenToCanvasPoint(Input.touches[i].position);

                            Managers.FX.PlayFX(
                                _name_TouchEffect,
                                canvasPos,
                                Quaternion.identity,
                                Vector3.one,
                                _rttr_CanvasRoot,
                                1f,
                                0f);
                        }
                    }
                }
            }
#endif
        }

        public void SetActiveTouchEffect(bool bActive)
        {
            bTouchEffectEnable = bActive;
        }
    }
}