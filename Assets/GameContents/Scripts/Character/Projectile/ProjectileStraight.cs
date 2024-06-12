using Game.Character;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class ProjectileStraight : ProjectileBase
    {
        [Header ("Proj Straight Settings")]
        [SerializeField]
        protected float _moveSpdPerSec;
        [SerializeField]
        protected float _radius = 0f;
        [SerializeField]
        protected float _liveTime = 1f;

        protected float _elasedTime = 0f;

        public override void Shot(Character.CharacterObject owner, int targetLayerMask, System.Action<AttackableObject, Vector3> callback_Release)
        {
            base.Shot(owner, targetLayerMask, callback_Release);
            targetLayerMask |= LayerMask.GetMask("World");

            _elasedTime = 0f;
        }

        private void Update()
        {
            if (bWaitForEnd)
                return;

            var moveDelta = _moveSpdPerSec * Time.deltaTime;

            if (Owner.CharCtrl.CtrlType != Character.CharacterController.ControllerType.Net)
            {
                RaycastHit hitInfo;

                if (_radius <= 0f)
                    Physics.Raycast(this.transform.position, this.transform.forward, out hitInfo, moveDelta, _targetLayerMask, QueryTriggerInteraction.Ignore);
                else
                    Physics.SphereCast(this.transform.position, _radius, this.transform.forward, out hitInfo, moveDelta, _targetLayerMask, QueryTriggerInteraction.Ignore);

                if (hitInfo.collider != null)
                {
                    var hitObj = hitInfo.collider.gameObject.GetComponent<AttackableObject>();
                    this.ReturnToPool(hitObj, this.transform.position);
                    return;
                }

                if (_elasedTime >= _liveTime)
                    ReturnToPool(null, this.transform.position);
                else
                    _elasedTime += Time.deltaTime;
            }

            this.transform.position += this.transform.forward * moveDelta;
        }
    }
}