using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class Loading_ScreenShotFadeOut : LoadingBase {

        [SerializeField]
        protected RawImage _img_ScreenShot;

        protected override void OnEnable()
        {
            base.OnEnable();

            // 텍스쳐 생성
            _img_ScreenShot.texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            //텍스쳐 해제
            Destroy(_img_ScreenShot.texture);
            _img_ScreenShot.texture = null;
        }

        public override void PlayForward(System.Action endCallback)
        {
            // 스크린 샷에 방해 되지 않도록 알파를 0으로 만듦
            SetScreenShotAlpha(0f);
            this.gameObject.SetActive(true);
            
            // 애니메이션을 초기화 하되, Play는 하지 않는다.
            if (_anim != null && _anim.clip != null)
            {
                AnimationState animState = _anim[_anim.clip.name];
                animState.time = 0;
            }
            StopAllCoroutines();

            // 스크린 샷 코루틴 시작
            StartCoroutine( Cor_ScreenShot(endCallback));
        }
        
        private IEnumerator Cor_ScreenShot(System.Action endCallback)
        {
            // 프레임 종료 대기
            yield return new WaitForEndOfFrame();

            // 스크린샷
            var tex = _img_ScreenShot.texture as Texture2D;
            tex.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            tex.Apply();

            // 바로 보여준다.
            SetScreenShotAlpha(1f);

            // 콜백 처리
            endCallback?.Invoke();
        }

        private void SetScreenShotAlpha(float alpha)
        {
            var color = _img_ScreenShot.color;
            color.a = alpha;
            _img_ScreenShot.color = color;
        }
    }
}