using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class FXObject : PoolingObject
    {
        [SerializeField]
        protected ParticleSystem _particle = null;
        [SerializeField]
        protected Animation _anim = null;
        [SerializeField]
        protected string _AnimName_Start = null;
        [SerializeField]
        protected string _AnimName_Loop = null;
        [SerializeField]
        protected string _AnimName_End = null;

        [SerializeField]
        protected TrailRenderer trailRenderer;

        [SerializeField]
        protected SpriteAnimation _spriteAnim = null;

        [Header("Return시 가장 먼저 없애야 할 것들")]
        [SerializeField]
        protected GameObject[] _go_Others = null;

        protected Renderer[] _renderers = null;
        protected ParticleSystem[] _particleChildren = null;

        public bool IsLoop { get; private set; } = false;

        protected override void Awake()
        {
            base.Awake();

            // 파티클 세팅
            if (_particle == null)
                _particle = this.GetComponent(typeof(ParticleSystem)) as ParticleSystem;

            // 애니메이션 세팅
            if (_anim == null)
                _anim = GetComponentInChildren(typeof(Animation), true) as Animation;

            if (_anim != null && string.IsNullOrWhiteSpace(_AnimName_Start) && string.IsNullOrWhiteSpace(_AnimName_Loop))
            {
                if (_anim.clip != null)
                {
                    if (_anim.clip.isLooping)
                        _AnimName_Loop = _anim.clip.name;
                    else
                        _AnimName_Start = _anim.clip.name;
                }
            }

            // 스프라이트 애니메이션 세팅
            if (_spriteAnim == null)
                _spriteAnim = GetComponent(typeof(SpriteAnimation)) as SpriteAnimation;

            if (trailRenderer != null)
                trailRenderer = GetComponent(typeof(TrailRenderer)) as TrailRenderer;

            // 렌더러 세팅
            _renderers = this.GetComponentsInChildren<Renderer>(true);


            // Loop 체크
            _particleChildren = this.GetComponentsInChildren<ParticleSystem>(true);
            for (int i = 0; i < _particleChildren.Length; ++i)
                IsLoop |= _particleChildren[i].main.loop;

            if (!string.IsNullOrEmpty(_AnimName_Loop))
                IsLoop = true;

            if (_spriteAnim != null)
            {
                IsLoop |= _spriteAnim.wrapMode == SpriteAnimation.WrapMode.Loop;
                IsLoop |= _spriteAnim.wrapMode == SpriteAnimation.WrapMode.PingPong;
            }

            if (!IsLoop)
            {
                if (_particle == null 
                    && _particleChildren.Length == 0 
                    && _anim == null
                    && _spriteAnim == null)
                {
                    IsLoop = true;
                }
            }
        }

        protected override void LateUpdate()
        {
            if (!bWaitForEnd)
            {
                if (!IsLoop)
                {
                    // 파티클이 살아 있다면
                    if (_particle != null && _particle.IsAlive(true))
                        return;

                    // 애니메이션이 살아 있다면?
                    if (_anim != null && _anim.isPlaying)
                        return;

                    // Sprite 애니메이션이 살아 있다면?
                    if (_spriteAnim != null && _spriteAnim.isPlaying)
                        return;

                    ReturnToPool();
                }
                else if (IsLoop)
                {
                    // 시작 애니메이션이 살아 있다면?
                    if (_anim != null && _anim.clip != null && _anim.clip.name == _AnimName_Start)
                    {
                        _anim.clip = null;
                        _anim.CrossFade(_AnimName_Loop, 0.15f);
                    }
                }
            }

            base.LateUpdate();
        }

        public override void ReturnToPoolForce()
        {
            if (_particle != null)
                _particle.Stop(true, ParticleSystemStopBehavior.StopEmitting);

            if (_anim != null)
            {
                _anim.clip = null;
                _anim.Stop();
            }

            if (_spriteAnim != null)
                _spriteAnim.Stop();

            base.ReturnToPoolForce();
        }

        public override void ReturnToPool()
        {
            if (_particle != null)
                _particle.Stop(true, ParticleSystemStopBehavior.StopEmitting);

            if (_anim != null)
            {
                if (string.IsNullOrEmpty(_AnimName_End))
                    _anim.Stop();
                else
                    _anim.CrossFade(_AnimName_End, 0.15f);
            }

            if (_spriteAnim != null)
                _spriteAnim.Stop();

            if (_go_Others != null)
            {
                for (int i = 0; i < _go_Others.Length; ++i)
                {
                    if (_go_Others[i] == null)
                        continue;

                    _go_Others[i].SetActive(false);
                }
            }

            base.ReturnToPool();
        }

        public override bool CheckWaitForEnd()
        {
            // 파티클이 살아 있다면
            if (_particle != null && _particle.IsAlive(true))
                return false;

            // 애니메이션이 살아 있다면?
            if (_anim != null && _anim.isPlaying)
                return false;

            // Sprite 애니메이션이 살아 있다면?
            if (_spriteAnim != null && _spriteAnim.isPlaying)
                return false;

            return true;
        }

        public void PlayFX(float speedScale, float playTimeOffset)
        {
            if (_particle != null)
                _particle.Play(true);

            // 파티클 실행
            for (int i = 0; i < _particleChildren.Length; ++i)
            {
                _particleChildren[i].time = playTimeOffset;

                var main = _particleChildren[i].main;
                main.simulationSpeed = speedScale;
            }

            // 애니메이션 실행
            if (_anim != null)
            {
                if(!string.IsNullOrEmpty(_AnimName_Start))
                {
                    _anim.clip = _anim.GetClip(_AnimName_Start);
                    _anim.Play(_AnimName_Start);
                    _anim[_AnimName_Start].time = playTimeOffset;
                    _anim[_AnimName_Start].speed = speedScale;
                }
                else if (!string.IsNullOrEmpty(_AnimName_Loop))
                {
                    _anim.clip = _anim.GetClip(_AnimName_Loop);
                    _anim.Play(_AnimName_Loop);
                    _anim[_AnimName_Loop].time = playTimeOffset;
                    _anim[_AnimName_Loop].speed = speedScale;
                }
                else if (_anim.clip != null)
                {
                    _anim.clip = null;
                }
            }

            // 스프라이트 애니메이션 실행
            if (_spriteAnim != null)
            {
                _spriteAnim.Play();
                _spriteAnim.time = playTimeOffset;
                _spriteAnim.normalizedSpeed = speedScale;
            }

            if (_go_Others != null)
            {
                for (int i = 0; i < _go_Others.Length; ++i)
                {
                    if (_go_Others[i] == null)
                        continue;

                    _go_Others[i].SetActive(true);
                }
            }
        }

        public void SetSpeedScale(float speedScale)
        {
            if (_anim != null && _anim.clip != null)
                _anim[_anim.clip.name].speed = speedScale;

            for (int i = 0; i < _particleChildren.Length; ++i)
            {
                var main = _particleChildren[i].main;
                main.simulationSpeed = speedScale;
            }

            if (_spriteAnim != null)
                _spriteAnim.normalizedSpeed = speedScale;
        }
    }
}