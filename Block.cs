using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pong
{
    public class Block
    {
        public Vector2 V;                 // rectangle top-left position
        public Orientation O { get; set; }     // left block or right block    
        public int Points;
        public int Height { get; set; }
        public int Width { get; set; }

        public Block(Vector2 starting, bool left, int h, int w)
        {
            V = starting;
            O = new Orientation(left);
            Points = 0;
            Height = h;
            Width = w;
        }

        public void Move(bool up, int speed)
        {

            V.Y += (speed * (up ? -1 : 1));
        }

        // think that it would natural to go over, their fault not mine
        public bool WithinBoundaries(int bounds)
        {
            return ((this.V.Y + Height) <= bounds && this.V.Y >= 0);
        }

    }

}
