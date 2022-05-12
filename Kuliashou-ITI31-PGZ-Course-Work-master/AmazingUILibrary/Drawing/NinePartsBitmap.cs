using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;
using System;

namespace AmazingUILibrary.Drawing
{
    public class NinePartsBitmap : IDisposable
    {
        private Bitmap _topLeftCorner;
        private Bitmap _topRightCorner;
        private Bitmap _bottomLeftCorner;
        private Bitmap _bottomRightCorner;

        private Bitmap _leftSide;
        private Bitmap _rightSide;
        private Bitmap _topSide;
        private Bitmap _bottomSide;

        private Bitmap _center;

        private Size2 _topLeftCornerSize;
        private Size2 _topRightCornerSize;
        private Size2 _bottomLeftCornerSize;
        private Size2 _bottomRightCornerSize;

        private Size2 _leftSideSize;
        private Size2 _rightSideSize;
        private Size2 _topSideSize;
        private Size2 _bottomSideSize;

        private Size2 _centerSize;

        public NinePartsBitmap(RenderTarget target, Bitmap source, int left, int right, int top, int bottom)
        {
            BitmapProperties bitmapProperties = new BitmapProperties()
            {
                PixelFormat = new PixelFormat(SharpDX.DXGI.Format.R8G8B8A8_UNorm, AlphaMode.Premultiplied),
            };

            Size2 sourceSize = source.PixelSize;

            _topLeftCornerSize = new Size2(left, top);
            _topRightCornerSize = new Size2(sourceSize.Width - right, top);
            _bottomLeftCornerSize = new Size2(left, sourceSize.Height - bottom);
            _bottomRightCornerSize = new Size2(sourceSize.Width - right, sourceSize.Height - bottom);

            _leftSideSize = new Size2(left, bottom - top);
            _rightSideSize = new Size2(sourceSize.Width - right, bottom - top);
            _topSideSize = new Size2(right - left, top);
            _bottomSideSize = new Size2(right - left, sourceSize.Height - bottom);

            _centerSize = new Size2(right - left, bottom - top);

            _topLeftCorner = new Bitmap(target, _topLeftCornerSize, bitmapProperties);
            _topRightCorner = new Bitmap(target, _topRightCornerSize, bitmapProperties);
            _bottomLeftCorner = new Bitmap(target, _bottomLeftCornerSize, bitmapProperties);
            _bottomRightCorner = new Bitmap(target, _bottomRightCornerSize, bitmapProperties);
            _leftSide = new Bitmap(target, _leftSideSize, bitmapProperties);
            _rightSide = new Bitmap(target, _rightSideSize, bitmapProperties);
            _topSide = new Bitmap(target, _topSideSize, bitmapProperties);
            _bottomSide = new Bitmap(target, _bottomSideSize, bitmapProperties);
            _center = new Bitmap(target, _centerSize, bitmapProperties);

            _topLeftCorner.CopyFromBitmap(source, new RawPoint(0, 0), new RawRectangle(0, 0, left, top));
            _topRightCorner.CopyFromBitmap(source, new RawPoint(0, 0), new RawRectangle(right, 0, sourceSize.Width, top));
            _bottomLeftCorner.CopyFromBitmap(source, new RawPoint(0, 0), new RawRectangle(0, bottom, left, sourceSize.Height));
            _bottomRightCorner.CopyFromBitmap(source, new RawPoint(0, 0), new RawRectangle(right, bottom, sourceSize.Width, sourceSize.Height));

            _leftSide.CopyFromBitmap(source, new RawPoint(0, 0), new RawRectangle(0, top, left, bottom));
            _rightSide.CopyFromBitmap(source, new RawPoint(0, 0), new RawRectangle(right, top, sourceSize.Width, bottom));
            _topSide.CopyFromBitmap(source, new RawPoint(0, 0), new RawRectangle(left, 0, right, top));
            _bottomSide.CopyFromBitmap(source, new RawPoint(0, 0), new RawRectangle(left, bottom, right, sourceSize.Height));

            _center.CopyFromBitmap(source, new RawPoint(0, 0), new RawRectangle(left, top, right, bottom));
        }

        public void Draw(RenderTarget target, RawRectangleF bounds)
        {
            DrawCenterScaled(target, bounds);
            DrawSidesScaled(target, bounds);
            DrawCorners(target, bounds);
        }

        private void DrawCenterScaled(RenderTarget target, RawRectangleF bounds)
        {
            float left = bounds.Left + _topLeftCornerSize.Width;
            float top = bounds.Top + _topLeftCornerSize.Height;

            float right = bounds.Right - _rightSideSize.Width;
            float bottom = bounds.Bottom - _bottomSideSize.Height;

            target.DrawBitmap(_center, new RawRectangleF(left, top, right, bottom), 1, BitmapInterpolationMode.Linear);
        }

        private void DrawSidesScaled(RenderTarget target, RawRectangleF bounds)
        {
            float rightStart = bounds.Right - _rightSideSize.Width;
            float leftEnd = bounds.Left + _leftSideSize.Width;
            float top = bounds.Top + _topSideSize.Height;
            float bottom = bounds.Bottom - _bottomSideSize.Height;

            target.DrawBitmap(_leftSide, new RawRectangleF(bounds.Left, top, leftEnd, bottom), 1, BitmapInterpolationMode.Linear);
            target.DrawBitmap(_rightSide, new RawRectangleF(rightStart, top, bounds.Right, bottom), 1, BitmapInterpolationMode.Linear);

            float bottomStart = bounds.Bottom - _bottomSideSize.Height;
            float topEnd = bounds.Top + _topSideSize.Height;
            float left = bounds.Left + _leftSideSize.Width;
            float right = bounds.Right - _rightSideSize.Width;

            target.DrawBitmap(_topSide, new RawRectangleF(left, bounds.Top, right, topEnd), 1, BitmapInterpolationMode.Linear);
            target.DrawBitmap(_bottomSide, new RawRectangleF(left, bottomStart, right, bounds.Bottom), 1, BitmapInterpolationMode.Linear);
        }

        private void DrawCorners(RenderTarget target, RawRectangleF bounds)
        {
            target.DrawBitmap(_topLeftCorner, new RawRectangleF(
                bounds.Left,
                bounds.Top,
                bounds.Left + _topLeftCornerSize.Width,
                bounds.Top + _topLeftCornerSize.Height),
                1, BitmapInterpolationMode.Linear);
            target.DrawBitmap(_topRightCorner, new RawRectangleF(
                bounds.Right - _topRightCornerSize.Width,
                bounds.Top,
                bounds.Right,
                bounds.Top + _topRightCornerSize.Height),
                1, BitmapInterpolationMode.Linear);
            target.DrawBitmap(_bottomLeftCorner, new RawRectangleF(
                bounds.Left,
                bounds.Bottom - _bottomLeftCornerSize.Height,
                bounds.Left + _bottomLeftCornerSize.Width,
                bounds.Bottom),
                1, BitmapInterpolationMode.Linear);
            target.DrawBitmap(_bottomRightCorner, new RawRectangleF(
                bounds.Right - _bottomRightCornerSize.Width,
                bounds.Bottom - _bottomRightCornerSize.Height,
                bounds.Right,
                bounds.Bottom),
                1, BitmapInterpolationMode.Linear);
        }

        public void Dispose()
        {
            _topLeftCorner.Dispose();
            _topRightCorner.Dispose();
            _bottomLeftCorner.Dispose();
            _bottomRightCorner.Dispose();
            _leftSide.Dispose();
            _rightSide.Dispose();
            _topSide.Dispose();
            _bottomSide.Dispose();
            _center.Dispose();
        }
    }
}
