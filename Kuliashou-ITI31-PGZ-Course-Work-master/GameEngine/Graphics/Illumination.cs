using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.D3DCompiler;
using Buffer11 = SharpDX.Direct3D11.Buffer;

namespace GameEngine.Graphics
{
    public partial class Renderer
    {
        public enum LightSourceType : int
        {
            Base = 0,
            Directional,
            Point,
            Spot
        }
        private const int LightSourceTypeCount = (int)LightSourceType.Spot + 1;

        [StructLayout(LayoutKind.Sequential)]
        public struct LightSource
        {
            public LightSourceType lightSourceType;
            public Vector3 color;
            public Vector3 position;
            public float _padding0;
            public Vector3 direction;
            public float _padding1;
            public float spotAngle;
            public Vector3 attenuation;
            public float constantAttenuation { get => attenuation.X; set => attenuation.X = value; }
            public float linearAttenuation { get => attenuation.Y; set => attenuation.Y = value; }
            public float quadraticAttenuation { get => attenuation.Z; set => attenuation.Z = value; }
        }

        public const int MaxLights = 8;

        [StructLayout(LayoutKind.Sequential)]
        public struct IlluminationProperties
        {
            public Vector4 eyePosition;
            public Vector3 globalAmbient;
            public float _padding;
            public LightSource lightSource0;
            public LightSource lightSource1;
            public LightSource lightSource2;
            public LightSource lightSource3;
            public LightSource lightSource4;
            public LightSource lightSource5;
            public LightSource lightSource6;
            public LightSource lightSource7;
            public LightSource this[int index]
            {
                get
                {
                    LightSource l = new LightSource();
                    switch (index)
                    {
                        case 0:
                            l = lightSource0;
                            break;
                        case 1:
                            l = lightSource1;
                            break;
                        case 2:
                            l = lightSource2;
                            break;
                        case 3:
                            l = lightSource3;
                            break;
                        case 4:
                            l = lightSource4;
                            break;
                        case 5:
                            l = lightSource5;
                            break;
                        case 6:
                            l = lightSource6;
                            break;
                        case 7:
                            l = lightSource7;
                            break;
                    }
                    return l;
                }
                set
                {
                    switch (index)
                    {
                        case 0:
                            lightSource0 = value;
                            break;
                        case 1:
                            lightSource1 = value;
                            break;
                        case 2:
                            lightSource2 = value;
                            break;
                        case 3:
                            lightSource3 = value;
                            break;
                        case 4:
                            lightSource4 = value;
                            break;
                        case 5:
                            lightSource5 = value;
                            break;
                        case 6:
                            lightSource6 = value;
                            break;
                        case 7:
                            lightSource7 = value;
                            break;
                    }
                }
            }
        }

        private string[] lightClassVariableNames =
            new string[LightSourceTypeCount]
            {
                "baseLight",
                "directinalLight",
                "pointLight",
                "spotLight"
            };

        private Buffer11 _illuminationPropertiesBufferObject;

        private int _lightInterfaceCount;
        private ClassInstance[] _lightInterfaces;
        private int[] _lightVariableOffsets = new int[MaxLights];
        private ClassInstance[] _lightInstances;

        private void InitializeIllumination(CompilationResult pixelShaderByteCode)
        {
            ShaderReflection pixelShaderReflection = new ShaderReflection(pixelShaderByteCode);
            _lightInterfaceCount = pixelShaderReflection.InterfaceSlotCount;
            if (_lightInterfaceCount != MaxLights) throw new IndexOutOfRangeException("Light interfaces count");
            _lightInterfaces = new ClassInstance[_lightInterfaceCount];

            ShaderReflectionVariable shaderVariableLights = pixelShaderReflection.GetVariable("lights");

            for (int i = 0; i <= MaxLights - 1; ++i)
                _lightVariableOffsets[i] = shaderVariableLights.GetInterfaceSlot(i);

            _lightInstances = new ClassInstance[LightSourceTypeCount];
            for (int i = 0; i <= LightSourceTypeCount - 1; ++i)
                _lightInstances[i] = _pixelShaderClassLinkage.GetClassInstance(lightClassVariableNames[i], 0);

            Utilities.Dispose(ref shaderVariableLights);
            Utilities.Dispose(ref pixelShaderReflection);
        }

        private void CreateIlluminationConstantBuffer()
        {
            _illuminationPropertiesBufferObject = new Buffer11(
                _device,
                Utilities.SizeOf<IlluminationProperties>(),
                ResourceUsage.Dynamic,
                BindFlags.ConstantBuffer,
                CpuAccessFlags.Write,
                ResourceOptionFlags.None,
                0);
        }

        public void UpdateIllumination(IlluminationProperties illumination)
        {
            DataStream dataStream;
            _deviceContext.MapSubresource(
                _illuminationPropertiesBufferObject,
                MapMode.WriteDiscard,
                SharpDX.Direct3D11.MapFlags.None,
                out dataStream);
            dataStream.Write(illumination);
            _deviceContext.UnmapSubresource(_illuminationPropertiesBufferObject, 0);
            _deviceContext.PixelShader.SetConstantBuffer(1, _illuminationPropertiesBufferObject);

            for (int i = 0; i < MaxLights; ++i)
            {
                _lightInterfaces[_lightVariableOffsets[i]] = _lightInstances[(int)illumination[i].lightSourceType];
            }
        }

        public void DisposeIllumination()
        {
            Utilities.Dispose(ref _illuminationPropertiesBufferObject);
            for (int i = 0; i <= LightSourceTypeCount - 1; ++i)
                Utilities.Dispose(ref _lightInstances[i]);
            _lightInterfaces = null;
        }
    }
}
