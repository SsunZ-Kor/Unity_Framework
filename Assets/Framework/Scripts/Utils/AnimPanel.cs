using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{

    [RequireComponent(typeof(Animation))]
    public class AnimPanel : MonoBehaviour
    {
        [SerializeField]
        protected Animation _anim = null;
        protected System.Action _endCallback;
        protected System.Action<string> _eventCallback;
        public bool IsPlay { get; protected set; } = false;

        public bool IsEnd => (!_anim.isPlaying) || _anim[_anim.clip.name].normalizedTime >= 1.0f;
        
        protected virtual void Awake()
        {
            if (_anim == null)
                _anim = GetComponent(typeof(Animation)) as Animation;
        }

        protected virtual void OnDisable()
        {
            _anim.Stop();
            IsPlay = false;
        }

        public virtual void Sample(float factor)
        {
            if (_anim.clip == null)
                return;

            var clipName = _anim.clip.name;

            _anim.Stop();
            _anim[clipName].enabled = true;
            _anim[clipName].time = _anim[clipName].length * factor;
            _anim[clipName].weight = 1;
            _anim.Sample();
            _anim[clipName].enabled = false;
        }

        public virtual void Play(System.Action endCallback, System.Action<string> eventCallback)
        {
            _endCallback = endCallback;
            _eventCallback = eventCallback;
            _anim.Play();

            IsPlay = true;
        }

        protected virtual void Update()
        {
            if (IsPlay && IsEnd)
            {
                IsPlay = false;
                if (_endCallback != null)
                {
                    var call = _endCallback;
                    _endCallback = null;

                    call.Invoke();
                }
            }
        }

        public virtual void OnEvent(string parameter)
        {
            if(_eventCallback != null)
            {
                _eventCallback.Invoke(parameter);
            }
        }
    }
}