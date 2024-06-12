using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class LoadingBase : MonoBehaviour
    {
        [SerializeField]
        protected Animation _anim;

        protected virtual void OnEnable() { }
        protected virtual void OnDisable() { }

        public virtual void PlayForward(System.Action endCallback)
        {
            this.gameObject.SetActive(true);

            if (_anim != null && _anim.clip != null)
            {
                AnimationState animState = _anim[_anim.clip.name];
                animState.time = 0;
                animState.speed = 1f;

                _anim.Play();
            }

            StopAllCoroutines();
            StartCoroutine(Check_PlayEnd(endCallback));
        }

        public virtual void PlayBack(System.Action endCallback)
        {
            if (_anim != null && _anim.clip != null)
            {
                AnimationState animState = _anim[_anim.clip.name];
                animState.time = animState.length;
                animState.speed = -1f;

                _anim.Play();
            }

            StopAllCoroutines();

            if (endCallback == null)
                endCallback = () => this.gameObject.SetActive(false);
            else
                endCallback += () => this.gameObject.SetActive(false);

            StartCoroutine(Check_PlayEnd(endCallback));
        }

        protected virtual IEnumerator Check_PlayEnd(System.Action endCallback)
        {
            while (_anim != null && _anim.isPlaying)
                yield return null;

            if (endCallback != null)
                endCallback.Invoke();
        }

        public void SetActive(bool bActive)
        {
            throw new System.NotImplementedException();
        }
    }
}
