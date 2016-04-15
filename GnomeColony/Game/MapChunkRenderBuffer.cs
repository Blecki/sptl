using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Game
{
    public class MapChunkRenderBuffer<Cell>
    {
        public ChunkedMap<Cell, MapChunkRenderBuffer<Cell>>.TaggedGrid Grid { get; private set; }
        public TileSheet TileSheet { get; private set; }
        public int TileWidth { get; private set; }
        public int TileHeight { get; private set; }

        private Func<Cell, Object> TileIndexSelector;
        private Mesh Mesh = null;

        public MapChunkRenderBuffer(
            ChunkedMap<Cell, MapChunkRenderBuffer<Cell>>.TaggedGrid Grid,
            TileSheet TileSheet,
            Func<Cell, Object> TileIndexSelector,
            int TileWidth,
            int TileHeight)
        {
            this.Grid = Grid;
            this.TileSheet = TileSheet;
            this.TileIndexSelector = TileIndexSelector;
            this.TileWidth = TileWidth;
            this.TileHeight = TileHeight;
        }

        public void InvalidateMesh()
        {
            Mesh = null;
        }

        private Mesh CreateTileMesh(int X, int Y, int TileIndex)
        {
            var geometry = Mesh.CreateSpriteQuad();
            Mesh.Transform(geometry, Matrix.CreateScale(TileWidth, TileHeight, 1.0f));
            Mesh.Transform(geometry, Matrix.CreateTranslation(X * TileWidth, Y * TileHeight, 0.0f));

            var uvTransform = TileSheet.TileMatrix(TileIndex);
            Mesh.MorphEx(geometry, v => new Vertex
            {
                Position = v.Position,
                TextureCoordinate = Vector2.Transform(v.TextureCoordinate, uvTransform)
            });

            return geometry;
        }

        public void UpdateMesh()
        {
            if (Mesh == null)
            {
                var tileMeshes = new List<Mesh>();

                Grid.ForAll((cell, x, y) =>
                {
                    var tileIndexObject = TileIndexSelector(cell);

                    if (tileIndexObject is int)
                    {
                        var tileIndex = tileIndexObject as int?;
                        if (tileIndex.Value == -1) return;
                        tileMeshes.Add(CreateTileMesh(x, y, tileIndex.Value));
                    }
                    else if (tileIndexObject is int[])
                    {
                        var tileIndexes = tileIndexObject as int[];
                        foreach (var index in tileIndexes)
                            if (index != -1) tileMeshes.Add(CreateTileMesh(x, y, index));
                    }

                });

                Mesh = Mesh.Merge(tileMeshes.ToArray());
            }
        }

        public void Render(GraphicsDevice Device)
        {
            UpdateMesh();
            Mesh.Render(Device);
        }
    }
}
