using DisposeGame.Components;
using GameEngine.Collisions;
using GameEngine.Graphics;
using GameEngine.Scripts;
using SharpDX;
using System.Collections.Generic;

namespace DisposeGame.Scripts
{
    public class BulletScript : Script
    {
        private Vector3 _direction;
        private float _speed;
        private int _damage;

        private List<Game3DObject> _walls;

        public BulletScript(Vector3 direction, List<Game3DObject> walls, float speed = 100f, int damage = 10)
        {
            _direction = direction;
            _speed = speed;
            _damage = damage;
            _walls = walls;
        }

        public override void Update(float delta)
        {
            GameObject.MoveBy(_direction * delta * _speed);
            ObjectCollision collision = GameObject.Collision;
            foreach (Game3DObject gameObject in GameObject.Scene.GameObjects)
            {
                if (gameObject == GameObject) continue;

                if (gameObject.Collision != null)
                {
                    if (ObjectCollision.Intersects(gameObject.Collision, collision))
                    {
                        gameObject.GetComponent<HealthComponent>()?.DealDamage(_damage);
                        GameObject.Scene.RemoveGameObject(GameObject);
                    }
                }
            }
            foreach(Game3DObject wall in _walls)
            {
                if (wall.Collision != null)
                {
                    if (ObjectCollision.Intersects(wall.Collision, collision))
                    {
                        GameObject.Scene.RemoveGameObject(GameObject);
                    }
                }
            }
        }
    }
}
