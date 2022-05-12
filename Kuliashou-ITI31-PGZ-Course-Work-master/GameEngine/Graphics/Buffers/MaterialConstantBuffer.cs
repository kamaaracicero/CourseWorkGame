using SharpDX;
using SharpDX.Direct3D11;
using System.Runtime.InteropServices;

namespace GameEngine.Graphics.Buffers
{
    [StructLayout(LayoutKind.Sequential)]
    public struct MaterialProperties
    {
        public Vector3 emmisiveK;
        public float _padding0;
        public Vector3 ambientK;
        public float _padding1;
        public Vector3 diffuseK;
        public float _padding2;
        public Vector3 specularK;
        public float specularPower;
    }

    public class MaterialConstantBuffer : ConstantBuffer<MaterialProperties>
    {
        public MaterialConstantBuffer(Device device, DeviceContext deviceContext, CommonShaderStage commonShaderStage,
            int subresource, int slot) : base(device, deviceContext, commonShaderStage, subresource, slot)
        {
        }

        public void Update(MaterialProperties materialProperties)
        {
            _data = materialProperties;

            base.Update();
        }
    }
}
