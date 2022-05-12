using DisposeGame.Components;
using GameEngine.Graphics;

namespace DisposeGame.Scripts.Bonuses
{
    public class InvisibilityBonusScript : PickableBonusScript
    {
        public InvisibilityBonusScript(Game3DObject picker, float duration = 5) : base(picker)
        {
            OnPicked += _ => picker.GetComponent<VisibilityComponent>().MakeInvisible(duration);
        }
    }
}
