using System;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Windows;
using Device11 = SharpDX.Direct3D11.Device;

namespace GameEngine.Graphics
{
    public class DirectX3DGraphics : IDisposable
    {
        private static DirectX3DGraphics _directX3DGraphics;

        private RenderForm _renderForm;
        public RenderForm RenderForm { get => _renderForm; }

        private SampleDescription _sampleDescription;
        public SampleDescription SampleDescription { get => _sampleDescription; }

        private SwapChainDescription _swapChainDescription;

        private Device11 _device;
        public Device11 Device { get => _device; }

        private SharpDX.DXGI.Device _deviceDXGI;
        private SharpDX.Direct2D1.Device _device2D;
        public SharpDX.Direct2D1.Device Device2D { get => _device2D; }


        private SwapChain _swapChain;
        public SwapChain SwapChain { get => _swapChain; }

        private DeviceContext _deviceContext;
        public DeviceContext DeviceContext { get => _deviceContext; }

        private RasterizerStateDescription _rasterizerStateDescription;
        private RasterizerState _rasterizerStage;

        private Factory _factory;

        private Texture2D _backBuffer;
        public Texture2D BackBuffer { get => _backBuffer; }

        private RenderTargetView _renderTargetView;

        private Texture2DDescription _depthStencilBufferDescription;

        private Texture2D _depthStencilBuffer;

        private DepthStencilView _depthStencilView;

        private bool _isFullScreen;
        public bool IsFullScreen
        {
            get { return _isFullScreen; }
            set
            {
                if (value != _isFullScreen)
                {
                    _isFullScreen = value;
                    _swapChain.SetFullscreenState(_isFullScreen, null);
                }
            }
        }

        private DirectX3DGraphics(RenderForm renderForm)
        {
            _renderForm = renderForm;

            Configuration.EnableObjectTracking = true;

            _sampleDescription = new SampleDescription(4, (int)StandardMultisampleQualityLevels.StandardMultisamplePattern);

            _swapChainDescription = new SwapChainDescription()
            {
                BufferCount = 1,
                ModeDescription = new ModeDescription(
                    _renderForm.Width, _renderForm.Height, new Rational(60, 1), Format.R8G8B8A8_UNorm
                    ),
                IsWindowed = true,
                OutputHandle = renderForm.Handle,
                SampleDescription = _sampleDescription,
                SwapEffect = SwapEffect.Discard,
                Usage = Usage.RenderTargetOutput
            };

            Device11.CreateWithSwapChain(
                DriverType.Hardware,
                DeviceCreationFlags.BgraSupport,
                _swapChainDescription,
                out _device,
                out _swapChain
                );

            _deviceDXGI = _device.QueryInterface<SharpDX.DXGI.Device>();
            _device2D = new SharpDX.Direct2D1.Device(_deviceDXGI);

            _deviceContext = _device.ImmediateContext;

            _rasterizerStateDescription = new RasterizerStateDescription()
            {
                FillMode = FillMode.Solid,
                CullMode = CullMode.Back,
                IsFrontCounterClockwise = true,
                IsMultisampleEnabled = true,
                IsAntialiasedLineEnabled = true,
                IsDepthClipEnabled = true

            };

            _rasterizerStage = new RasterizerState(_device, _rasterizerStateDescription);
            _deviceContext.Rasterizer.State = _rasterizerStage;

            _factory = _swapChain.GetParent<Factory>();

            _factory.MakeWindowAssociation(_renderForm.Handle, WindowAssociationFlags.IgnoreAll);

            _depthStencilBufferDescription = new Texture2DDescription()
            {
                Format = Format.D32_Float_S8X24_UInt,
                ArraySize = 1,
                MipLevels = 1,
                Width = _renderForm.ClientSize.Width,
                Height = _renderForm.ClientSize.Height,
                SampleDescription = _sampleDescription,
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.DepthStencil,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None
            };
        }

        public static DirectX3DGraphics GetInstance(RenderForm renderForm = null)
        {
            if (_directX3DGraphics == null)
            {
                _directX3DGraphics = new DirectX3DGraphics(renderForm);
            }
            return _directX3DGraphics;
        }

        public void Resize()
        {
            Utilities.Dispose(ref _depthStencilView);
            Utilities.Dispose(ref _depthStencilBuffer);
            Utilities.Dispose(ref _renderTargetView);
            Utilities.Dispose(ref _backBuffer);

            _swapChain.ResizeBuffers(_swapChainDescription.BufferCount,
                _renderForm.ClientSize.Width, _renderForm.ClientSize.Height,
                Format.Unknown, SwapChainFlags.None);

            _backBuffer = Texture2D.FromSwapChain<Texture2D>(_swapChain, 0);

            _renderTargetView = new RenderTargetView(_device, _backBuffer);

            _depthStencilBufferDescription.Width = _renderForm.ClientSize.Width;
            _depthStencilBufferDescription.Height = _renderForm.ClientSize.Height;
            _depthStencilBuffer = new Texture2D(_device, _depthStencilBufferDescription);

            _depthStencilView = new DepthStencilView(_device, _depthStencilBuffer);

            _deviceContext.Rasterizer.SetViewport(
                new Viewport(0, 0, _renderForm.ClientSize.Width, _renderForm.ClientSize.Height, 0f, 1f)
                );
            _deviceContext.OutputMerger.SetTargets(_depthStencilView, _renderTargetView);
        }

        public void ClearBuffers(Color backgroundColor)
        {
            _deviceContext.ClearDepthStencilView(_depthStencilView, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1f, 0);
            _deviceContext.ClearRenderTargetView(_renderTargetView, backgroundColor);
        }

        public void Dispose()
        {
            Utilities.Dispose(ref _depthStencilView);
            Utilities.Dispose(ref _depthStencilBuffer);
            Utilities.Dispose(ref _renderTargetView);
            Utilities.Dispose(ref _backBuffer);
            Utilities.Dispose(ref _factory);
            Utilities.Dispose(ref _rasterizerStage);
            Utilities.Dispose(ref _deviceContext);
            Utilities.Dispose(ref _swapChain);
            Utilities.Dispose(ref _device2D);
            Utilities.Dispose(ref _deviceDXGI);
            Utilities.Dispose(ref _device);
        }
    }
}
