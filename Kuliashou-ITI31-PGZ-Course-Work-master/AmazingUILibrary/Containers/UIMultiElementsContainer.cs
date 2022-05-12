using AmazingUILibrary.Drawing;
using SharpDX;
using System.Collections.Generic;

namespace AmazingUILibrary.Containers
{
    public class UIMultiElementsContainer : UIElement
    {
        protected List<UIElement> Elements { get; }

        public UIMultiElementsContainer(Vector2 position, Vector2 size) : base(position, size)
        {
            Elements = new List<UIElement>();
        }

        public virtual void Add(UIElement element)
        {
            element.Parent = this;
            Elements.Add(element);
        }

        public virtual void Remove(UIElement element)
        {
            if (Elements.Remove(element))
            {
                element.Parent = null;
            }
        }

        internal override void Draw(DrawingContext context)
        {
            foreach (UIElement element in Elements)
            {
                element.BeginDraw(context);
            }
        }

        public override bool Press(float x, float y)
        {
            if (!IsVisible) return false;
            bool isPressed = false;
            for (int i = 0, len = Elements.Count; i < len; i++)
            {
                isPressed = Elements[i].Press(x, y) || isPressed;
            }
            isPressed = base.Press(x, y) || isPressed;
            return isPressed;
        }

        public override bool Release(float x, float y)
        {
            if (!IsVisible) return false;
            bool isReleased = false;
            for (int i = 0, len = Elements.Count; i < len; i++)
            {
                isReleased = Elements[i].Release(x, y) || isReleased;
            }
            isReleased = base.Release(x, y) || isReleased;
            return isReleased;
        }
    }
}
