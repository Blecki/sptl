using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Game.Input
{
    public class WirePaintTool : ITool
    {
        bool MouseDown = false;
        Vector2 MouseStart;
        Vector2 MouseEnd;

        Gem.Pathfinding<Wire> Pathfinder;

        ChunkedMap<Wire, MapChunkRenderBuffer<Wire>>.CellCoordinate[] Offsets = new ChunkedMap<Wire, MapChunkRenderBuffer<Wire>>.CellCoordinate[]
        {
            new ChunkedMap<Wire, MapChunkRenderBuffer<Wire>>.CellCoordinate(0, -1),
            new ChunkedMap<Wire, MapChunkRenderBuffer<Wire>>.CellCoordinate(1, 0),
            new ChunkedMap<Wire, MapChunkRenderBuffer<Wire>>.CellCoordinate(0, 1),
            new ChunkedMap<Wire, MapChunkRenderBuffer<Wire>>.CellCoordinate(-1, 0)
        };

        private int RelativeDirection(Coordinate A, Coordinate B)
        {
            var relativeX = B.X - A.X;
            if (relativeX < -1) return Wire.RIGHT;
            if (relativeX == -1) return Wire.LEFT;
            if (relativeX > 1) return Wire.LEFT;
            if (relativeX == 1) return Wire.RIGHT;

            var relativeY = B.Y - A.Y;
            if (relativeY < -1) return Wire.DOWN;
            if (relativeY == -1) return Wire.UP;
            if (relativeY > 1) return Wire.UP;
            if (relativeY == 1) return Wire.DOWN;

            return Wire.UP;
        }

        private int Opposite(int D)
        {
            switch (D)
            {
                case Wire.UP: return Wire.DOWN;
                case Wire.RIGHT: return Wire.LEFT;
                case Wire.DOWN: return Wire.UP;
                case Wire.LEFT: return Wire.RIGHT;
                default: return 0;
            }
        }

        public WirePaintTool(Play Game)
        {
            Pathfinder = new Gem.Pathfinding<Wire>(
                wire =>
                    Offsets.Select(o => new ChunkedMap<Wire, MapChunkRenderBuffer<Wire>>.CellCoordinate(wire.Coordinate.X + o.X, wire.Coordinate.Y + o.Y))
                    .Where(c => Game.WireMap.GetCell(c).Connections == 0)
                    .Select(c => Game.WireMap.GetCell(c))
                    .ToList(),
                wire => 1.0f);
        }

        public void Update(Play Game, MainInputState InputState)
        {
            if (Game.Input.Check("LEFTPRESS"))
            {
                if (!MouseDown)
                {
                    MouseStart = Game.MouseWorldPosition;
                    MouseDown = true;
                }
                else
                {
                    MouseEnd = Game.MouseWorldPosition;
                    // Trace path to draw wire.
                }
            }
            else
            {
                if (MouseDown)
                {
                    MouseDown = false;
                    MouseEnd = Game.MouseWorldPosition;

                    var pathresult = Pathfinder.Flood(Game.WireMap.GetCellAtWorld((int)MouseStart.X, (int)MouseStart.Y),
                        wire => Object.ReferenceEquals(wire, Game.WireMap.GetCellAtWorld((int)MouseEnd.X, (int)MouseEnd.Y)),
                        wire => 1.0f);

                    if (pathresult.GoalFound)
                    {
                        var path = pathresult.FinalNode.ExtractPath();
                        for (var x = 1; x < path.Count; ++x)
                        {
                            var a = path[x - 1];
                            var b = path[x];
                            var rel = RelativeDirection(a.Coordinate, b.Coordinate);
                            a.Connections |= rel;
                            b.Connections |= Opposite(rel);
                            Game.WireMap.GetChunkForCellAt(a.Coordinate.X, a.Coordinate.Y).InvalidateMesh();
                            Game.WireMap.GetChunkForCellAt(b.Coordinate.X, b.Coordinate.Y).InvalidateMesh();
                        }
                    }
                }
            }
        }

        public void Render(GraphicsDevice GraphicsDevice, Effect DiffuseEffect, Play Game)
        {
        }
    }
}
