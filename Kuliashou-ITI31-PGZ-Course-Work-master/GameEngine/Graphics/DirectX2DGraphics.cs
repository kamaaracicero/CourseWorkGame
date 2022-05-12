using System;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using Direct2DFactory = SharpDX.Direct2D1.Factory;
using Direct2DPixelFormat = SharpDX.Direct2D1.PixelFormat;
using Direct2DAlphaMode = SharpDX.Direct2D1.AlphaMode;
using Direct2DTextAntialiasMode = SharpDX.Direct2D1.TextAntialiasMode;
using AmazingUILibrary.Drawing;

namespace GameEngine.Graphics
{
    public class DirectX2DGraphics : IDisposable
    {
        private DirectX3DGraphics _directX3DGraphics;

        private Direct2DFactory _factory;

        private RenderTargetProperties _renderTargetProperties;
        private RenderTarget _renderTarget;
        private RawRectangleF _renderTargetClientRectangle;
        public RawRectangleF RenderTargetClientRectangle { get => _renderTargetClientRectangle; }

        private DrawingContext _drawingContext;
        public DrawingContext DrawingContext { get => _drawingContext; }

        public DirectX2DGraphics(Game.Game game, DirectX3DGraphics directX3DGraphics)
        {
            game.SwapChainResizing += Game_SwapChainResizing;
            game.SwapChainResized += Game_SwapChainResized;

            _directX3DGraphics = directX3DGraphics;

            _factory = new Direct2DFactory();

            _renderTargetProperties.DpiX = 0;
            _renderTargetProperties.DpiY = 0;
            _renderTargetProperties.MinLevel = FeatureLevel.Level_10;
            _renderTargetProperties.PixelFormat = new Direct2DPixelFormat(
                Format.Unknown,
                Direct2DAlphaMode.Premultiplied);
            _renderTargetProperties.Type = RenderTargetType.Hardware;
            _renderTargetProperties.Usage = RenderTargetUsage.None;

            _drawingContext = new DrawingContext();
        }

        private void Game_SwapChainResizing(object sender, EventArgs e)
        {
            _drawingContext.OnResizing();
            Utilities.Dispose(ref _renderTarget);
        }

        private void Game_SwapChainResized(object sender, EventArgs e)
        {
            Surface surface = _directX3DGraphics.BackBuffer.QueryInterface<Surface>();
            _renderTarget = new RenderTarget(_factory, surface, _renderTargetProperties);
            Utilities.Dispose(ref surface);

            _renderTarget.AntialiasMode = AntialiasMode.PerPrimitive;
            _renderTarget.TextAntialiasMode = Direct2DTextAntialiasMode.Cleartype;
            _renderTargetClientRectangle.Left = 0;
            _renderTargetClientRectangle.Top = 0;
            _renderTargetClientRectangle.Right = _renderTarget.Size.Width;
            _renderTargetClientRectangle.Bottom = _renderTarget.Size.Height;

            _drawingContext.OnResized(_renderTarget);
        }

        public void BeginDraw()
        {
            _renderTarget.BeginDraw();
        }

        public void EndDraw()
        {
            _renderTarget.EndDraw();
        }

        public void Dispose()
        {
            _drawingContext.Dispose();
            Utilities.Dispose(ref _renderTarget);
            Utilities.Dispose(ref _factory);
        }
    }
}
