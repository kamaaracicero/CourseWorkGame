using DisposeGame.Components;
using GameEngine.Scripts;

namespace DisposeGame.Scripts.Character
{
    public class PlayerUnbreakableScript : Script
    {
        private HealthComponent _health;

        private bool _isReloading;
        private float _reloadingTime;
        private float _cooldown;

        public PlayerUnbreakableScript(HealthComponent health, float cooldown = 1f)
        {
            _cooldown = cooldown;
            _isReloading = false;
            _reloadingTime = 0;
            _health = health;
            _health.OnDamaged += (_1, _2) =>
            { 
                _health.IsUnbreakable = true;

                _reloadingTime = 0;
                _isReloading = true;
            };
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

            _health.IsUnbreakable = false;
        }
    }
}
