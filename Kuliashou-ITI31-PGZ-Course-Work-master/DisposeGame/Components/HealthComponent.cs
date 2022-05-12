using GameEngine.Components;
using System;

namespace DisposeGame.Components
{
    public class HealthComponent : ObjectComponent
    {
        public event Action OnDeath;
        public event Action<int> OnChanged;
        public event Action<int, int> OnDamaged;
        public event Action<int, int> OnHealed;

        private int _maxHealth;
        private int _health;
        private bool _isDead;

        public bool IsUnbreakable { get; set; }

        public HealthComponent(int health)
        {
            _maxHealth = health;
            _health = health;
            _isDead = false;
        }

        public void DealDamage(int damage)
        {
            if (_isDead) return;
            if (IsUnbreakable) return;

            _health -= damage;
            OnDamaged?.Invoke(_health, damage);

            if (_health <= 0)
            {
                _isDead = true;
                GameObject.Scene.RemoveGameObject(GameObject);
                OnDeath?.Invoke();
            }

            OnChanged?.Invoke(_health);
        }

        public void Heal(int heal)
        {
            _health += heal;
            OnHealed?.Invoke(_health, heal);

            if (_health > _maxHealth)
            {
                _health = _maxHealth;
            }

            OnChanged?.Invoke(_health);
        }
    }
}
