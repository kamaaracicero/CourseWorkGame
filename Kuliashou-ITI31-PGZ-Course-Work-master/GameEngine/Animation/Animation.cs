using System;

namespace GameEngine.Animation
{
    public class Animation
    {
        public event Action AnimationEnded;
        public event Action AnimationIterationEnded;
        protected event Action<float> Process;
        protected event Action TransitionEnded;
        protected event Action TransitionPaused;

        private float[] _keys;
        private float _transitionDuration;
        private int _currentKey;
        private int _iterations;
        private int _initialIterations;
        private float _delay;
        private Transition _currentTransition;

        public Animation(float[] keys, float duration, int iterations = 1, float delay = -1)
        {
            if (keys.Length < 2) throw new ArgumentException("The number of keys must be more than 2");

            _keys = keys;
            _transitionDuration = duration / (keys.Length - 1);
            _currentKey = 0;
            _iterations = iterations;
            _initialIterations = iterations;
            _delay = delay;
            _currentTransition = delay > 0 ? GetDelayTransition(delay) : GetNextTransition();
        }

        private Transition GetNextTransition()
        {
            if (_currentKey + 1 >= _keys.Length)
            {
                AnimationIterationEnded?.Invoke();
                if (--_iterations <= 0)
                {
                    AnimationEnded?.Invoke();
                    return null;
                }
                _currentKey = 0;
            }
            Transition transition = GetTransition(_keys[_currentKey], _keys[++_currentKey], _transitionDuration);
            transition.Process += Process;
            transition.TransitionEnded += TransitionEnded;
            transition.TransitionPaused += TransitionPaused;
            return transition;
        }

        private Transition GetDelayTransition(float delay)
        {
            return GetTransition(_keys[0], _keys[0], delay);
        }

        private Transition GetTransition(float start, float end, float duration)
        {
            Transition transition = CreateTransition(start, end, duration);
            transition.TransitionEnded += NextTransition;
            return transition;
        }

        private void NextTransition()
        {
            _currentTransition = GetNextTransition();
        }

        protected virtual Transition CreateTransition(float start, float end, float duration)
        {
            return new Transition(start, end, duration);
        }

        public void AddProcess(Action<float> process)
        {
            Process += process;
            _currentTransition.Process += process;
        }

        public void AddTransitionEnded(Action transitionEnded)
        {
            TransitionEnded += transitionEnded;
            _currentTransition.TransitionEnded += transitionEnded;
        }

        public void AddTransitionPaused(Action transitionPaused)
        {
            TransitionPaused += transitionPaused;
            _currentTransition.TransitionPaused += transitionPaused;
        }

        public void Restart()
        {
            Cancel();
            _currentTransition = _delay > 0 ? GetDelayTransition(_delay) : GetNextTransition();
        }

        public void Pause()
        {
            _currentTransition?.Pause();
        }

        public void Cancel()
        {
            _currentTransition?.Cancel();
            _currentKey = 0;
            _iterations = _initialIterations;
        }

        public void Continue()
        {
            _currentTransition?.Continue();
        }
    }
}
