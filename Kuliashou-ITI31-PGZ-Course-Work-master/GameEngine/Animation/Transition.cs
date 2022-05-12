using SharpDX;
using System;
using System.Collections.Generic;

namespace GameEngine.Animation
{
    public class Transition
    {
        public event Action<float> Process;
        public event Action TransitionEnded;
        public event Action TransitionPaused;

        private static List<Transition> _transitions = new List<Transition>();
        private static List<Transition> _transitionsToAdd = new List<Transition>();

        private float _start;
        private float _end;
        private float _duration;
        private float _time;

        private bool _isPaused;
        private bool _isCanceled;

        public virtual float CurrentTime { get => _time / _duration; }

        /// <summary>
        /// Transition class.
        /// </summary>
        /// <param name="start">Start value</param>
        /// <param name="end">End value</param>
        /// <param name="duration">Duration in seconds.</param>
        public Transition(float start, float end, float duration)
        {
            _start = start;
            _end = end;
            _duration = duration;
            _time = 0;
            _isPaused = false;
            _isCanceled = false;

            _transitionsToAdd.Add(this);
        }

        public static void UpdateTransitions(float delta)
        {
            _transitions.AddRange(_transitionsToAdd);
            _transitionsToAdd.Clear();
            for (int i = 0; i < _transitions.Count; i++)
            {
                if (_transitions[i].Update(delta))
                {
                    _transitions[i].TransitionEnded?.Invoke();
                    _transitions.RemoveAt(i--);
                }
            }
        }

        private bool Update(float delta)
        {
            if (_isCanceled) return true;
            if (_isPaused) return false;

            bool isRemoved = false;
            _time += delta;
            if (_time > _duration)
            {
                _time = _duration;
                isRemoved = true;
            }
            Process?.Invoke(MathUtil.Lerp(_start, _end, CurrentTime));
            return isRemoved;
        }

        public void Pause()
        {
            _isPaused = true;
            TransitionPaused?.Invoke();
        }

        public void Continue()
        {
            _isPaused = false;
        }

        public void Cancel()
        {
            _isCanceled = true;
            Process = null;
            TransitionEnded = null;
            TransitionPaused = null;
        }
    }
}
