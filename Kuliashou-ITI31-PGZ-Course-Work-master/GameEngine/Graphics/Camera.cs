using SharpDX;

namespace GameEngine.Graphics
{
    public class Camera : Game3DObject
    {
        protected float _fovY;
        public float FOVY { get => _fovY; set => _fovY = value; }

        protected float _aspect;
        public float Aspect { get => _aspect; set => _aspect = value; }

        protected float _windowWidth;
        public float WindowWidth { get => _windowWidth; set => _windowWidth = value; }

        protected float _windowHeight;
        public float WindowHeight { get => _windowHeight; set => _windowHeight = value; }

        public Camera(Vector3 position, float rotX = 0.0f,
            float rotY = 0.0f, float rotZ = 0.0f, float fovY = MathUtil.PiOverFour,
            float aspect = 1.0f) : base(position, new Vector3(rotX, rotY, rotZ))
        {
            _fovY = fovY;
            _aspect = aspect;
            IsHidden = true;
        }

        public void MoveForZ(float direction)
        {
            Matrix rotation = Matrix.RotationYawPitchRoll(Rotation.Z, Rotation.Y, Rotation.X);
            Vector3 viewTo = (Vector3)Vector4.Transform(Vector4.UnitZ * direction, rotation);
            MoveBy(viewTo.X, viewTo.Y, viewTo.Z);
        }

        public void MoveForX(float direction)
        {
            Matrix rotation = Matrix.RotationYawPitchRoll(Rotation.Z, Rotation.Y, Rotation.X);
            Vector3 viewTo = (Vector3)Vector4.Transform(Vector4.UnitX * direction, rotation);
            MoveBy(viewTo.X, viewTo.Y, viewTo.Z);
        }

        public virtual Matrix GetPojectionMatrix()
        {
            return Matrix.PerspectiveFovLH(_fovY, _aspect, 0.1f, 1000.0f);
        }

        public Matrix GetViewMatrix()
        {
            Matrix rotation = Matrix.RotationYawPitchRoll(Rotation.Z, Rotation.Y, Rotation.X);
            Vector3 viewTo = (Vector3)Vector4.Transform(Vector4.UnitZ, rotation);
            Vector3 viewUp = (Vector3)Vector4.Transform(Vector4.UnitY, rotation);
            return Matrix.LookAtLH(Position, Position + viewTo, viewUp);
        }
    }

}
