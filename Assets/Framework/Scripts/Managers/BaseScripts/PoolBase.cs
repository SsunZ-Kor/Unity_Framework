using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class PoolBase : MonoBehaviour
    {
        public enum PopOptionForNotEnough
        {
            None,
            Force,
            Instantiate,
        }


        protected GameObject prf_Object;

        protected LinkedList<PoolingObject> _activeObject = new LinkedList<PoolingObject>();
        protected LinkedList<PoolingObject> _inactiveObject = new LinkedList<PoolingObject>();

        public int Count {get; protected set;}

        public static PoolBase Create(string name, Transform tr_Parent)
        {
            var newGo_Pool = new GameObject(name);
            if (tr_Parent is RectTransform)
                newGo_Pool.AddComponent(typeof(RectTransform));

            newGo_Pool.transform.SetParent(tr_Parent);
            newGo_Pool.transform.Reset();
            
            return newGo_Pool.AddComponent(typeof(PoolBase)) as PoolBase;
        }

        private void OnDestroy()
        {
            while(_activeObject.Count > 0)
            {
                if (_activeObject.First.Value != null)
                    GameObject.Destroy(_activeObject.First.Value.gameObject);

                _activeObject.RemoveFirst();
            }

            while (_inactiveObject.Count > 0)
            {
                if (_inactiveObject.First.Value != null)
                    GameObject.Destroy(_inactiveObject.First.Value.gameObject);

                _inactiveObject.RemoveFirst();
            }

            _activeObject.Clear();
            _inactiveObject.Clear();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="prf">반드시 템플릿에 알맞는 컴포넌트를 소유하고 있어야함</param>
        public void Init(GameObject prf, int count)
        {
            int totalObjCount = _activeObject.Count + _inactiveObject.Count;
            if (totalObjCount == count)
                return;

            prf_Object = prf;
            Count = count;

            if (totalObjCount < count)
            {
                while (totalObjCount < count)
                {
                    var go_New = GameObject.Instantiate(prf, this.transform) as GameObject;
                    var comp_T = go_New.GetComponent(typeof(PoolingObject)) as PoolingObject;
                    if (comp_T == null)
                        throw new System.Exception(string.Format("PoolBase->Init is Failed. Because Not Found {0} Components", this.gameObject.name));

                    comp_T._Pool = this;

                    _inactiveObject.AddLast(comp_T.Node_PoolingObj);
                    totalObjCount = _activeObject.Count + _inactiveObject.Count;
                    go_New.SetActive(false);
                }
            }
            else
            {
                while (totalObjCount > count)
                {
                    if(_inactiveObject.Count > 0)
                    {
                        var poolObj_Del = _inactiveObject.First.Value;
                        _inactiveObject.RemoveFirst();

                        GameObject.Destroy(poolObj_Del.gameObject);
                    }
                }
            }
        }

        /// <summary>
        /// Pop한뒤 오브젝트를 수집하여 Disable시에 PoolingObject.ReturnToPoolForce를 꼭 호출하세요.
        /// </summary>
        /// <param name="bForce"></param>
        /// <returns></returns>
        public PoolingObject Pop(PopOptionForNotEnough popOption)
        {
            PoolingObject poolingObj = null;

            // 안쓰고 있는 풀링 오브젝트가 존재한다면
            if (_inactiveObject.Count > 0)
            {
                poolingObj = _inactiveObject.First.Value;
                _inactiveObject.RemoveFirst();
            }
            // 안쓰고 있는 오브젝트가 없지만, 강제로 꺼내와야 할 경우에
            else
            {
                switch(popOption)
                {
                    case PopOptionForNotEnough.None:
                    {

                    }
                    return null;
                    case PopOptionForNotEnough.Force:
                    {
                        poolingObj = _activeObject.First.Value;
                        poolingObj.ReturnToPoolForce();

                        poolingObj = _inactiveObject.First.Value;
                        _inactiveObject.RemoveFirst();
                    }
                    break;
                    case PopOptionForNotEnough.Instantiate:
                    {
                        GameObject go_New = GameObject.Instantiate(prf_Object, this.transform);
                        poolingObj = go_New.GetComponent(typeof(PoolingObject)) as PoolingObject;
                        if (poolingObj == null)
                            throw new System.Exception(string.Format("PoolBase->Init is Failed. Because Not Found {0} Components", this.gameObject.name));

                        poolingObj._Pool = this;
                    }
                    break;
                }
            }

            // 꺼내온게 있다면
            if (poolingObj != null)
            {
                _activeObject.AddLast(poolingObj.Node_PoolingObj);
                poolingObj.ResetObject();
                poolingObj.transform.SetParent(null);
            }

            return poolingObj;
        }

        public void Push(PoolingObject poolingObj)
        {
            if (poolingObj == null)
            {
                Debug.LogError(string.Format("PoolingObj is Null : {0}", this.gameObject.name));
                return;
            }

            poolingObj.transform.SetParent(this.transform);

            // 어떤 리스트에 있던 녀석이건 일단은 해당 리스트에서 삭제
            var prevLlint = poolingObj.Node_PoolingObj.List;
            if (prevLlint != null)
                prevLlint.Remove(poolingObj.Node_PoolingObj);

            _inactiveObject.AddLast(poolingObj.Node_PoolingObj);
        }

        public bool IsActive(PoolingObject poolingObj)
        {
            return poolingObj.Node_PoolingObj.List == this._activeObject;
        }

        public bool IsInactive(PoolingObject poolingObj)
        {
            return poolingObj.Node_PoolingObj.List == this._inactiveObject;
        }

        public List<PoolingObject> GetAllItem()
        {
            var result = new List<PoolingObject>(Count);
            result.AddRange(_inactiveObject);
            result.AddRange(_activeObject);

            return result;
        }

        public void RetrieveAllItems()
        {
            while (_activeObject.Count > 0)
            {
                if (_activeObject.First.Value != null)
                    _activeObject.First.Value.ReturnToPoolForce();
                else
                    _activeObject.RemoveFirst();
            }

            _activeObject.Clear();
        }
    }
}

