using DisposeGame.Components;
using DisposeGame.Scenes;
using GameEngine.Animation;
using GameEngine.Game;
using GameEngine.Graphics;
using GameEngine.Scripts;
using SharpDX;
using SharpDX.DirectInput;
using System;
using System.Collections.Generic;

namespace DisposeGame.Scripts.Character
{
    public class PlayerMovementScript : KeyboardListenerScript
    {
        public event Action OnJump;

        private CharacterMovement _movement;
        private InputController _inputController;
        private Vector3 _moveDirection;
        private float _mouseSensitivity;

        public PlayerMovementScript(Animation animation, PhysicsComponent physics, List<Game3DObject> walls, float speed = 30f, float jump = 30, float mouseSensitivity = 0.25f)
        {
            var pauseMenuScene = new PauseMenuScene();

            _mouseSensitivity = mouseSensitivity;

            Actions.Add(Key.W, delta => _moveDirection += Vector3.UnitZ);
            Actions.Add(Key.S, delta => _moveDirection -= Vector3.UnitZ);
            Actions.Add(Key.A, delta => _moveDirection -= Vector3.UnitX);
            Actions.Add(Key.D, delta => _moveDirection += Vector3.UnitX);
            Actions.Add(Key.Escape, delta => GameObject.Scene.Game.ChangeScene(pauseMenuScene));
            Actions.Add(Key.Space, delta =>
            {
                if (GameObject.Position.Y < 0.01)
                {
                    physics.AddImpulse(Vector3.UnitY * jump);
                    OnJump?.Invoke();
                }
            });

            _inputController = InputController.GetInstance();

            _movement = new CharacterMovement(animation, speed, walls);

        }

        public override void Update(float delta)
        {
            base.Update(delta);

            if (_inputController.MouseUpdate)
            {
                GameObject.RotateZ(delta * _mouseSensitivity * _inputController.MouseRelativePositionX);
            }
        }

        protected override void BeforeKeyProcess(float delta)
        {
            _moveDirection = Vector3.Zero;
        }

        protected override void AfterKeyProcess(float delta)
        {
            _movement.Move(GameObject, _moveDirection, delta);
        }
    }
}
