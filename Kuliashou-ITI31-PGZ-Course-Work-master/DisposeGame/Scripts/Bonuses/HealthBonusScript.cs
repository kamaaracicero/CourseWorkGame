using DisposeGame.Components;
using GameEngine.Graphics;

namespace DisposeGame.Scripts.Bonuses
{
    public class HealthBonusScript : PickableBonusScript
    {
        public HealthBonusScript(Game3DObject picker, int heal = 10) : base(picker)
        {
            OnPicked += _ => picker.GetComponent<HealthComponent>().Heal(heal);
        }
    }
}
