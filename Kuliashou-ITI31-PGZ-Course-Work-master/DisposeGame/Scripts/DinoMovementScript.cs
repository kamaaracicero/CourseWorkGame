using GameEngine.Scripts;
using SharpDX.DirectInput;

namespace DisposeGame.Scripts
{
    public class DinoMovementScript : KeyboardListenerScript
    {
        public DinoMovementScript()
        {
            Actions.Add(Key.Left, delta => GameObject.MoveBy(-0.1f, 0, 0));
            Actions.Add(Key.Right, delta => GameObject.MoveBy(0.1f, 0, 0));
            Actions.Add(Key.Up, delta => GameObject.MoveBy(0, 0, 0.1f));
            Actions.Add(Key.Down, delta => GameObject.MoveBy(0, 0, -0.1f));
            Actions.Add(Key.NumberPad8, delta => GameObject.MoveBy(0, 0.1f, 0));
            Actions.Add(Key.NumberPad2, delta => GameObject.MoveBy(0, -0.1f, 0));
            Actions.Add(Key.NumberPad7, delta => GameObject.RotateX(0.01f));
            Actions.Add(Key.NumberPad9, delta => GameObject.RotateX(-0.01f));
            Actions.Add(Key.NumberPad4, delta => GameObject.RotateY(0.01f));
            Actions.Add(Key.NumberPad6, delta => GameObject.RotateY(-0.01f));
            Actions.Add(Key.NumberPad1, delta => GameObject.RotateZ(0.01f));
            Actions.Add(Key.NumberPad3, delta => GameObject.RotateZ(-0.01f));
        }
    }
}
