using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public enum BattleSceneStateType
    {
        None,
        Ready,
        Start,
        Playing,
        End,
    }


    public abstract class BattleSceneStateBase
    {
        protected BattleSceneController battleSceneCtrl { get; private set; }
         
        public virtual void Init(BattleSceneController battleSceneCtrl)
        {
            this.battleSceneCtrl = battleSceneCtrl;
        }

        public virtual void OnStart()
        {

        }

        public virtual void OnUpdate()
        {

        }

        public virtual void OnEnd()
        {

        }
    }
}