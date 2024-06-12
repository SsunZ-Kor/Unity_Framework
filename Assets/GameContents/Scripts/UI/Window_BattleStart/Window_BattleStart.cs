using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class Window_BattleStart : WindowBase
    {
        [SerializeField]
        private AnimPanel _animPanel = null;

        public override bool CloseSelf()
        {
            if (_animPanel.IsPlay)
                return false;

            return base.CloseSelf();
        }

        public void Play(System.Action endCallback)
        {
            _animPanel.Play(endCallback, null);
        }
    }
}