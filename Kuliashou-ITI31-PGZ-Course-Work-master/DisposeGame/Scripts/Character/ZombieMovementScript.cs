using DisposeGame.Components;
using GameEngine.Animation;
using GameEngine.Collisions;
using GameEngine.Graphics;
using GameEngine.Scripts;
using SharpDX;
using System;
using System.Collections.Generic;

namespace DisposeGame.Scripts.Character
{
    public class ZombieMovementScript : Script
    {
        private CharacterMovement _movement;
        private Game3DObject _target;
        private HealthComponent _targetHealth;
        private PhysicsComponent _targetPhysics;
        private VisibilityComponent _targetVisibility;
        private float _radius;
        private int _damage;

        private bool _isReloading;
        private float _reloadingTime;
        private float _cooldown;

        public ZombieMovementScript(Game3DObject target, Animation animation, List<Game3DObject> walls, float speed = 20f, float radius = 40f, int damage = 10, float cooldown = 0.2f)
        {
            _target = target;
            _radius = radius;
            _movement = new CharacterMovement(animation, speed, walls);
            _damage = damage;

            _targetHealth = _target.GetComponent<HealthComponent>();
            _targetPhysics = _target.GetComponent<PhysicsComponent>();
            _targetVisibility = _target.GetComponent<VisibilityComponent>();

            _cooldown = cooldown;
            _reloadingTime = 0;
            _isReloading = false;
        }

        public override void Update(float delta)
        {
            Vector3 direction = _target.Position - GameObject.Position;
            MoveZombie(delta, direction);
            AttackTarget(delta, direction);
        }

        private void MoveZombie(float delta, Vector3 direction)
        {
            float distance = (float)Math.Sqrt(direction.X * direction.X + direction.Z * direction.Z);

            if (distance > _radius || _targetVisibility.IsInvisible)
            {
                _movement.Move(GameObject, Vector3.Zero, delta);
                return;
            }

            float angle = (float)Math.Atan2(direction.X, direction.Z);
            GameObject.SetRotationZ(angle);

            _movement.Move(GameObject, Vector3.UnitZ, delta);
        }

        private void AttackTarget(float delta, Vector3 direction)
        {
            if (_isReloading)
            {
                _reloadingTime += delta;
                if (_reloadingTime >= _cooldown)
                {
                    _isReloading = false;
                }
                return;
            }

            if (ObjectCollision.Intersects(_target.Collision, GameObject.Collision))
            {
                if (_targetHealth.IsUnbreakable) return;

                _targetHealth.DealDamage(_damage);
                Vector3 impulse = Vector3.Normalize(direction) * _damage * _damage;
                impulse.Y = _damage;
                _targetPhysics.AddImpulse(impulse);

                _reloadingTime = 0;
                _isReloading = true;
            }
        }
    }
}
