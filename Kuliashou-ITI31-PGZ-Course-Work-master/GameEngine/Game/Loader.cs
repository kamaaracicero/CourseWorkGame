using System;
using SharpDX;
using SharpDX.DXGI;
using SharpDX.WIC;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using Assimp;
using Assimp.Configs;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using GameEngine.Graphics;

namespace GameEngine.Game
{
    public class Loader : IDisposable
    {
        public static Texture DefaultTexture { get; private set; }
        public static Graphics.Material DefaultMaterial { get; private set; }

        private const string DEFAULT_FOLDER = "Textures\\";

        private DirectX3DGraphics _directX3DGraphics;
        private ImagingFactory _imagingFactory;
        private SamplerState samplerState;
        private SampleDescription _sampleDescription = new SampleDescription(1, 0);
        private List<MeshObject> meshList = new List<MeshObject>();
        private List<Graphics.Material> materials = new List<Graphics.Material>();
        private List<Texture> textures = new List<Texture>();

        public Loader(DirectX3DGraphics directX3DGraphics, SamplerState state)
        {
            _directX3DGraphics = directX3DGraphics;
            _imagingFactory = new ImagingFactory();
            samplerState = state;
            DefaultTexture = LoadTextureFromFile("Textures\\white.png", true);
            DefaultMaterial = new Graphics.Material(DefaultTexture, Vector3.Zero, Vector3.One, Vector3.One, Vector3.One, 1);
        }

        public BitmapFrameDecode LoadBitmapFromFile(string fileName)
        {
            BitmapDecoder decoder = new BitmapDecoder(_imagingFactory, fileName, DecodeOptions.CacheOnDemand);
            BitmapFrameDecode frame = decoder.GetFrame(0);

            Utilities.Dispose(ref decoder);
            return frame;
        }

        private Texture LoadTextureFromFile(string fileName, bool generateMips, int mipLevels = -1)
        {
            BitmapDecoder decoder = new BitmapDecoder(_imagingFactory,
                fileName, DecodeOptions.CacheOnDemand);
            BitmapFrameDecode bitmapFirstFrame = decoder.GetFrame(0);

            Utilities.Dispose(ref decoder);

            FormatConverter formatConverter = new FormatConverter(_imagingFactory);
            formatConverter.Initialize(bitmapFirstFrame, PixelFormat.Format32bppRGBA,
                BitmapDitherType.None, null, 0.0f, BitmapPaletteType.Custom);

            int stride = formatConverter.Size.Width * 4;
            DataStream buffer = new DataStream(
                formatConverter.Size.Height * stride, true, true);
            formatConverter.CopyPixels(stride, buffer);

            int width = formatConverter.Size.Width;
            int height = formatConverter.Size.Height;
            Texture2DDescription texture2DDescription = new Texture2DDescription()
            {
                Width = width,
                Height = height,
                MipLevels = (generateMips ? 0 : 1),
                ArraySize = 1,
                Format = Format.R8G8B8A8_UNorm,
                SampleDescription = _sampleDescription,
                Usage = ResourceUsage.Default,
                BindFlags = (
                    generateMips ?
                    BindFlags.ShaderResource | BindFlags.RenderTarget : BindFlags.ShaderResource
                    ),
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = (
                   generateMips ?
                   ResourceOptionFlags.GenerateMipMaps :
                   ResourceOptionFlags.None
                   )
            };

            Texture2D textureObject;

            if (generateMips)
                textureObject = new Texture2D(_directX3DGraphics.Device, texture2DDescription);
            else
            {
                DataRectangle dataRectangle = new DataRectangle(buffer.DataPointer, stride);
                textureObject = new Texture2D(_directX3DGraphics.Device, texture2DDescription, dataRectangle);
            }

            ShaderResourceViewDescription shaderResourceViewDescription =
                new ShaderResourceViewDescription()
                {
                    Dimension = ShaderResourceViewDimension.Texture2D,
                    Format = Format.R8G8B8A8_UNorm,
                    Texture2D = new ShaderResourceViewDescription.Texture2DResource
                    {
                        MostDetailedMip = 0,
                        MipLevels = (generateMips ? mipLevels : 1)
                    }
                };
            ShaderResourceView shaderResourceView =
                new ShaderResourceView(_directX3DGraphics.Device, textureObject, shaderResourceViewDescription);
            if (generateMips)
            {
                DataBox dataBox = new DataBox(buffer.DataPointer, stride, 1);
                _directX3DGraphics.DeviceContext.UpdateSubresource(dataBox, textureObject, 0);
                _directX3DGraphics.DeviceContext.GenerateMips(shaderResourceView);
            }

            Utilities.Dispose(ref formatConverter);

            return new Texture(textureObject, shaderResourceView, width, height, samplerState);
        }

