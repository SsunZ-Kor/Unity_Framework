using UnityEngine;
using System.Collections;

namespace Game
{
    public abstract class ManagerBase : MonoBehaviour
    {
        protected virtual void OnDisable()
        {
            this.StopAllCoroutines();
        }

        public abstract IEnumerator Init_Async();
    }
}