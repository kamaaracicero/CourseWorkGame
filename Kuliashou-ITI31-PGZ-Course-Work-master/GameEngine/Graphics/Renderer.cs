using System;
using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.DXGI;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using Device11 = SharpDX.Direct3D11.Device;
using AmazingUILibrary;
using GameEngine.Graphics.Buffers;

namespace GameEngine.Graphics
{
    public partial class Renderer : IDisposable
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct VertexDataStruct
        {
            public Vector4 position;
            public Vector4 normal;
            public Vector4 color;
            public Vector2 texCoord;
        }

        private DirectX3DGraphics _directX3DGraphics;
        private DirectX2DGraphics _directX2DGraphics;

        private Device11 _device;
        private DeviceContext _deviceContext;

        private VertexShader _vertexShader;
        private PixelShader _pixelShader;
        private ShaderSignature _shaderSignature;
        private InputLayout _inputLayout;

        private PerFrameConstantBuffer _perFrameConstantBuffer;
        private PerObjectConstantBuffer _perObjectConstantBuffer;
        private MaterialConstantBuffer _materialConstantBuffer;

        private Material _currentMaterial = null;
        private Texture _currentTexture = null;


        private SamplerState _anisotropicSampler;
        public SamplerState AnisotropicSampler { get => _anisotropicSampler; }

        private SamplerState _linearSampler;
        public SamplerState LinearSampler { get => _linearSampler; }

        private SamplerState _pointSampler;
        public SamplerState PointSampler { get => _pointSampler; }

        private ClassLinkage _pixelShaderClassLinkage;
        private ClassLinkage _vertexShaderClassLinkage;

        public Renderer(DirectX3DGraphics directX3DGraphics, DirectX2DGraphics directX2DGraphics)
        {
            _directX3DGraphics = directX3DGraphics;
            _directX2DGraphics = directX2DGraphics;
            _device = _directX3DGraphics.Device;
            _deviceContext = _directX3DGraphics.DeviceContext;

            _pixelShaderClassLinkage = new ClassLinkage(_device);
            _vertexShaderClassLinkage = new ClassLinkage(_device);

            CompilationResult vertexShaderByteCode =
                ShaderBytecode.CompileFromFile("Shaders\\vertex.hlsl", "vertexShader", "vs_5_0", ShaderFlags.None, EffectFlags.None, null, new IncludeHandler());

            _vertexShader = new VertexShader(_device, vertexShaderByteCode, _vertexShaderClassLinkage);
            Utilities.Dispose(ref _vertexShaderClassLinkage);

            CompilationResult pixelShaderByteCode =
                ShaderBytecode.CompileFromFile("Shaders\\pixel.hlsl", "pixelShader", "ps_5_0", ShaderFlags.None, EffectFlags.None, null, new IncludeHandler());

            _pixelShader = new PixelShader(_device, pixelShaderByteCode, _pixelShaderClassLinkage);

            InitializeIllumination(pixelShaderByteCode);


            InputElement[] inputElements = new[]
            {
                new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 0),
                new InputElement("NORMAL", 0, Format.R32G32B32A32_Float, 16, 0),
                new InputElement("COLOR", 0, Format.R32G32B32A32_Float, 32, 0),
                new InputElement("TEXCOORD", 0, Format.R32G32_Float, 48, 0)
            };

            _shaderSignature = ShaderSignature.GetInputSignature(vertexShaderByteCode);

            _inputLayout = new InputLayout(_device, _shaderSignature, inputElements);

            Utilities.Dispose(ref vertexShaderByteCode);
            Utilities.Dispose(ref pixelShaderByteCode);

            _deviceContext.InputAssembler.InputLayout = _inputLayout;

            RasterizerStateDescription _rasterizerStateDescription = new RasterizerStateDescription()
            {
                FillMode = FillMode.Solid,
                CullMode = CullMode.Back,
                IsFrontCounterClockwise = true,
                IsMultisampleEnabled = true,
                IsAntialiasedLineEnabled = true,
                IsDepthClipEnabled = true

            };

            _deviceContext.Rasterizer.State = new RasterizerState(_device, _rasterizerStateDescription);

            SamplerStateDescription samplerStateDescription;

