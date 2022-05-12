using System;
using SharpDX;
using SharpDX.Direct3D11;
using Buffer11 = SharpDX.Direct3D11.Buffer;

namespace GameEngine.Graphics.Buffers
{
    public class ConstantBuffer<T> : IDisposable where T : struct
    {
        protected T _data;

        private DeviceContext _deviceContext;
        private CommonShaderStage _commonShaderStage;

        private Buffer11 _buffer;

        private int _subresource;
        private int _slot;

        public ConstantBuffer(Device device, DeviceContext deviceContext, CommonShaderStage commonShaderStage,
            int subresource, int slot)
        {
            _deviceContext = deviceContext;
            _buffer = new Buffer11(
                device,
                Utilities.SizeOf<T>(),
                ResourceUsage.Dynamic,
                BindFlags.ConstantBuffer,
                CpuAccessFlags.Write,
                ResourceOptionFlags.None,
                0);
            _commonShaderStage = commonShaderStage;
            _subresource = subresource;
            _slot = slot;
        }

        protected void Update()
        {
            DataStream dataStream;
            _deviceContext.MapSubresource(_buffer, MapMode.WriteDiscard, MapFlags.None,
                out dataStream);
            dataStream.Write(_data);
            _deviceContext.UnmapSubresource(_buffer, _subresource);
            _commonShaderStage.SetConstantBuffer(_slot, _buffer);

        }

        public void Dispose()
        {
            Utilities.Dispose(ref _buffer);
        }
    }
}
