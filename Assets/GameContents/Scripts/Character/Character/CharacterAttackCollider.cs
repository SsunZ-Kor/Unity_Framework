using BubbleFighter.Network.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Character
{
    public class CharacterAttackCollider : MonoBehaviour
    {
        private HashSet<Collider> _set_Object = new HashSet<Collider>();
        private System.Action<AttackableObject, Vector3> _callback_Hit;

        private void Awake()
        {
            var myCol = this.GetComponent<Collider>();
            if (myCol != null)
            {
                myCol.isTrigger = false;

                var myMeshCol = myCol as MeshCollider;
                if (myMeshCol != null)
                    myMeshCol.convex = true;
            }
            else
            {
                Debug.LogError("CharacterAttackCollider->Awake :: Not Found Collider");
            }

            var myRigidBody = this.GetComponent<Rigidbody>();
            if (myRigidBody != null)
            {
                myRigidBody.useGravity = false;
                myRigidBody.isKinematic = false;
                myRigidBody.mass = 0f;
                myRigidBody.constraints = RigidbodyConstraints.FreezeAll;
            }
            else
            {
                Debug.LogError("CharacterAttackCollider->Awake :: Not Found RigidBody");
            }
        }

        private void OnDisable()
        {
            _set_Object.Clear();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (_set_Object.Contains(collision.collider))
                return;

            _set_Object.Add(collision.collider);

            var atkObj = collision.collider.GetComponent<AttackableObject>();
            if (atkObj == null)
                return;

            Debug.Log($"AttackHit : {atkObj.name}");

            if (_callback_Hit != null)
            {
                var call = _callback_Hit;
                _callback_Hit = null;
                call.Invoke(atkObj, collision.contacts[0].point);
            }
        }

        public void SetCallback(System.Action<AttackableObject, Vector3> callback_Hit)
        {
            _callback_Hit = callback_Hit;
        }
    }
}