using Game.Character;
using KinematicCharacterController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public abstract class GravityFeildBase : MonoBehaviour, IMoverController
    {
        [SerializeField]
        public PhysicsMover PlanetMover;

        protected World _world;
        public int Index { get; protected set; }

        // 직접 Transform.XXXX으로 세팅하지 말고 여기다가 세팅한다.
        protected Vector3 _lastPosition;
        protected Quaternion _lastRotation;

        protected HashSet<CharacterObject> _set_CharOnFeild = new HashSet<CharacterObject>();

        public void Init(World world, int serialNo)
        {
            _world = world;
            Index = serialNo;
        }

        protected virtual void Start()
        {
            PlanetMover.MoverController = this;

            _lastPosition = PlanetMover.Rigidbody.position;
            _lastRotation = PlanetMover.Rigidbody.rotation;
        }

        public virtual void UpdateMovement(out Vector3 goalPosition, out Quaternion goalRotation, float deltaTime)
        {
            goalPosition = _lastPosition;
            goalRotation = _lastRotation;
        }

        public void ControlGravity(CharacterObject character)
        {
            if (_set_CharOnFeild.Contains(character))
                return;

            _set_CharOnFeild.Add(character);
            character.SetGravityFeildIndex(this.Index);
        }

        public void UnControlGravity(CharacterObject character)
        {
            if (!_set_CharOnFeild.Contains(character))
                return;

            _set_CharOnFeild.Remove(character);
            character.SetGravityFeildIndex(-1);

            character.Gravity = _world.BaseGravity;
        }
    }
}