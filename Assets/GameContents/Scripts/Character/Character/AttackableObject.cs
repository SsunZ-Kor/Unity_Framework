using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Character
{
    public partial class AttackableObject : MonoBehaviour
    {
        public int TeamNo { get; protected set; }

        public int LayerMask_Mine { get; protected set; }
        public int LayerMask_Enemy { get; protected set; }
        public int Layer_Proj { get; protected set; } // 발사체 레이어

        public virtual void SetLayer(int userTeam, int team, int layerMask_Mine, int layerMask_Enemy)
        {
            LayerMask_Mine = layerMask_Mine;
            LayerMask_Enemy = layerMask_Enemy;

            TeamNo = team;

            if(userTeam == team)
            {
                this.gameObject.SetLayer(LayerMask.NameToLayer("MyCompany_Character"), true);
                Layer_Proj = LayerMask.NameToLayer("MyCompany_Projectile");
            }
            else
            {
                this.gameObject.SetLayer(LayerMask.NameToLayer("EnemyCompany_Character"), true);
                Layer_Proj = LayerMask.NameToLayer("EnemyCompany_Projectile");
            }
        }
    }
}