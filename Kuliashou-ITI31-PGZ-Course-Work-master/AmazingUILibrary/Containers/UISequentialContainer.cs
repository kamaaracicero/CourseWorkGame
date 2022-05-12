using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmazingUILibrary.Containers
{
    public class UISequentialContainer : UIMultiElementsContainer
    {
        public enum Alignment
        {
            Start,
            Center,
            End
        }

        private bool _isVertical;
        private float _mainAxisSize = 0f;
        private Alignment _mainAxis;
        private Alignment _crossAxis;
        private readonly Dictionary<Alignment, Func<UIElement, int, float>> _alignmentsX;
        private readonly Dictionary<Alignment, Func<UIElement, int, float>> _alignmentsY;

        public bool IsVertical
        {
            get => _isVertical;
            set
            {
                _isVertical = value;
                RecalculateMainAxisSize();
                RecalculateElementsPosition();
            }
        }

        public Alignment MainAxis
        {
            get => _mainAxis;
            set
            {
                _mainAxis = value;
                RecalculateElementsPosition();
            }
        }

        public Alignment CrossAxis
        {
            get => _crossAxis;
            set
            {
                _crossAxis = value;
                RecalculateElementsPosition();
            }
        }

        public override Vector2 Size
        {
            get => base.Size;
            set
            {
                base.Size = value;
                RecalculateElementsPosition();
            }
        }

        public override Vector2 LocalPosition
        {
            get => base.LocalPosition;
            set
            {
                base.LocalPosition = value;
                RecalculateElementsPosition();
            }
        }


        public UISequentialContainer(Vector2 position, Vector2 size, bool isVertical = true)
            : base(position, size)
        {
            _isVertical = isVertical;

            _mainAxis = Alignment.Start;
            _crossAxis = Alignment.Start;

            _alignmentsX = new Dictionary<Alignment, Func<UIElement, int, float>>
            {
                { Alignment.Start, (UIElement element, int index) => GetSizeX(index) },
                { Alignment.Center, (UIElement element, int index) => GetStartX(element) / 2f + GetSizeX(index) },
                { Alignment.End, (UIElement element, int index) => GetStartX(element) + GetSizeX(index) },
            };
            _alignmentsY = new Dictionary<Alignment, Func<UIElement, int, float>>
            {
                { Alignment.Start, (UIElement element, int index) => GetSizeY(index) },
                { Alignment.Center, (UIElement element, int index) => GetStartY(element) / 2f + GetSizeY(index) },
                { Alignment.End, (UIElement element, int index) => GetStartY(element) + GetSizeY(index) },
            };
        }

        public override void Add(UIElement element)
        {
            base.Add(element);

            _mainAxisSize += _isVertical ? element.Size.Y : element.Size.X;
            element.OnResized += OnChildResize;

            RecalculateElementsPosition();
        }

        public override void Remove(UIElement element)
        {
            base.Remove(element);

            _mainAxisSize -= _isVertical ? element.Size.Y : element.Size.X;
            element.OnResized -= OnChildResize;

            RecalculateElementsPosition();

        }

        private void RecalculateElementsPosition()
        {
            for (int i = 0; i < Elements.Count; i++)
            {
                CalculatePosition(Elements[i], i);
            }
        }

        private void CalculatePosition(UIElement element, int index)
        {
            element.LocalPosition = new Vector2(
                _alignmentsX[GetAlignmentX()](element, index),
                _alignmentsY[GetAlignmentY()](element, index));
        }

        private Alignment GetAlignmentX()
        {
            return _isVertical ? _crossAxis : _mainAxis;
        }

        private Alignment GetAlignmentY()
        {
            return _isVertical ? _mainAxis : _crossAxis;
        }

        private float GetStartX(UIElement element)
        {
            return _isVertical ? Size.X - element.Size.X : Size.X - _mainAxisSize;
        }

        private float GetStartY(UIElement element)
        {
            return _isVertical ? Size.Y - _mainAxisSize : Size.Y - element.Size.Y;
        }

        private float GetSizeX(int index)
        {
            return _isVertical ? 0 : Elements.GetRange(0, index).Sum(el => el.Size.X);
        }

        private float GetSizeY(int index)
        {
            return _isVertical ? Elements.GetRange(0, index).Sum(el => el.Size.Y) : 0;
        }

        private void RecalculateMainAxisSize()
        {
            _mainAxisSize = _isVertical ?
                Elements.Sum(element => element.Size.Y) :
                Elements.Sum(element => element.Size.X);
        }

        private void OnChildResize()
        {
            RecalculateMainAxisSize();
            RecalculateElementsPosition();
        }
    }
}