            samplerStateDescription =
                new SamplerStateDescription
                {
                    Filter = Filter.Anisotropic,
                    AddressU = TextureAddressMode.Clamp,
                    AddressV = TextureAddressMode.Clamp,
                    AddressW = TextureAddressMode.Clamp,
                    MipLodBias = 0,
                    MaximumAnisotropy = 16,
                    ComparisonFunction = Comparison.Never,
                    BorderColor = new SharpDX.Mathematics.Interop.RawColor4(1.0f, 1.0f, 1.0f, 1.0f),
                    MinimumLod = 0,
                    MaximumLod = float.MaxValue
                };

            _anisotropicSampler = new SamplerState(_directX3DGraphics.Device, samplerStateDescription);

            samplerStateDescription.Filter = Filter.MinMagMipLinear;
            _linearSampler = new SamplerState(_directX3DGraphics.Device, samplerStateDescription);

            samplerStateDescription.Filter = Filter.MinMagMipLinear;
            _pointSampler = new SamplerState(_directX3DGraphics.Device, samplerStateDescription);
        }

        public void CreateConstantBuffers()
        {
            _perFrameConstantBuffer = new PerFrameConstantBuffer(_device, _deviceContext, _deviceContext.VertexShader, 0, 0);
            _perObjectConstantBuffer = new PerObjectConstantBuffer(_device, _deviceContext, _deviceContext.VertexShader, 0, 1);
            _materialConstantBuffer = new MaterialConstantBuffer(_device, _deviceContext, _deviceContext.PixelShader, 0, 0);

            CreateIlluminationConstantBuffer();
        }

        public void BeginRender()
        {
            _directX3DGraphics.ClearBuffers(Color.DarkGray);
        }

        public void RenderUserInterface(UIElement ui)
        {
            _directX2DGraphics.BeginDraw();
            ui.BeginDraw(_directX2DGraphics.DrawingContext);
            _directX2DGraphics.EndDraw();
        }

        public void UpdatePerFrameConstantBuffers(float time)
        {
            _perFrameConstantBuffer.Update(time);
        }

        public void UpdatePerObjectConstantBuffers(Matrix world, Matrix view, Matrix projection, int timeScaling)
        {
            _perObjectConstantBuffer.Update(world, view, projection, timeScaling);
        }

        public void SetTexture(Texture texture)
        {
            if (_currentTexture != texture && texture != null)
            {
                _deviceContext.PixelShader.SetShaderResource(0, texture.ShaderResourceView);
                _deviceContext.PixelShader.SetSampler(0, texture.SamplerState);

                _currentTexture = texture;
            }
        }

        public void SetMaterial(Material material)
        {
            if (_currentMaterial != material)
            {
                SetTexture(material.Texture);

                _currentMaterial = material;
                _materialConstantBuffer.Update(material.MaterialProperties);
            }
        }

        public void RenderMeshObject(MeshObject meshObject)
        {
            SetMaterial(meshObject.Material);

            _deviceContext.InputAssembler.PrimitiveTopology = meshObject.PrimitiveTopology;

            _deviceContext.InputAssembler.SetVertexBuffers(0, meshObject.VertexBufferBinding);
            _deviceContext.InputAssembler.SetIndexBuffer(meshObject.IndicesBufferObject, Format.R32_UInt, 0);

            _deviceContext.VertexShader.Set(_vertexShader);
            _deviceContext.PixelShader.Set(_pixelShader, _lightInterfaces);

            _deviceContext.DrawIndexed(meshObject.IndicesCount, 0, 0);
        }


        public void EndRender()
        {
            _directX3DGraphics.SwapChain.Present(1, PresentFlags.Restart);
        }


        public void Dispose()
        {
            _perFrameConstantBuffer.Dispose();
            _perObjectConstantBuffer.Dispose();
            _materialConstantBuffer.Dispose();
            Utilities.Dispose(ref _linearSampler);
            Utilities.Dispose(ref _anisotropicSampler);
            Utilities.Dispose(ref _inputLayout);
            Utilities.Dispose(ref _shaderSignature);
            DisposeIllumination();
            Utilities.Dispose(ref _pixelShader);
            Utilities.Dispose(ref _vertexShader);
            Utilities.Dispose(ref _pixelShaderClassLinkage);
        }
    }
}
