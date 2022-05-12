using DisposeGame.Components;
using GameEngine.Game;
using GameEngine.Graphics;
using GameEngine.Scripts;
using SharpDX;
using System.Collections.Generic;

namespace DisposeGame.Scripts.Character
{
    public class PlayerGunScript : Script
    {
        private InputController _inputController;

        private Game3DObject _bulletPrototype;

        private AmmoComponent _ammo;

        private List<Game3DObject> _walls;

        private bool _isReloading;
        private float _reloadingTime;
        private float _cooldown;

        public PlayerGunScript(Game3DObject bullet, AmmoComponent ammo, List<Game3DObject> walls, float cooldown = 0.2f)
        {
            _inputController = InputController.GetInstance();
            _bulletPrototype = bullet;
            _ammo = ammo;
            _cooldown = cooldown;
            _reloadingTime = 0;
            _isReloading = false;
            _walls = walls;
        }

        public override void Update(float delta)
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
            if (_ammo.Ammo <= 0) return;
            if (_inputController.MouseUpdate && _inputController.MouseButtons[0])
            {
                Vector3 rotation = GameObject.Rotation;
                Matrix rotationMatrix = Matrix.RotationYawPitchRoll(rotation.Z, rotation.Y, rotation.X);
                Vector3 direction = (Vector3)Vector3.Transform(-Vector3.UnitY, rotationMatrix);
                Game3DObject bullet = _bulletPrototype.GetCopy();
                bullet.MoveTo(GameObject.Position);
                bullet.AddScript(new BulletScript(direction, _walls));
                GameObject.Scene.AddGameObject(bullet);

                _reloadingTime = 0;
                _isReloading = true;
                _ammo.DecrementAmmo();
            }
        }
    }
}
