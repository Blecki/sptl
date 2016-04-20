using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public class ChunkedMap<Cell, ChunkTag> 
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int ChunkWidth { get; private set; }
        public int ChunkHeight { get; private set; }
        public int ChunksX { get; private set; }
        public int ChunksY { get; private set; }
        public int CellWidth { get; private set; }
        public int CellHeight { get; private set; }
        public int PixelWidth { get { return Width * CellWidth; } }
        public int PixelHeight { get { return Height * CellHeight; } }

        public ChunkedMap(int Width, int Height, int ChunkWidth, int ChunkHeight, 
            int CellWidth, int CellHeight,
            Func<int, int, Cell> CellCreator,
            Func<TaggedGrid, ChunkTag> TagCreator)
        {
            this.Width = Width;
            this.Height = Height;
            this.ChunkWidth = ChunkWidth;
            this.ChunkHeight = ChunkHeight;
            this.ChunksX = Width / ChunkWidth;
            this.ChunksY = Height / ChunkHeight;
            this.CellWidth = CellWidth;
            this.CellHeight = CellHeight;

            Data = new Grid<TaggedGrid>(0, 0, ChunksX, ChunksY, (x, y) => new TaggedGrid(x, y, ChunkWidth, ChunkHeight, TagCreator, CellCreator));
        }

        public class Grid<T>
        {
            public int OffsetX { get; private set; }
            public int OffsetY { get; private set; }
            public int Width { get; private set; }
            public int Height { get; private set; }

            private T[] Cells;

            public Grid(int X, int Y, int Width, int Height, Func<int, int, T> CellCreator)
            {
                this.OffsetX = X;
                this.OffsetY = Y;
                this.Width = Width;
                this.Height = Height;

                Cells = new T[Width * Height];

                ForAll((c, x, y) => this[x, y] = CellCreator(X + x, Y + y));
            }

            public int Normalize(int x, int y)
            {
                return (y * Width) + x;
            }

            public T this[int x, int y]
            {
                get { return Cells[Normalize(x, y)]; }
                set { Cells[Normalize(x, y)] = value; }
            }

            public void ForAll(Action<T, int, int> func)
            {
                for (var x = 0; x < Width; ++x)
                    for (var y = 0; y < Height; ++y)
                        func(this[x, y], x, y);
            }
        }

        public class TaggedGrid : Grid<Cell>
        {
            internal ChunkTag Tag;

            public TaggedGrid(int X, int Y, int Width, int Height, Func<TaggedGrid, ChunkTag> TagCreator, Func<int, int, Cell> CellCreator)
                : base(X, Y, Width, Height, CellCreator)
            {
                this.Tag = TagCreator(this);
            }
        }

        private Grid<TaggedGrid> Data;

        public struct ChunkCoordinate
        {
            public int X;
            public int Y;

            public ChunkCoordinate(int X, int Y)
            {
                this.X = X;
                this.Y = Y;
            }
        }

        public struct CellCoordinate
        {
            public int X;
            public int Y;

            public CellCoordinate(int X, int Y)
            {
                this.X = X;
                this.Y = Y;
            }
        }

        public struct InternalCoordinate
        {
            public int X;
            public int Y;

            public InternalCoordinate(int X, int Y)
            {
                this.X = X;
                this.Y = Y;
            }
        }

        public struct WorldCoordinate
        {
            public int X;
            public int Y;

            public WorldCoordinate(int X, int Y)
            {
                this.X = X;
                this.Y = Y;
            }
        }

        public ChunkCoordinate CellToChunk(CellCoordinate C)
        {
            return new ChunkCoordinate(C.X / ChunkWidth, C.Y / ChunkHeight);
        }

        public InternalCoordinate CellToInternal(CellCoordinate C)
        {
            return new InternalCoordinate(C.X % ChunkWidth, C.Y % ChunkHeight);
        }

        public WorldCoordinate CellToWorld(CellCoordinate C)
        {
            return new WorldCoordinate(C.X * CellWidth, C.Y * CellHeight);
        }

        public CellCoordinate WorldToCell(WorldCoordinate C)
        {
            var r = new CellCoordinate(C.X / CellWidth, C.Y / CellHeight);
            if (C.X < 0) r.X -= 1;
            if (C.Y < 0) r.Y -= 1;
            return r;
        }

        public CellCoordinate GetWrappedCellCoordinate(CellCoordinate C)
        {
            var x = C.X % Width;
            if (x < 0) x = Width + x;

            var y = C.Y % Height;
            if (y < 0) y = Height + y;

            return new CellCoordinate(x, y);
        }

        public ChunkCoordinate GetWrappedChunkCoordinate(ChunkCoordinate C)
        {
            var x = C.X % ChunksX;
            if (x < 0) x = ChunksX + x;

            var y = C.Y % ChunksY;
            if (y < 0) y = ChunksY + y;

            return new ChunkCoordinate(x, y);
        }

        public void ForEachChunk(Action<ChunkTag, int, int> Callback)
        {
            Data.ForAll((grid, x, y) =>
            {
                Callback(grid.Tag, x, y);
            });
        }

        public ChunkTag GetChunk(ChunkCoordinate Coordinate)
        {
            Coordinate = GetWrappedChunkCoordinate(Coordinate);
            return Data[Coordinate.X, Coordinate.Y].Tag;
        }

        public Cell GetCell(CellCoordinate Coordinate)
        {
            Coordinate = GetWrappedCellCoordinate(Coordinate);
            var chunkCoordinate = CellToChunk(Coordinate);
            var internalCoordinate = CellToInternal(Coordinate);
            return Data[chunkCoordinate.X, chunkCoordinate.Y][internalCoordinate.X, internalCoordinate.Y];
        }

        public Cell GetCellUnsafe(Coordinate Coordinate)
        {
            return GetCell(Coordinate.X, Coordinate.Y);
        }

        public Cell GetCell(int X, int Y)
        {
            return GetCell(new CellCoordinate(X, Y));
        }

        public Cell GetCellAtWorld(int X, int Y)
        {
            return GetCell(WorldToCell(new WorldCoordinate(X, Y)));
        }

        public ChunkTag GetChunkForCellAtWorld(int X, int Y)
        {
            var coordinate = new WorldCoordinate(X, Y);
            return GetChunk(CellToChunk(GetWrappedCellCoordinate(WorldToCell(coordinate))));
        }

        public ChunkTag GetChunkForCellAt(Coordinate C)
        {
            return GetChunkForCellAt(C.X, C.Y);
        }

        public ChunkTag GetChunkForCellAt(int X, int Y)
        {
            var coordinate = new CellCoordinate(X, Y);
            return GetChunk(CellToChunk(GetWrappedCellCoordinate(coordinate)));
        }

        public void ForEachChunkInWorldRect(float X, float Y, float W, float H, Action<ChunkTag, int, int> Callback)
        {
            var cX = (int)System.Math.Floor(X / CellWidth);
            var cY = (int)System.Math.Floor(Y / CellHeight);

            if (cX < 0) cX -= ChunkWidth;
            if (cY < 0) cY -= ChunkHeight;

            cX /= ChunkWidth;
            cY /= ChunkHeight;

            for (var _x = cX; (_x * ChunkWidth * CellWidth) < X + W; ++_x)
                for (var _y = cY; (_y * ChunkHeight * CellHeight) < Y + H; ++_y)
                    Callback(GetChunk(new ChunkCoordinate(_x, _y)), _x * ChunkWidth * CellWidth, _y * ChunkHeight * CellHeight);
        }

        public void ForEachCellInWorldRect(float X, float Y, float Width, float Height, Action<Cell, int, int> Callback)
        {
            for (var _x = (int)Math.Floor(X / CellWidth); (_x * CellWidth) < X + Width; ++_x)
                for (var _y = (int)Math.Floor(Y / CellHeight); (_y * CellWidth) < Y + Height; ++_y)
                    Callback(GetCell(new CellCoordinate(_x, _y)), _x, _y);
        }
    }
}
