using GameEngine.Game;
using GameEngine.Graphics;
using SharpDX;
using SharpDX.Direct3D;
using System.Collections.Generic;

namespace GameEngine.Collisions
{
    public class StaticBoxCollision : ObjectCollision
    {
        private BoundingBox _box;

        private MeshObject _mesh;

        public StaticBoxCollision(BoundingBox box, int verticesCount)
        {
            _box = box;

            Vector4 color;
            switch (verticesCount)
            {
                case 22: // Red
                    color = new Vector4(1, 0, 0, 1);
                    break;
                case 24: // Yellow
                    color = new Vector4(1, 1, 0, 1);
                    break;
                case 72: // Cian
                    color = new Vector4(0, 1, 1, 1);
                    break;
                case 120: // Purple
                    color = new Vector4(1, 0, 1, 1);
                    break;
                case 156: // Green
                    color = new Vector4(0, 1, 0, 1);
                    break;
                default: // Blue
                    color = new Vector4(0, 0, 1, 1);
                    break;
            }

            _mesh = MakeMesh(color);
        }

        public StaticBoxCollision(BoundingBox box)
        {
            _box = box;

            _mesh = MakeMesh(Vector4.One);
        }

        protected override object GetCollision()
        {
            return _box;
        }

        public override ObjectCollision GetCopy()
        {
            return new StaticBoxCollision(_box);
        }

        public override MeshObject GetMesh()
        {
            return _mesh;
        }

        private MeshObject MakeMesh(Vector4 color)
        {
            Vector3[] corners = _box.GetCorners();

            List<Renderer.VertexDataStruct> vertices = new List<Renderer.VertexDataStruct>();
            List<uint> indices = new List<uint>();

            uint k = 0;
            for (int i = 0; i < corners.Length; i++)
            {
                for (int j = i; j < corners.Length; j++)
                {
                    vertices.Add(new Renderer.VertexDataStruct()
                    {
                        position = new Vector4(corners[i], 1),
                        color = color
                    });
                    vertices.Add(new Renderer.VertexDataStruct()
                    {
                        position = new Vector4(corners[j], 1),
                        color = color
                    });
                    indices.Add(k++);
                    indices.Add(k++);
                }
            }

            return new MeshObject(
                DirectX3DGraphics.GetInstance(), 
                vertices.ToArray(),
                indices.ToArray(), 
                PrimitiveTopology.LineList, 
                new Material(Loader.DefaultTexture, Vector3.One, Vector3.One, Vector3.One, Vector3.One, 1),
                true, "", false);
        }
    }
}
