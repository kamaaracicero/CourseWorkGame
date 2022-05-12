using System;
using SharpDX;
using SharpDX.DirectInput;
using SharpDX.Windows;

namespace GameEngine.Game
{
    public class InputController : IDisposable
    {
        private static InputController _instance;

        private DirectInput _directInput;

        private Keyboard _keyboard;
        private KeyboardState _keyboardState;

        private bool _keyboardUpdate = false;
        public bool KeyboardUpdate { get => _keyboardUpdate; }
        private bool _keyboardAcquired;

        private Mouse _mouse;
        private MouseState _mouseState;
        private bool _mouseUpdate = false;
        public bool MouseUpdate { get => _mouseUpdate; }
        private bool _mouseAcquired;

        private bool[] _mouseButtons = new bool[8];
        public bool[] MouseButtons { get => _mouseButtons; }

        private int _mouseRelativePositionX;
        public int MouseRelativePositionX { get => _mouseRelativePositionX; }

        private int _mouseRelativePositionY;
        public int MouseRelativePositionY { get => _mouseRelativePositionY; }

        private int _mouseRelativePositionZ;
        public int MouseRelativePositionZ { get => _mouseRelativePositionZ; }

        private float _mouseX;
        private float _mouseY;

        public float MousePositionX { get => _mouseX; }
        public float MousePositionY { get => _mouseY; }

        private bool _keyEscPreviousPressed;
        private bool _keyEscCurrentPressed;
        private bool _keyEsc;
        public bool KeyEsc { get => _keyEsc; }

        private bool _keyF2PreviousPressed;
        private bool _keyF2CurrentPressed;
        private bool _keyF2;
        public bool KeyF2 { get => _keyF2; }

        public Ray? CursorRay { get; set; }

        private InputController(RenderForm renderForm)
        {
            _directInput = new DirectInput();

            _keyboard = new Keyboard(_directInput);
            _keyboard.SetCooperativeLevel(renderForm.Handle,
                CooperativeLevel.Foreground | CooperativeLevel.NonExclusive);

            AcquireKeyboard();
            _keyboardState = new KeyboardState();

            _mouse = new Mouse(_directInput);
            _mouse.SetCooperativeLevel(renderForm.Handle,
                CooperativeLevel.Foreground | CooperativeLevel.NonExclusive);

            AcquireMouse();
            _mouseState = new MouseState();

            renderForm.MouseMove += (sender, args) =>
            {
                _mouseX = args.X;
                _mouseY = args.Y;
            };
        }

        public static InputController GetInstance(RenderForm renderForm = null)
        {
            if (_instance == null)
            {
                _instance = new InputController(renderForm);
            }
            return _instance;
        }

        public void ResetAcquired()
        {
            _keyboardAcquired = false;
            _mouseAcquired = false;
        }

        public bool IsPressed(Key key)
        {
            return _keyboardState.IsPressed(key);
        }

        private void AcquireKeyboard()
        {
            try
            {
                _keyboard.Acquire();
                _keyboardAcquired = true;
            }
            catch (SharpDXException e)
            {
                if (e.ResultCode.Failure)
                    _keyboardAcquired = false;
            }
        }
        private void AcquireMouse()
        {
            try
            {
                _mouse.Acquire();
                _mouseAcquired = true;
            }
            catch (SharpDXException e)
            {
                if (e.ResultCode.Failure)
                    _mouseAcquired = false;
            }
        }

        private bool TriggerByKeyUp(Key key, ref bool previos, ref bool current)
        {
            previos = current;
            current = _keyboardState.IsPressed(key);
            return previos && !current;
        }

        private bool TriggerByKeyDown(Key key, ref bool previos, ref bool current)
        {
            previos = current;
            current = _keyboardState.IsPressed(key);
            return !previos && current;
        }

        private void ProcessKeyboardState()
        {
            _keyEsc = TriggerByKeyUp(Key.Escape, ref _keyEscPreviousPressed, ref _keyEscCurrentPressed);
            _keyF2 = TriggerByKeyDown(Key.F2, ref _keyF2PreviousPressed, ref _keyF2CurrentPressed);
        }

        public void UpdateKeyboardState()
        {
            if (!_keyboardAcquired) AcquireKeyboard();
            ResultDescriptor resultCode = ResultCode.Ok;
            try
            {
                _keyboard.GetCurrentState(ref _keyboardState);
                ProcessKeyboardState();
                _keyboardUpdate = true;
            }
            catch (SharpDXException e)
            {
                resultCode = e.Descriptor;
                _keyboardUpdate = false;
            }
            if (resultCode == ResultCode.InputLost || resultCode == ResultCode.NotAcquired)
                _keyboardUpdate = false;
        }

        private void ProcessMouseState()
        {
            for (int i = 0; i <= 7; ++i)
                _mouseButtons[i] = _mouseState.Buttons[i];
            _mouseRelativePositionX = _mouseState.X;
            _mouseRelativePositionY = _mouseState.Y;
            _mouseRelativePositionZ = _mouseState.Z;
        }

        public void UpdateMouseState()
        {
            if (!_mouseAcquired) AcquireMouse();
            ResultDescriptor resultCode = ResultCode.Ok;
            try
            {
                _mouse.GetCurrentState(ref _mouseState);
                ProcessMouseState();
                _mouseUpdate = true;
            }
            catch (SharpDXException e)
            {
                resultCode = e.Descriptor;
                _mouseUpdate = false;
            }
            if (resultCode == ResultCode.InputLost || resultCode == ResultCode.NotAcquired)
                _mouseUpdate = false;
        }

        public void Dispose()
        {
            _mouse.Unacquire();
            Utilities.Dispose(ref _mouse);
            _keyboard.Unacquire();
            Utilities.Dispose(ref _keyboard);
            Utilities.Dispose(ref _directInput);
        }
    }
}
