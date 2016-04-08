using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Gem;

namespace Game
{
    public struct Coordinate
    {
        public int X;
        public int Y;

        public Coordinate(int X, int Y)
        {
            this.X = X;
            this.Y = Y;
        }

        public Vector2 AsVector2()
        {
            return new Vector2(X, Y);
        }
        
        public static bool operator==(Coordinate A, Coordinate B)
        {
            return A.X == B.X && A.Y == B.Y;
        }

        public static bool operator !=(Coordinate A, Coordinate B)
        {
            return A.X != B.X || A.Y != B.Y;
        }

    }
}
