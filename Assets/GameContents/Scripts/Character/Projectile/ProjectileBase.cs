using Game.Character;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public abstract class ProjectileBase : FXObject
    {
        [SerializeField]
        private Color[] teamColor = null;

        public Character.CharacterObject Owner { get; protected set; }
        protected int _targetLayerMask;

        public System.Action<AttackableObject, Vector3> _callback_Release = null;

        public virtual void Shot(CharacterObject owner, int targetLayerMask, System.Action<AttackableObject, Vector3> callback_Release)
        {
            Owner = owner;
            _targetLayerMask = targetLayerMask;
            _callback_Release = callback_Release;

            ChangeColor(GetTeamColor(owner.TeamNo));
        }

        public virtual void ReturnToPool(AttackableObject hitObj, Vector3 releasePos)
        {
            if (_callback_Release != null)
            {
                var call = _callback_Release;
                _callback_Release = null;
                call.Invoke(hitObj, releasePos);
            }

            base.ReturnToPool();
        }

        protected void ChangeColor(Color color)
        {
            if (_renderers != null && _renderers.Length > 0)
            {
                foreach (var renderer in _renderers)
                    renderer.material.color = color;
            }

            if (_particle != null)
            {
                var mainModule = _particle.main;
                mainModule.startColor = color;
            }

            if (trailRenderer != null)
            {
                trailRenderer.startColor = color;
            }
        }

        protected Color GetTeamColor(int teamNo)
        {
            var teamIdx = teamNo - 1;
            if (teamColor == null || !teamColor.CheckIndex(teamIdx))
                return Color.white;

            return teamColor[teamIdx];
        }
    }
}