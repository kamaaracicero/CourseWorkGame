using SharpDX;
using SharpDX.Direct3D11;
using System.Runtime.InteropServices;

namespace GameEngine.Graphics.Buffers
{
    [StructLayout(LayoutKind.Sequential)]
    public struct PerFrameData
    {
        public float time;
        public Vector3 _padding;
    }

    public class PerFrameConstantBuffer : ConstantBuffer<PerFrameData>
    {
        public PerFrameConstantBuffer(Device device, DeviceContext deviceContext, 
            CommonShaderStage commonShaderStage, int subresource, int slot) 
            : base(device, deviceContext, commonShaderStage, subresource, slot)
        {
        }

        public void Update(float time)
        {
            _data.time = time;
            base.Update();
        }
    }
}