        public Game3DObject LoadGameObjectFromFile(string path, Vector3 startPosition, Vector3 startRotation, string texture = null)
        {
            var importer = new AssimpContext();
            importer.SetConfig(new NormalSmoothingAngleConfig(66.0f));
            var fileScene = importer.ImportFile(path, PostProcessSteps.Triangulate);
            var indices = new List<uint>();
            var vertices = new List<Renderer.VertexDataStruct>();
            var root = fileScene.RootNode;
            Game3DObject gameObject = null;
            meshList = new List<MeshObject>();
            materials = new List<Graphics.Material>();
            textures = new List<Texture>();

            ExtractTextures(fileScene);
            ExtractMaterials(fileScene, texture);
            ExtractMeshes(fileScene, indices, vertices);

            gameObject = new Game3DObject(startPosition, startRotation);
            if (root.HasChildren)
            {
                gameObject.IsHidden = true;
                foreach (var child in root.Children)
                {

                    if (child.HasMeshes)
                    {
                        child.Transform.Decompose(out Vector3D _, out Assimp.Quaternion rotation, out Vector3D position);
                        var childObject = gameObject.AddChild(new Game3DObject(new Vector3(position.X, position.Y, position.Z), ToEulerAngles(rotation)));
                        childObject.AddMeshObject(meshList[child.MeshIndices[0]]);
                        GetChildrenMeshes(ref childObject, child);
                    }
                }
            }
            else
            {
                gameObject.AddMeshObject(meshList[root.MeshIndices[0]]);
            }

            return gameObject;
        }

        private void GetChildrenMeshes(ref Game3DObject parent, Node node)
        {
            if (node.HasChildren)
            {
                var objects = new List<Game3DObject>();
                foreach (var child in node.Children)
                {
                    node.Transform.Decompose(out Vector3D _, out Assimp.Quaternion rotation, out Vector3D position);
                    var newChildObject = new Game3DObject(new Vector3(position.X, position.Y, position.Z), ToEulerAngles(rotation));
                    newChildObject.AddMeshObject(meshList.Find(x => x.Name == child.Name));
                    GetChildrenMeshes(ref newChildObject, child);

                    parent.AddChild(newChildObject);
                }
            }
        }

        private void ExtractMeshes(Assimp.Scene fileScene, List<uint> indices, List<Renderer.VertexDataStruct> vertices)
        {
            foreach (var mesh in fileScene.Meshes)
            {
                vertices.Clear();
                indices.Clear();
                for (int i = 0, j = 0; i < mesh.Vertices.Count && j < mesh.TextureCoordinateChannels[0].Count; i++, j++)
                {
                    var vert = mesh.Vertices[i];
                    var tex = mesh.TextureCoordinateChannels[0][j];
                    var normal = mesh.Normals[i];
                    vertices.Add(new Renderer.VertexDataStruct
                    {
                        position = new Vector4(vert.X, vert.Y, -vert.Z, 1),
                        texCoord = new Vector2(tex.X, 1 - tex.Y),
                        normal = new Vector4(normal.X, normal.Y, normal.Z, 1),
                        color = Vector4.One
                    });
                }

                foreach (var index in mesh.GetIndices())
                {
                    indices.Add((uint)index);
                }

                PrimitiveTopology type;
                switch (mesh.PrimitiveType)
                {
                    case PrimitiveType.Line:
                        {
                            type = PrimitiveTopology.LineList;
                            break;
                        }
                    case PrimitiveType.Point:
                        {
                            type = PrimitiveTopology.PointList;
                            break;
                        }
                    default:
                        {
                            type = PrimitiveTopology.TriangleList;
                            break;
                        }
                }

                meshList.Add(new MeshObject(_directX3DGraphics, vertices.ToArray(), indices.ToArray(), type, materials[mesh.MaterialIndex], true, mesh.Name));
            }
        }

