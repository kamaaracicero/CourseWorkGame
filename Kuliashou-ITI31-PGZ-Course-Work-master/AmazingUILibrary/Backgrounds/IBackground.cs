using AmazingUILibrary.Drawing;
using SharpDX.Mathematics.Interop;
using System;

namespace AmazingUILibrary.Backgrounds
{
    public interface IBackground
    {
        void Draw(DrawingContext context, RawRectangleF bounds);
    }
}
