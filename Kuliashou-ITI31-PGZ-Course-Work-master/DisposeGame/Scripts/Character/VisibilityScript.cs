using DisposeGame.Components;
using GameEngine.Scripts;

namespace DisposeGame.Scripts.Character
{
    public class VisibilityScript : Script
    {
        private VisibilityComponent _visibility;

        public VisibilityScript(VisibilityComponent visibility)
        {
            _visibility = visibility;
        }

        public override void Update(float delta)
        {
            _visibility.UpdateVisibility(delta);
        }
    }
}
