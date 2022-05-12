using SharpDX.WIC;
using System;

namespace AmazingUILibrary.Drawing
{
    public class DecodedNinePartsBitmap : IDisposable
    {
        public BitmapFrameDecode Bitmap { get; }
        public int Left { get; }
        public int Right { get; }
        public int Top { get; }
        public int Bottom { get; }

        public DecodedNinePartsBitmap(BitmapFrameDecode bitmap, int left, int right, int top, int bottom)
        {
            Bitmap = bitmap;
            Left = left;
            Right = right;
            Top = top;
            Bottom = bottom;
        }

        public void Dispose()
        {
            Bitmap.Dispose();
        }
    }
}
