using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public abstract class PoolingObject : MonoBehaviour
    {
        [System.NonSerialized]
        public PoolBase _Pool;
        public LinkedListNode<PoolingObject> Node_PoolingObj { get; protected set; }

        protected bool bWaitForEnd;

        protected virtual void Awake()
        {
            Node_PoolingObj = new LinkedListNode<PoolingObject>(this);
        }

        protected virtual void LateUpdate()
        {
            if (bWaitForEnd && CheckWaitForEnd())
                ReturnToPoolForce();
        }

        public virtual void ResetObject()
        {
            bWaitForEnd = false;
        }

        public virtual void GenerateObject(Vector3 vLocalPos, Quaternion qLocalRot, Vector3 vLocalScale, Transform tr_Parent)
        {
            bWaitForEnd = false;

            this.transform.SetParent(tr_Parent);
            this.transform.localPosition = vLocalPos;
            this.transform.localRotation = qLocalRot;
            this.transform.localScale = vLocalScale;

            this.gameObject.SetActive(true);
        }

        public virtual void ReturnToPoolForce()
        {
            // Destroy 됬을 경우 예외처리
            if (this == null || this.gameObject == null)
            {
#if DevClient
                Debug.LogError("ReturnToPoolForce :: PoolingObj is Destroyed");
#endif
                return;
            }

            if (_Pool == null)
            {
                GameObject.Destroy(this.gameObject);
                return;
            }

            if (_Pool.IsInactive(this))
                return;

            _Pool.Push(this);
            this.gameObject.SetActive(false);
        }

        public virtual void ReturnToPool()
        {
            bWaitForEnd = true;
            if (CheckWaitForEnd())
                ReturnToPoolForce();
        }

        public abstract bool CheckWaitForEnd();
    }
}

