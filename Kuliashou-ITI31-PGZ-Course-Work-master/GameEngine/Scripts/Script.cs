using GameEngine.Graphics;

namespace GameEngine.Scripts
{
    public abstract class Script
    {
        public Game3DObject GameObject { get; set; }

        public abstract void Update(float delta);
    }
}
