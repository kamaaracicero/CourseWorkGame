using GameEngine.Graphics;
using SharpDX;
using SharpDX.Direct3D;
using System.Collections.Generic;

namespace GameEngine.Collisions
{
    public class BoxCollision : ObjectCollision
    {
        private Vector3 _halfSize;

        private MeshObject _mesh;

        public BoxCollision(float width, float height) : this(width, height, width)
        {
        }

        public BoxCollision(float sizeX, float sizeY, float sizeZ) : this(new Vector3(sizeX, sizeY, sizeZ) / 2f)
        {
        }

        private BoxCollision(Vector3 halfSize)
        {
            _halfSize = halfSize;

            _mesh = MakeMesh();
        }

        protected override object GetCollision()
        {
            Vector3 position = GameObject.Position;
            return new BoundingBox(position - _halfSize, position + _halfSize);
        }

        public override ObjectCollision GetCopy()
        {
            return new BoxCollision(_halfSize);
        }

        public override MeshObject GetMesh()
        {
            return _mesh;
        }

        private MeshObject MakeMesh()
        {
            Vector3[] corners = new BoundingBox(-_halfSize, _halfSize).GetCorners();

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
                        color = Vector4.One
                    });
                    vertices.Add(new Renderer.VertexDataStruct()
                    {
                        position = new Vector4(corners[j], 1),
                        color = Vector4.One
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
                new Material(null, Vector3.Zero, Vector3.One, Vector3.One, Vector3.One, 1),
                true, "", false);
        }
    }
}
