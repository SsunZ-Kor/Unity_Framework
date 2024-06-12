using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(AudioSource))]
    public class SFXObject : PoolingObject
    {
        [SerializeField]
        protected AudioSource _audio;
        public AudioSource AudioSource { get { return _audio; } }
        public System.Action<SFXObject> _endCallback = null;
        public string AudioClipName
        {
            get
            {
                if (_audio == null || _audio.clip == null)
                    return null;

                return _audio.clip.name;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            _audio = this.gameObject.GetComponent(typeof(AudioSource)) as AudioSource;
            _audio.spatialBlend = 1f;
        }

        protected override void LateUpdate()
        {
            if (!_audio.isPlaying)
                ReturnToPoolForce();
            base.LateUpdate();
        }

        public override void ReturnToPoolForce()
        {
            if (_audio != null)
            {
                _audio.Stop();
                _audio.clip = null;
            }

            base.ReturnToPoolForce();
            if (_endCallback != null)
            {
                var call = _endCallback;
                _endCallback = null;
                call.Invoke(this);
            }
        }

        public override bool CheckWaitForEnd()
        {
            // 회수 요청이 오면 뒤도 안보고 풀로 보내버린다.
            return true;
        }

        public void PlaySFX(AudioClip clip, SFXType sfxType, bool isLoof, float playTimeOffset, System.Action<SFXObject> endCallback)
        {
            _audio.clip = clip;
            _audio.loop = isLoof;
            _audio.spatialBlend = sfxType == SFXType._2D ? 0f : 1f;
            _endCallback = endCallback;

            if (playTimeOffset == 0f)
            {
                _audio.time = 0;
                _audio.Play();
            }
            else if (playTimeOffset < 0)
            {
                _audio.PlayDelayed(playTimeOffset);
            }
            else if (playTimeOffset > 0)
            {
                playTimeOffset = Mathf.Clamp(playTimeOffset, 0f, clip.length);
                _audio.time = playTimeOffset;
                _audio.UnPause();
            }

        }
    }
}
