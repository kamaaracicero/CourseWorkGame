using SharpDX;
using AmazingUILibrary.Drawing;

namespace AmazingUILibrary.Elements
{
    public class UIText : UIElement
    {
        private string _text;
        private string _brush;
        private string _format;

        public string Text
        {
            get => _text;
            set
            {
                _text = value;
            }
        }

        public UIText(Vector2 position, string text, Vector2 size, string format, string brush)
            : base(position, size)
        {
            _text = text;
            _brush = brush;
            _format = format;
        }

        public UIText(string text, Vector2 size, string format, string brush)
            : this(Vector2.Zero, text, size, format, brush)
        {
        }

        internal override void Draw(DrawingContext context)
        {
            context.DrawText(_text, _format, BoundingRectangle, _brush);
        }
    }
}
