using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Game.Input
{
    public class WirePaintTool : ITool, Play.ITransientRenderItem
    {
        bool MouseDown = false;
        Vector2 MouseStart;

        Gem.Pathfinding<Wire> Pathfinder;
        Gem.Pathfinding<Wire>.PathfindingResult Path = null;
        List<Wire> ExtractedPath = null;
        private Mesh TileMesh;
        ChunkedMap<Wire, MapChunkRenderBuffer<Wire>>.CellCoordinate TileUnderMouse;

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
                    .Where(c =>
                    {
                        var wrapped = Game.WireMap.GetWrappedCellCoordinate(c);
                        var cell = Game.WireMap.GetCell(wrapped);
                        if (wrapped.X == TileUnderMouse.X && wrapped.Y == TileUnderMouse.Y)
                        {
                            if (cell.Device == null) return true;
                            if (cell.Cell.Terminal) return true;
                            return false;
                        }
                        if (cell.Connections != 0) return false;
                        if (cell.Device != null) return false;
                        return true;
                    })
                    .Select(c => Game.WireMap.GetCell(c))
                    .ToList(),
                wire => 1.0f);

            TileMesh = Mesh.CreateSpriteQuad();
            Mesh.Transform(TileMesh, Matrix.CreateScale(Game.WireMap.CellWidth));
        }

        public void Update(Play Game, MainInputState InputState)
        {
            if (Game.Input.Check("ESCAPE"))
            {
                InputState.ActiveTool = new DefaultInteractionTool();
                Game.RemoveTransientRenderItem(this);
                return;
            }

            if (Game.Input.Check("LEFTPRESS"))
            {
                if (!MouseDown)
                {
                    MouseStart = Game.MouseWorldPosition;

                    TileUnderMouse = Game.WireMap.GetWrappedCellCoordinate(
                        Game.WireMap.WorldToCell(
                            new ChunkedMap<Wire, MapChunkRenderBuffer<Wire>>.WorldCoordinate((int)MouseStart.X, (int)MouseStart.Y)));

                    Game.AddTransientRenderItem(this);
                    Path = null;
                    ExtractedPath = null;
                    MouseDown = true;
                }
                else
                {
                    var nextTileUnderMouse = Game.WireMap.GetWrappedCellCoordinate(
                        Game.WireMap.WorldToCell(
                            new ChunkedMap<Wire, MapChunkRenderBuffer<Wire>>.WorldCoordinate((int)Game.MouseWorldPosition.X, (int)Game.MouseWorldPosition.Y)));

                    if (Path == null || TileUnderMouse.X != nextTileUnderMouse.X || TileUnderMouse.Y != nextTileUnderMouse.Y)
                    {
                        TileUnderMouse = nextTileUnderMouse;

                        Path = Pathfinder.Flood(Game.WireMap.GetCellAtWorld((int)MouseStart.X, (int)MouseStart.Y),
                           wire => Object.ReferenceEquals(wire, Game.WireMap.GetCell(TileUnderMouse)),
                           wire => 1.0f);

                        if (Path.GoalFound)
                        {
                            ExtractedPath = Path.FinalNode.ExtractPath();

                            // Disallow internal connections within devices. The only possibility we need to
                            // worry about is a path two tiles long that connects a device to itself.
                            if (ExtractedPath.Count == 2)
                            {
                                var start = ExtractedPath[0];
                                var end = ExtractedPath[1];
                                if (start.Device != null && end.Device != null)
                                    if (start.DeviceRoot.X == end.DeviceRoot.X && start.DeviceRoot.Y == end.DeviceRoot.Y)
                                        ExtractedPath = null;
                            }
                        }
                        else
                            ExtractedPath = null;                        
                    }
                }
            }
            else
            {
                if (MouseDown)
                {
                    MouseDown = false;
                    Game.RemoveTransientRenderItem(this);
                   
                    if (ExtractedPath != null)
                    {
                        for (var x = 1; x < ExtractedPath.Count; ++x)
                        {
                            var a = ExtractedPath[x - 1];
                            var b = ExtractedPath[x];
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
            var topLeft = Game.Camera.Unproject(Vector3.Zero);
            var bottomRight = Game.Camera.Unproject(new Vector3(800, 600, 0));

            if (ExtractedPath != null)
            {
                for (var x = 0; x < ExtractedPath.Count; ++x)
                {
                    Wire a = x == 0 ? null : ExtractedPath[x - 1];
                    Wire b = ExtractedPath[x];
                    Wire c = x == (ExtractedPath.Count - 1) ? null : ExtractedPath[x + 1];

                    var connections = b.Connections;
                    if (a != null) connections |= RelativeDirection(b.Coordinate, a.Coordinate);
                    if (c != null) connections |= RelativeDirection(b.Coordinate, c.Coordinate);

                    float _x = b.Coordinate.X * Game.WireMap.CellWidth;
                    float _y = b.Coordinate.Y * Game.WireMap.CellWidth;

                    if (_x >= bottomRight.X) _x -= Game.WireMap.PixelWidth;
                    if (_y >= bottomRight.Y) _y -= Game.WireMap.PixelHeight;

                    DiffuseEffect.Parameters["World"].SetValue(Matrix.CreateTranslation(_x, _y, 0.0f));
                    DiffuseEffect.Parameters["UVTransform"].SetValue(Game.WireSet.TileMatrix(connections));
                    DiffuseEffect.Parameters["Texture"].SetValue(Game.WireSet.Texture);
                    DiffuseEffect.CurrentTechnique.Passes[0].Apply();
                    TileMesh.Render(GraphicsDevice);
                }
            }
        }
    }
}
