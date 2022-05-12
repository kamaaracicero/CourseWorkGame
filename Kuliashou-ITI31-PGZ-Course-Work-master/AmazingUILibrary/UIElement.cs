using AmazingUILibrary.Backgrounds;
using AmazingUILibrary.Drawing;
using SharpDX;
using SharpDX.Mathematics.Interop;
using System;

namespace AmazingUILibrary
{
    public abstract class UIElement
    {
        public event Action OnClicked;
        public event Action OnPressed;
        public event Action OnReleazed;
        public event Action OnResized;
        public event Action OnMoved;

        private UIElement _parent;
        private IBackground _background;
        private Vector2 _position;
        private Vector2 _size;
        private bool _isPressed;

        public virtual UIElement Parent { get => _parent; set => _parent = value; }
        public virtual IBackground Background { get => _background; set => _background = value; }
        public virtual Vector2 GlobalPosition => (_parent != null) ? (_parent.GlobalPosition + LocalPosition) : LocalPosition;
        public virtual Vector2 LocalPosition
        {
            get => _position;
            set
            {
                _position = value;
                OnMoved?.Invoke();
            }
        }
        public virtual Vector2 Size
        {
            get => _size;
            set
            {
                _size = value;
                OnResized?.Invoke();
            }
        }

        public virtual RawRectangleF BoundingRectangle
        {
            get
            {
                Vector2 position = GlobalPosition;
                return new RawRectangleF(
                    position.X,
                    position.Y,
                    position.X + _size.X,
                    position.Y + _size.Y);
            }
        }

        public virtual bool IsVisible { get; set; } = true;
        public virtual bool IsClickable { get; set; } = false;

        public UIElement(Vector2 position, Vector2 size)
        {
            _position = position;
            _size = size;
        }

        public virtual bool Press(float x, float y)
        {
            if (!IsVisible || !IsClickable) return false;

            if (IsPointInBounds(x, y))
            {
                _isPressed = true;
                OnPressed?.Invoke();
                return true;
            }
            return false;
        }

        public virtual bool Release(float x, float y)
        {
            if (!IsVisible || !IsClickable) return false;

            if (_isPressed)
            {
                _isPressed = false;
                if (IsPointInBounds(x, y))
                {
                    OnClicked?.Invoke();
                    OnReleazed?.Invoke();
                    return true;
                }
                OnReleazed?.Invoke();
            }
            return false;
        }

        private bool IsPointInBounds(float x, float y)
        {
            RawRectangleF bounds = BoundingRectangle;
            return x > bounds.Left && x < bounds.Right && y > bounds.Top && y < bounds.Bottom;
        }

        public void BeginDraw(DrawingContext context)
        {
            if (!IsVisible) return;

            _background?.Draw(context, BoundingRectangle);
            Draw(context);
        }

        internal abstract void Draw(DrawingContext context);
    }
}
