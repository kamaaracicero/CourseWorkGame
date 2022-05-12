using AmazingUILibrary.Backgrounds;
using AmazingUILibrary.Containers;
using SharpDX;

namespace AmazingUILibrary.Elements
{
    public class UIButton : UISingleElementContainer
    {
        private IBackground _pressedBackground;
        private IBackground _releasedBackground;

        public IBackground PressedBackground { get => _pressedBackground; set => _pressedBackground = value; }
        public IBackground ReleasedBackground 
        {
            get => _releasedBackground; 
            set
            {
                _releasedBackground = value;
                Background = value;
            }
        }

        public UIButton(Vector2 position, Vector2 size, UIElement element) 
            : base(position, size, element)
        {
            IsClickable = true;
            OnPressed += () => Background = _pressedBackground;
            OnReleazed += () => Background = _releasedBackground;
        }

        public UIButton(Vector2 size, UIElement element)
            : this(Vector2.Zero, size, element)
        {
        }

        public UIButton(UIElement element)
            : this(Vector2.Zero, element.Size, element)
        {
        }
    }
}
