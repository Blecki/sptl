using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Game
{
    public enum ShadowEdgeDirection
    {
        Up,
        Right,
        Down,
        Left,
        Internal,
    }

    public class ShadowEdge
    {
        public Vector2 V0;
        public Vector2 V1;
        public ShadowEdgeDirection Direction;

        public ShadowEdge(Vector2 V0, Vector2 V1, ShadowEdgeDirection Direction)
        {
            this.V0 = V0;
            this.V1 = V1;
            this.Direction = Direction;
        }

        public static Coordinate EdgeNeighbor(Coordinate Of, ShadowEdgeDirection Direction)
        {
            switch (Direction)
            {
                case ShadowEdgeDirection.Up:
                    return new Coordinate(Of.X, Of.Y - 1);
                case ShadowEdgeDirection.Right:
                    return new Coordinate(Of.X + 1, Of.Y);
                case ShadowEdgeDirection.Down:
                    return new Coordinate(Of.X, Of.Y + 1);
                case ShadowEdgeDirection.Left:
                    return new Coordinate(Of.X - 1, Of.Y);
                case ShadowEdgeDirection.Internal:
                    return Of;
                default:
                    return Of;
            }
        }

        public static ShadowEdgeDirection Opposite(ShadowEdgeDirection Direction)
        {
            switch (Direction)
            {
                case ShadowEdgeDirection.Up:
                    return ShadowEdgeDirection.Down;
                case ShadowEdgeDirection.Right:
                    return ShadowEdgeDirection.Left;
                case ShadowEdgeDirection.Down:
                    return ShadowEdgeDirection.Up;
                case ShadowEdgeDirection.Left:
                    return ShadowEdgeDirection.Right;
                default:
                    return Direction;
            }
        }
    }

    public class TileTemplate
    {
        public bool CastShadow = false;
        public bool Solid = true;
        public ShadowEdge[] ShadowEdges;
        public Vector2[] CollisionPoints;
        public int TileIndex;
    }
}
