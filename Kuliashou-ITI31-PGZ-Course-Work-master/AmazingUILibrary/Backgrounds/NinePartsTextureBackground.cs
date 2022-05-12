using AmazingUILibrary.Drawing;
using SharpDX.Mathematics.Interop;

namespace AmazingUILibrary.Backgrounds
{
    public class NinePartsTextureBackground : IBackground
    {
        private string _bitmap;

        public NinePartsTextureBackground(string bitmap)
        {
            _bitmap = bitmap;
        }

        public void Draw(DrawingContext context, RawRectangleF bounds)
        {
            context.DrawNinePartsBitmap(_bitmap, bounds);
        }
    }
}
