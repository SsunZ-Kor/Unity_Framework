using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class Loading_AnimOpenAndClose : LoadingBase
    {
        [SerializeField]
        private AnimationClip _OpenAnimClip = null;
        [SerializeField]
        private AnimationClip _CloseAnimClip = null;

        private void Awake()
        {
            _anim.SafeAddAnimClip(_OpenAnimClip);
            _anim.SafeAddAnimClip(_CloseAnimClip);
        }

        public override void PlayForward(Action endCallback)
        {
            StopAllCoroutines();

            if (_OpenAnimClip == null)
            {
                endCallback?.Invoke();
                return;
            }

            this.gameObject.SetActive(true);
            _anim.Play(_OpenAnimClip.name);

            StartCoroutine(Check_PlayEnd(endCallback));
        }

        public override void PlayBack(Action endCallback)
        {
            StopAllCoroutines();
            if (_CloseAnimClip == null)
            {
                endCallback?.Invoke();
                return;
            }

            _anim.Play(_CloseAnimClip.name);

            if (endCallback == null)
                endCallback = () => this.gameObject.SetActive(false);
            else
                endCallback += () => this.gameObject.SetActive(false);

            StartCoroutine(Check_PlayEnd(endCallback));
        }
    }
}
