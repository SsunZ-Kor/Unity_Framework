using KinematicCharacterController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Character
{
    public partial class CharacterObject : ICharacterController
    {
        [Header("Character Motor Settings")]
        [SerializeField]
        protected KinematicCharacterMotor _motor = null;
        public KinematicCharacterMotor Motor => _motor;

        [SerializeField]
        public Vector3 WorldUp { get; protected set; }
        public Vector3 _gravity = new Vector3(0, -30f, 0);
        public Vector3 Gravity
        {
            get
            {
                return _gravity;
            }
            set
            {
                _gravity = value;
                WorldUp = (-Gravity).normalized;
            }
        }

        public int GravityFeildIndex { get; protected set; } = -1;

        public bool IsAir { get; protected set; } = true;

        protected Vector3 _vMove;          // 캐릭터가 이동하고자 하는 방향
        protected Quaternion _qRot;          // 캐릭터가 보고자 하는 방향
        protected float _fRotSpdPerSec;    // 캐릭터가 보고자 하는 방향

        protected bool bRequestChangeVelocity = false;
        protected Vector3 _vVel;     // Velocity, 한번 세팅 후 끝낸다.

        #region 네트워크 동기화용
        protected bool isDirty = false;
        protected bool isDirty_Vel = false;
        protected Vector3 Cache_vVel;
        #endregion

        public void OnAwake_Move()
        {
            _qRot = this.transform.rotation;
            WorldUp = -_gravity.normalized;

            Motor.CharacterController = this;
        }

        public void SetPosition(Vector3 worldPosition)
        {
            Motor.SetPosition(worldPosition);
        }

        public void SetRotation(Quaternion worldRotation)
        {
            _qRot = worldRotation;
            Motor.SetRotation(worldRotation);
        }

        public virtual void SetMove(Vector3 vMove)
        {
            if (!IsNetChar)
                isDirty = _vMove != vMove;

            _vMove = vMove;

            var vLocalMoveDir = this.transform.InverseTransformVector(vMove.normalized);
            AnimCtrl.SetFloat("MoveDirX", vLocalMoveDir.x);
            AnimCtrl.SetFloat("MoveDirY", vLocalMoveDir.z);
        }

        public virtual void SetLook(Quaternion qRot, float fRotSpdPerSec)
        {
            if (!IsNetChar)
                isDirty = _qRot != qRot;

            _qRot = qRot;
            _fRotSpdPerSec = fRotSpdPerSec;
        }

        public virtual void SetVelocity(Vector3 vVel)
        {
            if (!IsNetChar)
            {
                isDirty = true;
                isDirty_Vel = true;
                Cache_vVel = vVel;
            }

            bRequestChangeVelocity = true;
            _vVel = vVel;

            if (!IsAir && Vector3.Project(_vVel, WorldUp).magnitude > 0f)
            {
                IsAir = true;
                Motor.ForceUnground();
            }
        }

        public virtual void SetGravityFeildIndex(int idx)
        {
            if (!IsNetChar)
                isDirty = GravityFeildIndex != idx;

            GravityFeildIndex = idx;
        }

        #region Motor에서 호출되는 ICharacterController 메서드
        public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
        {
            var currAngle = Quaternion.Angle(currentRotation, _qRot);
            if (currAngle > 0)
            {
                var deltaDegrees = _fRotSpdPerSec * deltaTime;
                currentRotation = Quaternion.Slerp(currentRotation, _qRot, deltaDegrees / currAngle);
            }

            var vLook = currentRotation * Vector3.forward;
            vLook = Vector3.Cross(WorldUp, Vector3.Cross(vLook, WorldUp).normalized);
            currentRotation = Quaternion.LookRotation(vLook, WorldUp);
        }

        public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            // 물리 이동 요청
            if (bRequestChangeVelocity)
            {
                bRequestChangeVelocity = false;
                currentVelocity = _vVel;

                return;
            }

            // 공중 이동
            if (IsAir)
            {
                if (_vMove != Vector3.zero)
                {
                }
            }
            // 지상 이동
            else
            {
                if (_vMove == Vector3.zero)
                {
                    currentVelocity = Vector3.zero;
                }
                else
                {
                    float currentVelocityMagnitude = currentVelocity.magnitude;

                    Vector3 effectiveGroundNormal = Motor.GroundingStatus.GroundNormal;
                    if (currentVelocityMagnitude > 0f && Motor.GroundingStatus.SnappingPrevented)
                    {
                        // Take the normal from where we're coming from
                        Vector3 groundPointToCharacter = Motor.TransientPosition - Motor.GroundingStatus.GroundPoint;
                        if (Vector3.Dot(currentVelocity, groundPointToCharacter) >= 0f)
                        {
                            effectiveGroundNormal = Motor.GroundingStatus.OuterGroundNormal;
                        }
                        else
                        {
                            effectiveGroundNormal = Motor.GroundingStatus.InnerGroundNormal;
                        }
                    }

                    // Reorient velocity on slope
                    currentVelocity = Motor.GetDirectionTangentToSurface(currentVelocity, effectiveGroundNormal) * currentVelocityMagnitude;

                    // Calculate target velocity
                    var moveSpdPerSec = _vMove.magnitude;
                    var moveDir = _vMove * (1 / moveSpdPerSec);

                    Vector3 inputRight = Vector3.Cross(moveDir, Motor.CharacterUp);
                    Vector3 targetMovementVelocity = Vector3.Cross(effectiveGroundNormal, inputRight).normalized * moveSpdPerSec;

                    currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity, 1f - Mathf.Exp(-15f * deltaTime));
                }
            }

            // 중력 처리
            currentVelocity += Gravity * deltaTime;
        }

        public void AfterCharacterUpdate(float deltaTime)
        {
        }

        public void BeforeCharacterUpdate(float deltaTime)
        {
        }

        public bool IsColliderValidForCollisions(Collider coll)
        {
            return true;
        }

        public void OnDiscreteCollisionDetected(Collider hitCollider)
        {
        }

        public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
        {
        }

        public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
        {
        }

        public void PostGroundingUpdate(float deltaTime)
        {
            // Handle landing and leaving ground
            if (Motor.GroundingStatus.IsStableOnGround && !Motor.LastGroundingStatus.IsStableOnGround)
            {
                IsAir = false;
            }
            else if (!Motor.GroundingStatus.IsStableOnGround && Motor.LastGroundingStatus.IsStableOnGround)
            {
                IsAir = true;
            }
        }

        public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
        {
        }
        #endregion

        protected void OnLateUpdate_NetChangeMove()
        {
            if (!isDirty)
                return;

            isDirty = false;

#if UNITY_EDITOR
            if (!Managers.IsValid || Managers.Net == null)
                return;
#endif

            if (IntroSceneController.UseUDP)
            {
                NetProcess.UDP_Send_BattleMove(
                    isDirty_Vel,
                    Cache_vVel,
                    _vMove,
                    this.transform.position,
                    _qRot,
                    GravityFeildIndex);
            }
            else
            {
                NetProcess.Send_BattleMove(
                    isDirty_Vel,
                    Cache_vVel,
                    _vMove,
                    this.transform.position,
                    _qRot,
                    GravityFeildIndex);
            }

            isDirty_Vel = false;
        }
    }
}