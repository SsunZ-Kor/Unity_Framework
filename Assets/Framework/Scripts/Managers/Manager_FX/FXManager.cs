using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class FXManager : ManagerBase
    {
        public readonly Dictionary<string, int> _dic_FxRefCounts = new Dictionary<string, int>(); 
        public readonly Dictionary<string, PoolBase> _dic_FxPools = new Dictionary<string, PoolBase>();

        public override IEnumerator Init_Async()
        {
            yield break;
        }

        public bool IsRegistedFX(string Name)
        {
            if (string.IsNullOrEmpty(Name))
                return false;

            return _dic_FxRefCounts.ContainsKey(Name);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="prf_Fx">주의! 모든 이펙트들은 서로 다른 이름을 가지고 있어야 합니다.</param>
        /// <param name="count"></param>
        public void RegistFX(GameObject prf_Fx, int nInitCount = 1)
        {
            RegistFX<FXObject>(prf_Fx, nInitCount);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="prf_Fx">주의! 모든 이펙트들은 서로 다른 이름을 가지고 있어야 합니다.</param>
        /// <param name="count"></param>
        public void RegistFX<T>(GameObject prf_Fx, int nInitCount = 1) where T : FXObject
        {
            if (prf_Fx == null)
                return;

            prf_Fx.GetOrAddComponent<T>();
            if (_dic_FxRefCounts.ContainsKey(prf_Fx.name))
            {
                ++_dic_FxRefCounts[prf_Fx.name];
            }
            else
            {
                _dic_FxRefCounts.Add(prf_Fx.name, nInitCount);

                PoolBase fxPool = null;

                if (_dic_FxPools.TryGetValue(prf_Fx.name, out fxPool))
                {
                    if (fxPool == null)
                        _dic_FxPools.Remove(prf_Fx.name);
                }

                if (fxPool == null)
                {
                    fxPool = PoolBase.Create(string.Format("Pool_{0}", prf_Fx.name), this.transform);
                    _dic_FxPools.Add(prf_Fx.name, fxPool);
                }

                fxPool.Init(prf_Fx, nInitCount);
            }
        }

        public int GetRegistCount(string prfName)
        {
            int result = 0;
            if (_dic_FxRefCounts.TryGetValue(prfName, out result))
                return result;

            return 0;
        }

        public void RegistAdd(string prfName)
        {
            if (_dic_FxRefCounts.ContainsKey(prfName))
                ++_dic_FxRefCounts[prfName];
        }

        public void RemoveFX(string fxName)
        {
            if (string.IsNullOrEmpty(fxName))
                return;

            if (!_dic_FxRefCounts.ContainsKey(fxName))
                return;

            if (--_dic_FxRefCounts[fxName] > 0)
                return;

            _dic_FxRefCounts.Remove(fxName);

            PoolBase fxPool = null;
            if (!_dic_FxPools.TryGetValue(fxName, out fxPool))
                return;

            if (fxPool != null)
                GameObject.Destroy(fxPool.gameObject);

            _dic_FxPools.Remove(fxName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fxName">프리펩 이름</param>
        /// <param name="vLocalPos">로컬 좌표, tr_Parent == null이면 월드 좌표</param>
        /// <param name="qLocalRot">로컬 회전, tr_Parent == null이면 월드 회전</param>
        /// <param name="vLocalScale">로컬 스케일, tr_Parent == null이면 월드 스케일</param>
        /// <param name="tr_Parent">부모 Transform</param>
        /// <param name="playSpeedScale">재생 속도</param>
        /// <param name="playTimeOffset">빨리감기 Time</param>
        /// <returns></returns>
        public FXObject PlayFX(
            string fxName,
            Vector3 vLocalPos,
            Quaternion qLocalRot,
            Vector3 vLocalScale,
            Transform tr_Parent,
            float playSpeedScale,
            float playTimeOffset,
            PoolBase.PopOptionForNotEnough ePopType = PoolBase.PopOptionForNotEnough.Instantiate)
        {
            if (string.IsNullOrEmpty(fxName))
                return null;

            PoolBase fxPool = null;
            _dic_FxPools.TryGetValue(fxName, out fxPool);

            if (fxPool == null)
                return null;

            var fx = fxPool.Pop(ePopType) as FXObject;

            fx.GenerateObject(vLocalPos,qLocalRot, vLocalScale, tr_Parent);
            fx.PlayFX(playSpeedScale,playTimeOffset);

            return fx;
        }

        public FXObject PlayFX(
            string fxName,
            Transform tr_Parent,
            float playSpeedScale,
            float playTimeOffset,
            PoolBase.PopOptionForNotEnough ePopType = PoolBase.PopOptionForNotEnough.Instantiate)
        {
            return PlayFX(fxName, Vector3.zero, Quaternion.identity, Vector3.one, tr_Parent, playSpeedScale, playTimeOffset, ePopType);
        }


        public void RetrieveItems(string fxName)
        {
            var pool = _dic_FxPools.GetOrNull(fxName);
            if (pool == null)
                return;

            pool.RetrieveAllItems();
        }

        public void RetrieveAllItems()
        {
#if DevClient
            Debug.Log("FXManager->RetrieveAllItems");
#endif
            foreach(var pair in _dic_FxPools)
                pair.Value.RetrieveAllItems();
        }
    }
}