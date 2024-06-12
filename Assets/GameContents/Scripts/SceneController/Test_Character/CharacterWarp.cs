using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Character;
using UnityEngine.Events;

namespace Game
{
    public class CharacterWarp : MonoBehaviour
    {
        public CharacterWarp TeleportTo;

        public UnityAction<CharacterObject> OnCharacterTeleport;

        public bool isBeingTeleportedTo { get; set; }

        private void OnTriggerEnter(Collider other)
        {
            if (!isBeingTeleportedTo)
            {
                var cc = other.GetComponent<CharacterObject>();
                if (cc != null && !cc.IsNetChar)
                {
                    cc.Motor.SetPositionAndRotation(TeleportTo.transform.position, TeleportTo.transform.rotation);
                    OnCharacterTeleport?.Invoke(cc);
                    TeleportTo.isBeingTeleportedTo = true;
                }
            }

            isBeingTeleportedTo = false;
        }
    }
}