using SharpDX;

namespace AmazingUILibrary.Containers
{
    public class UIMarginContainer : UISingleElementContainer
    {

        public UIMarginContainer(UIElement element, float margin)
            : this(element, margin, margin, margin, margin)
        {
        }

        public UIMarginContainer(UIElement element, float horizontal, float vertical)
            : this(element, vertical, horizontal, vertical, horizontal)
        {
        }

        public UIMarginContainer(UIElement element, float top, float right, float bottom, float left)
            : base(element.LocalPosition, element.Size + new Vector2(left + right, top + bottom), element)
        {
            element.LocalPosition = new Vector2(left, top);
        }
    }
}
