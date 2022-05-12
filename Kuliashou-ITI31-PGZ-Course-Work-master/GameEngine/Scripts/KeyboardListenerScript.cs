using GameEngine.Game;
using SharpDX.DirectInput;
using System;
using System.Collections.Generic;

namespace GameEngine.Scripts
{
    public class KeyboardListenerScript : Script
    {
        private InputController _inputController;
        private Dictionary<Key, Action<float>> _actions;
        
        public Dictionary<Key, Action<float>> Actions { get => _actions; }

        public KeyboardListenerScript()
        {
            _inputController = InputController.GetInstance();
            _actions = new Dictionary<Key, Action<float>>();
        }

        public override void Update(float delta)
        {
            if (_inputController.KeyboardUpdate)
            {
                BeforeKeyProcess(delta);
                foreach (Key key in _actions.Keys)
                {
                    if (_inputController.IsPressed(key))
                    {
                        _actions[key].Invoke(delta);
                    }
                }
                AfterKeyProcess(delta);
            }
        }

        protected virtual void BeforeKeyProcess(float delta)
        {
        }

        protected virtual void AfterKeyProcess(float delta)
        {
        }
    }
}
