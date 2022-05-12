using GameEngine.Collisions;
using GameEngine.Components;
using GameEngine.Graphics;
using SharpDX;
using System;
using System.Collections.Generic;

namespace DisposeGame.Components
{
    public class PhysicsComponent : ObjectComponent
    {
        private List<Game3DObject> _walls;
        private List<Vector3> _impulses = new List<Vector3>();
        private Vector3 _gravity;
        private Vector3 _direction;
        private float _friction;

        public PhysicsComponent(List<Game3DObject> walls, float gravity = 1, float friction = 1)
        {
            _gravity = Vector3.UnitY * -gravity;
            _friction = friction;
            _direction = Vector3.Zero;
            _walls = walls;
        }

        public void UpdateObjectPosition(float delta)
        {
            Vector3 acceleration = _gravity * delta;
            for (int i = 0; i < _impulses.Count; i++)
            {
                acceleration += _impulses[i] * delta;
            }
            _impulses.Clear();

            _direction += acceleration;

            _direction -= (GetSignVector(_direction) * _friction * delta);

            if (MathUtil.NearEqual(_direction.X, 0))
            {
                _direction.X = 0;
            }
            if (MathUtil.NearEqual(_direction.Z, 0))
            {
                _direction.Z = 0;
            }

            GameObject.MoveBy(_direction);

            var partX = _direction * Vector3.UnitX;
            var partZ = _direction * Vector3.UnitZ;

            foreach (var gameObject in _walls)
            {
                if (ObjectCollision.Intersects(GameObject.Collision, gameObject.Collision))
                {
                    GameObject.MoveBy(-partX);
                    if (!ObjectCollision.Intersects(GameObject.Collision, gameObject.Collision))
                    {
                        continue;
                    }
                    GameObject.MoveBy(partX);
                    GameObject.MoveBy(-partZ);
                    if (!ObjectCollision.Intersects(GameObject.Collision, gameObject.Collision))
                    {
                        continue;
                    }
                    GameObject.MoveBy(partZ);
                }
            }

            Vector3 position = GameObject.Position;
            if (position.Y < 0)
            {
                _direction.Y = 0;
                GameObject.MoveTo(new Vector3(position.X, 0, position.Z));
            }
        }

        private static Vector3 GetSignVector(Vector3 vector)
        {
            return new Vector3(Math.Sign(vector.X), Math.Sign(vector.X), Math.Sign(vector.Z));
        }

        public void AddImpulse(Vector3 impulse)
        {
            _impulses.Add(impulse);
        }
    }
}