        private void ExtractMaterials(Assimp.Scene fileScene, string texture = null)
        {
            foreach (var material in fileScene.Materials)
            {

                Vector3 emissive = Vector3.One;
                Vector3 ambient = Vector3.One;
                Vector3 diffuse = Vector3.One;
                Vector3 specular = Vector3.One;
                if (material.GetMaterialTexture(TextureType.Diffuse, 0, out TextureSlot slot))
                {
                    if (texture != null)
                    {
                        materials.Add(new Graphics.Material(LoadTextureFromFile(texture, true, 4), emissive, ambient, diffuse, specular, 1));
                    }
                    else
                    {
                        try
                        {
                            materials.Add(new Graphics.Material(textures[slot.TextureIndex], emissive, ambient, diffuse, specular, 1));
                        }
                        catch (ArgumentOutOfRangeException e)
                        {
                            materials.Add(new Graphics.Material(LoadTextureFromFile(GetRelativePath(slot.FilePath), true, 4), emissive, ambient, diffuse, specular, 1));
                        }
                    }
                }
                else
                {
                    materials.Add(new Graphics.Material(DefaultTexture, emissive, ambient, diffuse, specular, 1));

                }
            }
            if (materials.Count == 0)
            {
                materials.Add(DefaultMaterial);
            }
        }

        private void ExtractTextures(Assimp.Scene fileScene)
        {
            foreach (var texture in fileScene.Textures)
            {
                if (texture.HasCompressedData)
                {
                    using (var stream = new MemoryStream(texture.CompressedData))
                    {
                        using (var img = (System.Drawing.Bitmap)(Image.FromStream(stream)))
                        {
                            if (img.PixelFormat != System.Drawing.Imaging.PixelFormat.Format32bppArgb)
                            {
                                using (var rightImage = img.Clone(new System.Drawing.Rectangle(0, 0, img.Width, img.Height), System.Drawing.Imaging.PixelFormat.Format32bppArgb))
                                {
                                    var str = Path.GetTempFileName();
                                    rightImage.Save(str);
                                    textures.Add(LoadTextureFromFile(str, true, 4));
                                }
                            }
                            else
                            {
                                var str = Path.GetTempFileName();
                                img.Save(str);
                                textures.Add(LoadTextureFromFile(str, true, 4));
                            }
                        }
                    }
                }
                else
                {
                    textures.Add(DefaultTexture);
                }
            }
        }

        private static Vector3 ToEulerAngles(Assimp.Quaternion q)
        {
            Vector3 angles;

            double sinr_cosp = 2 * (q.W * q.X + q.Y * q.Z);
            double cosr_cosp = 1 - 2 * (q.X * q.X + q.Y * q.Y);
            angles.X = (float)Math.Atan2(sinr_cosp, cosr_cosp);

            double sinp = 2 * (q.W * q.Y - q.Z * q.X);
            if (Math.Abs(sinp) >= 1)
                angles.Y = (float)(Math.Sign(sinp) * Math.PI / 2);
            else
                angles.Y = (float)Math.Asin(sinp);

            double siny_cosp = 2 * (q.W * q.Z + q.X * q.Y);
            double cosy_cosp = 1 - 2 * (q.Y * q.Y + q.Z * q.Z);
            angles.Z = (float)Math.Atan2(siny_cosp, cosy_cosp);

            return angles;
        }

        private static string GetRelativePath(string absolutePath)
        {
            return DEFAULT_FOLDER + Path.GetFileName(absolutePath);
        }


