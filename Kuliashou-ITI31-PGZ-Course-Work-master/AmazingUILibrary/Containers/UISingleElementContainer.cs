using AmazingUILibrary.Drawing;
using SharpDX;

namespace AmazingUILibrary.Containers
{
    public class UISingleElementContainer : UIElement
    {
        private readonly UIElement _element;

        public UISingleElementContainer(Vector2 position, Vector2 size, UIElement element) : base(position, size)
        {
            _element = element;
            element.Parent = this;
        }

        internal override void Draw(DrawingContext context)
        {
            _element.BeginDraw(context);
        }

        public override bool Press(float x, float y)
        {
            if (!IsVisible) return false;
            return _element.Press(x, y) | base.Press(x, y);
        }

        public override bool Release(float x, float y)
        {
            if (!IsVisible) return false;
            return _element.Release(x, y) | base.Release(x, y);
        }
    }
}
