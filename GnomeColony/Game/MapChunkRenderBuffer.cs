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

        private Func<Cell, int> TileIndexSelector;
        private Mesh Mesh = null;

        public MapChunkRenderBuffer(
            ChunkedMap<Cell, MapChunkRenderBuffer<Cell>>.TaggedGrid Grid,
            TileSheet TileSheet,
            Func<Cell, int> TileIndexSelector,
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
        
        public void UpdateMesh()
        {
            if (Mesh == null)
            {
                var tileMeshes = new List<Mesh>();

                Grid.ForAll((cell, x, y) =>
                {
                    var tileIndex = TileIndexSelector(cell);
                    if (tileIndex == -1) return;

                    var geometry = Mesh.CreateSpriteQuad();
                    Mesh.Transform(geometry, Matrix.CreateScale(TileWidth, TileHeight, 1.0f));
                    Mesh.Transform(geometry, Matrix.CreateTranslation(x * TileWidth, y * TileHeight, 0.0f));

                    var uvTransform = TileSheet.TileMatrix(tileIndex);
                    Mesh.MorphEx(geometry, v => new Vertex
                    {
                        Position = v.Position,
                        TextureCoordinate = Vector2.Transform(v.TextureCoordinate, uvTransform)
                    });

                    tileMeshes.Add(geometry);
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
