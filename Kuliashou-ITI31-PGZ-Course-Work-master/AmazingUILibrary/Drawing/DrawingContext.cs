using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using SharpDX.Mathematics.Interop;
using SharpDX.WIC;
using System;
using System.Collections.Generic;
using System.Linq;
using WriteFactory = SharpDX.DirectWrite.Factory;
using Direct2DBitmap = SharpDX.Direct2D1.Bitmap;
using Direct2DBitmapInterpolationMode = SharpDX.Direct2D1.BitmapInterpolationMode;
using WicPixelFormat = SharpDX.WIC.PixelFormat;

namespace AmazingUILibrary.Drawing
{
    public class DrawingContext : IDisposable
    {
        private const string DEFAULT_FONT_STYLE = "Arial";
        private const float DEFAULT_FONT_SIZE = 16f;

        private RenderTarget _renderTarget;

        private WriteFactory _writeFactory;
        private ImagingFactory _imagingFactory;

        private Dictionary<string, TextFormat> _textFormats;
        private Dictionary<string, RawColor4> _solidColorBrushColors;
        private Dictionary<string, SolidColorBrush> _solidColorBrushes;

        private Dictionary<string, BitmapFrameDecode> _decodedBitmaps;
        private Dictionary<string, Direct2DBitmap> _bitmaps;

        private Dictionary<string, DecodedNinePartsBitmap> _decodedNinePartsBitmaps;
        private Dictionary<string, NinePartsBitmap> _ninePartsBitmaps;


        public DrawingContext()
        {
            _writeFactory = new WriteFactory();
            _imagingFactory = new ImagingFactory();

            _textFormats = new Dictionary<string, TextFormat>();
            _solidColorBrushColors = new Dictionary<string, RawColor4>();
            _solidColorBrushes = new Dictionary<string, SolidColorBrush>();

            _decodedBitmaps = new Dictionary<string, BitmapFrameDecode>();
            _bitmaps = new Dictionary<string, Direct2DBitmap>();

            _decodedNinePartsBitmaps = new Dictionary<string, DecodedNinePartsBitmap>();
            _ninePartsBitmaps = new Dictionary<string, NinePartsBitmap>();
        }


        public void DrawText(string text, string format, RawRectangleF bounds, string brush)
        {
            _renderTarget.DrawText(text, _textFormats[format], bounds, _solidColorBrushes[brush]);
        }

        public void DrawBitmap(string bitmapName, RawRectangleF bounds, float opacity = 1f,
            Direct2DBitmapInterpolationMode interpolationMode = Direct2DBitmapInterpolationMode.Linear)
        {
            _renderTarget.DrawBitmap(_bitmaps[bitmapName], bounds, opacity, interpolationMode);
        }

        internal void DrawRectangle(RawRectangleF bounds, string brush)
        {
            _renderTarget.FillRectangle(bounds, _solidColorBrushes[brush]);
        }

        internal void DrawNinePartsBitmap(string bitmap, RawRectangleF bounds)
        {
            _ninePartsBitmaps[bitmap].Draw(_renderTarget, bounds);
        }


        public void NewTextFormat(string formatName, 
            string fontFamilyName = DEFAULT_FONT_STYLE,
            FontWeight fontWeight = FontWeight.Normal,
            FontStyle fontStyle = FontStyle.Normal, 
            FontStretch fontStretch = FontStretch.Normal, 
            float fontSize = DEFAULT_FONT_SIZE,
            TextAlignment textAlignment = TextAlignment.Leading, 
            ParagraphAlignment paragraphAlignment = ParagraphAlignment.Near)
        {
            if (_textFormats.ContainsKey(formatName)) return;
            TextFormat textFormat = new TextFormat(_writeFactory, fontFamilyName, fontWeight,
                fontStyle, fontStretch, fontSize);
            textFormat.TextAlignment = textAlignment;
            textFormat.ParagraphAlignment = paragraphAlignment;
            _textFormats.Add(formatName, textFormat);
        }

