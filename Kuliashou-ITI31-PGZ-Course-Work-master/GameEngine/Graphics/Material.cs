using GameEngine.Graphics.Buffers;
using SharpDX;

namespace GameEngine.Graphics
{
    public class Material
    {
        private MaterialProperties _materialProperties;
        public MaterialProperties MaterialProperties { get => _materialProperties; }

        private Texture _texture;
        public Texture Texture { get => _texture; }


        public Material(Texture texture, Vector3 emmisiveK, Vector3 ambientK, Vector3 diffuseK, Vector3 specularK, float specularPower)
        {
            _texture = texture;
            _materialProperties = new MaterialProperties
            {
                emmisiveK = emmisiveK,
                ambientK = ambientK,
                diffuseK = diffuseK,
                specularK = specularK,
                specularPower = specularPower
            };
        }
    }
}
