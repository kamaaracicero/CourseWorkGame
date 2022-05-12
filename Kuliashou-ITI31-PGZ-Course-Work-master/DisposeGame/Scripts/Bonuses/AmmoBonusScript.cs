using DisposeGame.Components;
using GameEngine.Graphics;

namespace DisposeGame.Scripts.Bonuses
{
    public class AmmoBonusScript : PickableBonusScript
    {
        public AmmoBonusScript(Game3DObject picker, int ammo = 20) : base(picker)
        {
            OnPicked += _ => picker.GetComponent<AmmoComponent>().AddAmmo(ammo);
        }
    }
}