        public void NewNinePartsBitmap(string bitmapName, BitmapFrameDecode bitmapFrame, int left, int right, int top, int bottom)
        {
            if (_decodedNinePartsBitmaps.ContainsKey(bitmapName)) return;
            _decodedNinePartsBitmaps.Add(bitmapName, new DecodedNinePartsBitmap(bitmapFrame, left, right, top, bottom));
        }

        public void NewSolidBrush(string brushName, RawColor4 color)
        {
            if (_solidColorBrushColors.ContainsKey(brushName)) return;
            _solidColorBrushColors.Add(brushName, color);
        }

        public void NewBitmap(string bitmapName, BitmapFrameDecode bitmapFrame)
        {
            if (_decodedBitmaps.ContainsKey(bitmapName)) return;
            _decodedBitmaps.Add(bitmapName, bitmapFrame);
        }


        private void CreateAndAddSolidBrush(string brushName)
        {
            SolidColorBrush brush = new SolidColorBrush(_renderTarget, _solidColorBrushColors[brushName]);
            _solidColorBrushes.Add(brushName, brush);
        }

        private void CreateAndAddBitmap(string bitmapName)
        {
            _bitmaps.Add(bitmapName, CreateBitmap(_decodedBitmaps[bitmapName]));
        }

        private void CreateAndAddNinePartsBitmap(string bitmapName)
        {
            DecodedNinePartsBitmap decoded = _decodedNinePartsBitmaps[bitmapName];
            Direct2DBitmap bitmap = CreateBitmap(decoded.Bitmap);

            _ninePartsBitmaps.Add(bitmapName, new NinePartsBitmap(_renderTarget, bitmap, decoded.Left, decoded.Right, decoded.Top, decoded.Bottom));
        }

        private Direct2DBitmap CreateBitmap(BitmapFrameDecode bitmapFrameDecode)
        {
            FormatConverter imageFormatConverter = new FormatConverter(_imagingFactory);
            imageFormatConverter.Initialize(
                bitmapFrameDecode,
                WicPixelFormat.Format32bppPRGBA,
                BitmapDitherType.Ordered4x4, null, 0.0, BitmapPaletteType.Custom);
            Direct2DBitmap bitmap = Direct2DBitmap.FromWicBitmap(_renderTarget, imageFormatConverter);

            Utilities.Dispose(ref imageFormatConverter);

            return bitmap;
        }


        public void OnResizing()
        {
            DisposeDictionaryElements(_bitmaps);
            DisposeDictionaryElements(_ninePartsBitmaps);
            DisposeDictionaryElements(_solidColorBrushes);
        }

        public void OnResized(RenderTarget renderTarget)
        {
            _renderTarget = renderTarget;

            string[] brushNames = _solidColorBrushColors.Keys.ToArray();
            for (int i = 0; i <= brushNames.Count() - 1; ++i)
                CreateAndAddSolidBrush(brushNames[i]);

            string[] bitmapNames = _decodedBitmaps.Keys.ToArray();
            for (int i = 0; i <= bitmapNames.Count() - 1; ++i)
                CreateAndAddBitmap(bitmapNames[i]);

            string[] ninePartsBitmapsNames = _decodedNinePartsBitmaps.Keys.ToArray();
            for (int i = 0; i <= ninePartsBitmapsNames.Count() - 1; ++i)
                CreateAndAddNinePartsBitmap(ninePartsBitmapsNames[i]);
        }

        public void Dispose()
        {
            DisposeDictionaryElements(_bitmaps);
            DisposeDictionaryElements(_ninePartsBitmaps);
            DisposeDictionaryElements(_decodedBitmaps);
            DisposeDictionaryElements(_decodedNinePartsBitmaps);
            DisposeDictionaryElements(_solidColorBrushes);
            DisposeDictionaryElements(_textFormats);
            _writeFactory.Dispose();
            _imagingFactory.Dispose();
        }

        public static void DisposeDictionaryElements<T>(Dictionary<string, T> dictionary) where T : class, IDisposable
        {
            for (int i = dictionary.Count - 1; i >= 0; --i)
            {
                KeyValuePair<string, T> nameValuePair = dictionary.ElementAt(i);
                dictionary.Remove(nameValuePair.Key);
                T value = nameValuePair.Value;
                Utilities.Dispose(ref value);
            }
        }
    }
}
