using GameEngine.Components;

namespace DisposeGame.Components
{
    public class VisibilityComponent : ObjectComponent
    {
        private float _duration;
        private float _time;
        private bool _isInvisible;

        public bool IsInvisible { get => _isInvisible; }

        public void MakeInvisible(float duration)
        {
            _duration = duration;
            _time = 0;
            _isInvisible = true;
        }

        public void UpdateVisibility(float delta)
        {
            if (_isInvisible)
            {
                _time += delta;
                if (_time >= _duration)
                {
                    _isInvisible = false;
                }
            }
        }
    }
}
