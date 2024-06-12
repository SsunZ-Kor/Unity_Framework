using KinematicCharacterController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Character;

namespace Game
{
    public class SphereGravityFeild : GravityFeildBase
    {
        [SerializeField]
        protected SphereCollider GravityField;

        [SerializeField]
        protected float GravityStrength = 10;
        [SerializeField]
        protected Vector3 OrbitAxis = Vector3.forward;
        [SerializeField]
        protected float OrbitSpeed = 10;

        public CharacterWarp OnPlaygroundTeleportingZone;
        public CharacterWarp OnPlanetTeleportingZone;


        protected override void Start()
        {
            base.Start();

            OnPlaygroundTeleportingZone.OnCharacterTeleport -= ControlGravity;
            OnPlaygroundTeleportingZone.OnCharacterTeleport += ControlGravity;

            OnPlanetTeleportingZone.OnCharacterTeleport -= UnControlGravity;
            OnPlanetTeleportingZone.OnCharacterTeleport += UnControlGravity;
        }

        public override void UpdateMovement(out Vector3 goalPosition, out Quaternion goalRotation, float deltaTime)
        {
            // Rotate Sphere
            Quaternion targetRotation = Quaternion.Euler(OrbitAxis * OrbitSpeed * deltaTime) * _lastRotation;
            _lastRotation = targetRotation;

            // Apply gravity to characters
            foreach (CharacterObject character in _set_CharOnFeild)
                character.Gravity = (PlanetMover.transform.position - character.transform.position).normalized * GravityStrength;

            base.UpdateMovement(out goalPosition, out goalRotation, deltaTime);
        }
    }
}
