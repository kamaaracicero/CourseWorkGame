using GameEngine.Graphics;
using SharpDX;
using SharpDX.Direct3D;
using System.Collections.Generic;

namespace GameEngine.Collisions
{
    public class SphereCollision : ObjectCollision
    {
        private float _radius;

        private MeshObject _mesh;

        public SphereCollision(float radius)
        {
            _radius = radius;

            _mesh = MakeMesh();
        }

        protected override object GetCollision()
        {
            return new BoundingSphere(GameObject.Position, _radius);
        }

        public override ObjectCollision GetCopy()
        {
            return new SphereCollision(_radius);
        }

        public override MeshObject GetMesh()
        {
            return _mesh;
        }

        private MeshObject MakeMesh()
        {
            Vector3[] corners = new BoundingBox(new Vector3(-_radius), new Vector3(_radius)).GetCorners();

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
