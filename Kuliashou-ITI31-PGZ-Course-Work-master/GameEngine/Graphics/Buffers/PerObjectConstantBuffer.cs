using SharpDX;
using SharpDX.Direct3D11;
using System.Runtime.InteropServices;

namespace GameEngine.Graphics.Buffers
{
    [StructLayout(LayoutKind.Sequential)]
    public struct PerObjectData
    {
        public Matrix worldViewProjectionMatrix;
        public Matrix worldMatrix;
        public Matrix inverseTransposeWorldMatrix;
        public int timeScaling;
        public Vector3 _padding;
    }

    public class PerObjectConstantBuffer : ConstantBuffer<PerObjectData>
    {
        public PerObjectConstantBuffer(Device device, DeviceContext deviceContext, 
            CommonShaderStage commonShaderStage, int subresource, int slot) 
            : base(device, deviceContext, commonShaderStage, subresource, slot)
        {

        }

        public void Update(Matrix world, Matrix view, Matrix projection, int timeScaling)
        {
            _data.worldViewProjectionMatrix = Matrix.Multiply(Matrix.Multiply(world, view), projection);
            _data.worldViewProjectionMatrix.Transpose();

            _data.worldMatrix = world;
            _data.worldMatrix.Transpose();

            _data.inverseTransposeWorldMatrix = Matrix.Invert(world);
            _data.timeScaling = timeScaling;

            base.Update();
        }
    }
}
