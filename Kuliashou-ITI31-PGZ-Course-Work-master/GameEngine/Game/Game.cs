using System;
using AmazingUILibrary;
using GameEngine.Animation;
using GameEngine.Graphics;
using SharpDX;
using SharpDX.Windows;
using Sound;

namespace GameEngine.Game
{
    public class Game : IDisposable
    {
        public event EventHandler SwapChainResizing;
        public event EventHandler SwapChainResized;

        private RenderForm _renderForm;

        private Camera _camera;

        private Scene _scene;
        private Scene _nextScene;

        private Renderer.IlluminationProperties _illumination;

        private DirectX3DGraphics _directX3DGraphics;
        private DirectX2DGraphics _directX2DGraphics;
        private Renderer _renderer;
        private SharpAudioDevice _audioDevice;
        private Loader _loader;

        private TimeHelper _timeHelper;
        private InputController _inputController;

        private UIElement _ui;

        public Game(Scene scene)
        {
            _scene = scene;

            _renderForm = new RenderForm();
            _renderForm.UserResized += RenderFormResizedCallback;

            _renderForm.MouseDown += (sender, args) =>
            {
                _ui.Press(_inputController.MousePositionX, _inputController.MousePositionY);
            };
            _renderForm.MouseUp += (sender, args) =>
            {
                if (!_ui.Release(_inputController.MousePositionX, _inputController.MousePositionY))
                {
                    Ray ray = Ray.GetPickRay(
                        (int)_inputController.MousePositionX,
                        (int)_inputController.MousePositionY,
                        new ViewportF(
                            _renderForm.ClientRectangle.X,
                            _renderForm.ClientRectangle.Y,
                            _renderForm.ClientRectangle.Width,
                            _renderForm.ClientRectangle.Height,
                            0, 1),
                        _camera.GetViewMatrix() * _camera.GetPojectionMatrix());

                    _inputController.CursorRay = ray;
                }
            };


            _directX3DGraphics = DirectX3DGraphics.GetInstance(_renderForm);
            _directX2DGraphics = new DirectX2DGraphics(this, _directX3DGraphics);
            _renderer = new Renderer(_directX3DGraphics, _directX2DGraphics);
            _renderer.CreateConstantBuffers();

            _audioDevice = new SharpAudioDevice();
            _loader = new Loader(_directX3DGraphics, _renderer.LinearSampler);

            _timeHelper = new TimeHelper();
            _inputController = InputController.GetInstance(_renderForm);

            _renderForm.GotFocus += (sender, args) =>
            {
                _inputController.ResetAcquired();
            };

            InitializeScene();
        }

        private void InitializeScene()
        {
            _scene.Game = this;
            _scene.Initialize(_loader, _audioDevice, _directX2DGraphics.DrawingContext, 
                _renderForm.ClientSize.Width, _renderForm.ClientSize.Height);
            _ui = _scene.UI;
            _illumination = _scene.Illumination;
            _camera = _scene.Camera;
        }

        public void RenderFormResizedCallback(object sender, EventArgs args)
        {
            SwapChainResizing?.Invoke(this, null);
            _directX3DGraphics.Resize();
            _camera.Aspect = _renderForm.ClientSize.Width / (float)_renderForm.ClientSize.Height;
            _camera.WindowWidth = _renderForm.ClientSize.Width;
            _camera.WindowHeight = _renderForm.ClientSize.Height;
            _ui.Size = new Vector2(_renderForm.ClientSize.Width, _renderForm.ClientSize.Height);
            SwapChainResized?.Invoke(this, null);

        }

        private bool _firstRun = true;

        public void RenderLoopCallback()
        {
            if (_firstRun)
            {
                RenderFormResizedCallback(this, EventArgs.Empty);
                _firstRun = false;
            }
            _timeHelper.Update();
            _renderForm.Text = "FPS: " + _timeHelper.FPS.ToString();

            _inputController.UpdateKeyboardState();
            _inputController.UpdateMouseState();
            Transition.UpdateTransitions(_timeHelper.DeltaT);
            _scene.Update(_timeHelper.DeltaT);

            if (_inputController.KeyboardUpdate)
            {
                if (_inputController.KeyF2) _directX3DGraphics.IsFullScreen = !_directX3DGraphics.IsFullScreen;
            }

            _renderer.UpdatePerFrameConstantBuffers(_timeHelper.Time);

            _illumination.eyePosition = (Vector4)_camera.Position;
            _renderer.UpdateIllumination(_illumination);

            _renderer.BeginRender();
            _scene.RenderGameObjects(_camera, _renderer);
            _renderer.RenderUserInterface(_ui);
            _renderer.EndRender();

            if (_nextScene != null)
            {
                _nextScene.PreviousScene = _scene;
                _scene = _nextScene;
                _nextScene = null;
                InitializeScene();
                RenderFormResizedCallback(this, EventArgs.Empty);
            }
        }

        public Scene ChangeScene(Scene scene)
        {
            _nextScene = scene;
            return _scene;
        }

        public void CloseGame()
        {
            _renderForm.Close();
        }

        public void Run()
        {
            RenderLoop.Run(_renderForm, RenderLoopCallback);
        }

        public void Dispose()
        {
            _scene.Dispose();
            _loader.Dispose();
            _audioDevice.Dispose();
            _inputController.Dispose();
            _renderer.Dispose();
            _directX3DGraphics.Dispose();
            _directX2DGraphics.Dispose();
        }
    }
}
