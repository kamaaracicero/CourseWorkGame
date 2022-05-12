using AmazingUILibrary;
using GameEngine.Animation;
using GameEngine.Game;
using GameEngine.Scripts;
using SharpDX;

namespace DisposeGame.Scripts
{
    public class DinoClickScript : Script
    {
        private InputController _inputController;
        private UIElement _ui;

        public DinoClickScript(UIElement ui)
        {
            _inputController = InputController.GetInstance();
            _ui = ui;
        }

        public override void Update(float delta)
        {
            if(_inputController.CursorRay != null)
            {
                if (_inputController.CursorRay.Value.Intersects(new BoundingSphere(GameObject.Position, 1)))
                {
                    new SmoothTransition(0, MathUtil.TwoPi, 2).Process += value => GameObject.SetRotationX(value);

                    _ui.LocalPosition = new Vector2(_inputController.MousePositionX, _inputController.MousePositionY);
                    _ui.IsVisible = true;

                    _inputController.CursorRay = null;
                }
            }
        }
    }
}
