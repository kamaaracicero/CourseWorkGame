using GameEngine.Collisions;
using GameEngine.Graphics;
using GameEngine.Scripts;
using System;

namespace DisposeGame.Scripts.Bonuses
{
    public abstract class PickableBonusScript : Script
    {
        private Game3DObject _picker;

        public event Action<Game3DObject> OnPicked;

        public PickableBonusScript(Game3DObject picker)
        {
            _picker = picker;
        }

        public override void Update(float delta)
        {
            if (ObjectCollision.Intersects(_picker.Collision, GameObject.Collision))
            {
                OnPicked?.Invoke(_picker);
                GameObject.Scene.RemoveGameObject(GameObject);
            }
        }
    }
}