        /////////////////////////////////////////////
        public Game3DObject MakeRectangle(Vector3 position, Vector3 rotation, Vector3 size)
        {
            Renderer.VertexDataStruct[] vertices =
                new Renderer.VertexDataStruct[24]
                {
                    new Renderer.VertexDataStruct // front 0
                    {
                        position = new Vector4(-size.X, size.Y, -size.Z, 1.0f),
                        normal = new Vector4(0.0f, 0.0f, -1.0f, 1.0f),
                        color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
                        texCoord = new Vector2(0.25f, 0.25f)
                    },
                    new Renderer.VertexDataStruct // front 1
                    {
                        position = new Vector4(-size.X, -size.Y, -size.Z, 1.0f),
                        normal = new Vector4(0.0f, 0.0f, -1.0f, 1.0f),
                        color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
                        texCoord = new Vector2(0.25f, 0.5f)
                    },
                    new Renderer.VertexDataStruct // front 2
                    {
                        position = new Vector4(size.X, -size.Y, -size.Z, 1.0f),
                        normal = new Vector4(0.0f, 0.0f, -1.0f, 1.0f),
                        color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
                        texCoord = new Vector2(0.5f, 0.5f)
                    },
                    new Renderer.VertexDataStruct // front 3
                    {
                        position = new Vector4(size.X, size.Y, -size.Z, 1.0f),
                        normal = new Vector4(0.0f, 0.0f, -1.0f, 1.0f),
                        color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
                        texCoord = new Vector2(0.5f, 0.25f)
                    },
                    new Renderer.VertexDataStruct // right 4
                    {
                        position = new Vector4(size.X, size.Y, -size.Z, 1.0f),
                        normal = new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
                        color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
                        texCoord = new Vector2(0.5f, 0.25f)
                    },
                    new Renderer.VertexDataStruct // right 5
                    {
                        position = new Vector4(size.X, -size.Y, -size.Z, 1.0f),
                        normal = new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
                        color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
                        texCoord = new Vector2(0.5f, 0.5f)
                    },
                    new Renderer.VertexDataStruct // right 6
                    {
                        position = new Vector4(size.X, -size.Y, size.Z, 1.0f),
                        normal = new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
                        color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
                        texCoord = new Vector2(0.75f, 0.5f)
                    },
                    new Renderer.VertexDataStruct // right 7
                    {
                        position = new Vector4(size.X, size.Y, size.Z, 1.0f),
                        normal = new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
                        color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
                        texCoord = new Vector2(0.75f, 0.25f)
                    },
                    new Renderer.VertexDataStruct // back 8
                    {
                        position = new Vector4(size.X, size.Y, size.Z, 1.0f),
                        normal = new Vector4(0.0f, 0.0f, 1.0f, 1.0f),
                        color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
                        texCoord = new Vector2(0.75f, 0.25f)
                    },
                    new Renderer.VertexDataStruct // back 9
                    {
                        position = new Vector4(size.X, -size.Y, size.Z, 1.0f),
                        normal = new Vector4(0.0f, 0.0f, 1.0f, 1.0f),
                        color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
                        texCoord = new Vector2(0.75f, 0.5f)
                    },
                    new Renderer.VertexDataStruct // back 10
                    {
                        position = new Vector4(-size.X, -size.Y, size.Z, 1.0f),
                        normal = new Vector4(0.0f, 0.0f, 1.0f, 1.0f),
                        color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
                        texCoord = new Vector2(1.0f, 0.5f)
                    },
                    new Renderer.VertexDataStruct // back 11
                    {
                        position = new Vector4(-size.X, size.Y, size.Z, 1.0f),
                        normal = new Vector4(0.0f, 0.0f, 1.0f, 1.0f),
                        color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
                        texCoord = new Vector2(1.0f, 0.25f)
                    },
                    new Renderer.VertexDataStruct // left 12
                    {
                        position = new Vector4(-size.X, size.Y, size.Z, 1.0f),
                        normal = new Vector4(-1.0f, 0.0f, 0.0f, 1.0f),
                        color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
                        texCoord = new Vector2(0.0f, 0.25f)
                    },
                    new Renderer.VertexDataStruct // left 13
                    {
                        position = new Vector4(-size.X, -size.Y, size.Z, 1.0f),
                        normal = new Vector4(-1.0f, 0.0f, 0.0f, 1.0f),
                        color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
                        texCoord = new Vector2(0.0f, 0.5f)
                    },
                    new Renderer.VertexDataStruct // left 14
                    {
                        position = new Vector4(-size.X, -size.Y, -size.Z, 1.0f),
                        normal = new Vector4(-1.0f, 0.0f, 0.0f, 1.0f),
                        color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
                        texCoord = new Vector2(0.25f, 0.5f)
                    },
                    new Renderer.VertexDataStruct // left 15
                    {
                        position = new Vector4(-size.X, size.Y, -size.Z, 1.0f),
                        normal = new Vector4(-1.0f, 0.0f, 0.0f, 1.0f),
                        color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
                        texCoord = new Vector2(0.25f, 0.25f)
                    },
                    new Renderer.VertexDataStruct // top 16
                    {
                        position = new Vector4(-size.X, size.Y, size.Z, 1.0f),
                        normal = new Vector4(0.0f, 1.0f, 0.0f, 1.0f),
                        color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
                        texCoord = new Vector2(0.25f, 0.0f)
                    },
                    new Renderer.VertexDataStruct // top 17
                    {
                        position = new Vector4(-size.X, size.Y, -size.Z, 1.0f),
                        normal = new Vector4(0.0f, 1.0f, 0.0f, 1.0f),
                        color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
                        texCoord = new Vector2(0.25f, 0.25f)
                    },
                    new Renderer.VertexDataStruct // top 18
                    {
                        position = new Vector4(size.X, size.Y, -size.Z, 1.0f),
                        normal = new Vector4(0.0f, 1.0f, 0.0f, 1.0f),
                        color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
                        texCoord = new Vector2(0.5f, 0.25f)
                    },
                    new Renderer.VertexDataStruct // top 19
                    {
                        position = new Vector4(size.X, size.Y, size.Z, 1.0f),
                        normal = new Vector4(0.0f, 1.0f, 0.0f, 1.0f),
                        color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
                        texCoord = new Vector2(0.5f, 0.0f)
                    },
                    new Renderer.VertexDataStruct // bottom 20
                    {
                        position = new Vector4(-size.X, -size.Y, -size.Z, 1.0f),
                        normal = new Vector4(0.0f, -1.0f, 0.0f, 1.0f),
                        color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
                        texCoord = new Vector2(0.25f, 0.5f)
                    },
                    new Renderer.VertexDataStruct // bottom 21
                    {
                        position = new Vector4(-size.X, -size.Y, size.Z, 1.0f),
                        normal = new Vector4(0.0f, -1.0f, 0.0f, 1.0f),
                        color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
                        texCoord = new Vector2(0.25f, 0.75f)
                    },
                    new Renderer.VertexDataStruct // bottom 22
                    {
                        position = new Vector4(size.X, -size.Y, size.Z, 1.0f),
                        normal = new Vector4(0.0f, -1.0f, 0.0f, 1.0f),
                        color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
                        texCoord = new Vector2(0.5f, 0.75f)
                    },
                    new Renderer.VertexDataStruct // bottom 23
                    {
                        position = new Vector4(size.X, -size.Y, -size.Z, 1.0f),
                        normal = new Vector4(0.0f, -1.0f, 0.0f, 1.0f),
                        color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
                        texCoord = new Vector2(0.5f, 0.5f)
                    }
                };
            uint[] indices = new uint[36]
            {
                8, 9, 10,       10, 11, 8,
                12, 13, 14,     14, 15, 12,
                20, 21, 22,     22, 23, 20,
                0, 1, 2,        2, 3, 0,
                4, 5, 6,        6, 7, 4,
                16, 17, 18,     18, 19, 16
            };

            Game3DObject go = new Game3DObject(position, rotation);
            go.AddMeshObject(new MeshObject(_directX3DGraphics, vertices, indices, PrimitiveTopology.TriangleList, DefaultMaterial, true, "", false));
            return go;
        }
        /////////////////////////////////////////////



        public void Dispose()
        {
            Utilities.Dispose(ref _imagingFactory);
        }
    }
}