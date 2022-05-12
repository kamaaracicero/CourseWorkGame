using GameEngine.Scripts;
using SharpDX.DirectInput;
using SharpDX;

namespace DisposeGame.Scripts
{
    public class CameraMovementScript : KeyboardListenerScript
    {
        private Vector3 _moveDirection;
        private float _speed;

        public CameraMovementScript(float speed = 1f)
        {
            _speed = speed;

            Actions.Add(Key.W, delta => _moveDirection += Vector3.UnitZ);
            Actions.Add(Key.S, delta => _moveDirection -= Vector3.UnitZ);
            Actions.Add(Key.A, delta => _moveDirection -= Vector3.UnitX);
            Actions.Add(Key.D, delta => _moveDirection += Vector3.UnitX);
        }

        protected override void BeforeKeyProcess(float delta)
        {
            _moveDirection = Vector3.Zero;
        }

        protected override void AfterKeyProcess(float delta)
        {
            _moveDirection.Normalize();
            Vector3 rotation = GameObject.Rotation;
            Matrix rotationMatrix = Matrix.RotationYawPitchRoll(rotation.Z, rotation.Y, rotation.X);
            GameObject.MoveBy((Vector3)Vector3.Transform(_moveDirection * _speed * delta, rotationMatrix));
        }
    }
}
