using SharpDX;

namespace GameEngine.Graphics
{
    public class OrthoCamera : Camera
    {
        public OrthoCamera(Vector3 position, float rotX = 0, float rotY = 0, float rotZ = 0, float fovY = 0.7853982F, float aspect = 1) 
            : base(position, rotX, rotY, rotZ, fovY, aspect)
        {
        }

        public override Matrix GetPojectionMatrix()
        {
            return Matrix.OrthoLH(_windowWidth * _fovY, _windowHeight * _fovY, -1000f, 1000f);
        }
    }
}
