namespace GameEngine.Animation
{
    public class SmoothAnimation : Animation
    {
        public SmoothAnimation(float[] keys, float duration, int iterations = 1, float delay = -1)
            : base(keys, duration, iterations, delay)
        {
        }

        protected override Transition CreateTransition(float start, float end, float duration)
        {
            return new SmoothTransition(start, end, duration); 
        }
    }
}
