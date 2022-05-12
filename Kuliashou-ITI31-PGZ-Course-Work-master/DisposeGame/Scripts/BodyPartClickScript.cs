using GameEngine.Animation;
using GameEngine.Game;
using GameEngine.Scripts;
using SharpDX;

namespace DisposeGame.Scripts
{
    public class BodyPartClickScript : Script
    {
        private InputController _inputController;

        public BodyPartClickScript()
        {
            _inputController = InputController.GetInstance();
        }

        public override void Update(float delta)
        {
            if(_inputController.CursorRay != null)
            {
                if (_inputController.CursorRay.Value.Intersects(new BoundingSphere(GameObject.Position, 1)))
                {
                    Animation animation = new Animation(new float[] { 0, MathUtil.PiOverFour, -MathUtil.PiOverFour, 0 }, 4, 2, 2);
                    animation.AddProcess(value => 
                    {
                        GameObject.SetRotationY(value);
                    });
                    animation.AnimationIterationEnded += (() =>
                    {
                        GameObject.RotateZ(MathUtil.PiOverFour);
                    });

                    //Transition t1 = new SmoothTransition(0, MathUtil.PiOverFour, 1);
                    //t1.Process += value => GameObject.SetRotationY(value);
                    //t1.TransitionEnded += () =>
                    //{
                    //    Transition t2 = new SmoothTransition(MathUtil.PiOverFour, -MathUtil.PiOverFour, 2);
                    //    t2.Process += value => GameObject.SetRotationY(value);
                    //    t2.TransitionEnded += () =>
                    //    {
                    //        Transition t3 = new SmoothTransition(-MathUtil.PiOverFour, 0, 1);
                    //        t3.Process += value => GameObject.SetRotationY(value);
                    //    };
                    //};

                    _inputController.CursorRay = null;
                }
            }
        }
    }
}
