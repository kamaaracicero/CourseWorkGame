using GameEngine.Components;
using System;

namespace DisposeGame.Components
{
    public class AmmoComponent : ObjectComponent
    {
        public event Action<int> OnChanged;
        public event Action<int> OnSpended;
        public event Action<int> OnReplenished;

        public int Ammo { get; private set; }

        public AmmoComponent(int ammo = 30)
        {
            Ammo = ammo;
        }

        public void AddAmmo(int amount)
        {
            Ammo += amount;
            OnReplenished?.Invoke(amount);
            OnChanged?.Invoke(Ammo);
        }

        public void DecrementAmmo()
        {
            Ammo--;
            OnSpended?.Invoke(Ammo);
            OnChanged?.Invoke(Ammo);
        }
    }
}
