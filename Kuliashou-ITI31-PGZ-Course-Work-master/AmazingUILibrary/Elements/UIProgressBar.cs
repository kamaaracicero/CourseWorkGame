using AmazingUILibrary.Drawing;
using SharpDX;

namespace AmazingUILibrary.Elements
{
    public class UIProgressBar : UIElement
    {
        private string _brush;

        private float _maxValue;
        private float _value;

        public float MaxValue { get => _maxValue; set => _maxValue = value; }
        public float Value { get => _value; set => _value = value; }

        public UIProgressBar(Vector2 position, Vector2 size, string brush) : base(position, size)
        {
            _value = 0;
            _maxValue = size.X;
            _brush = brush;
        }

        internal override void Draw(DrawingContext context)
        {
            var bounds = BoundingRectangle;
            bounds.Right -= (Size.X * (1 - (_value / _maxValue)));
            context.DrawRectangle(bounds, _brush);
        }
    }
}
