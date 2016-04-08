using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game
{
    public class Grid<T>
    {
        public int width { get; private set; }
        public int height { get; private set; }

        private T[] tiles;

        public Grid(int width, int height, Func<int,int,T> CellCreator)
        {
            this.width = width;
            this.height = height;

            tiles = new T[this.width * this.height];

            forAll((c, x, y) => this[x, y] = CellCreator(x, y));
        }


        public int Normalize(int x, int y)
        {
            return (y * width) + x;
        }

        public T this[int x, int y]
        {
            get { return tiles[Normalize(x, y)]; }
            set { tiles[Normalize(x, y)] = value; }
        }

        public void forRect(int x, int y, int w, int h, Action<T, int, int> func)
        {
            for (var _x = (x < 0 ? 0 : x); _x < (x + w) && _x < width; ++_x)
                for (var _y = (y < 0 ? 0 : y); _y < (y + h) && _y < height; ++_y)
                        func(this[_x, _y], _x, _y);
        }

        public void forAll(Action<T, int, int> func)
        {
            forRect(0, 0, width, height, func);
        }

        public A abortedForRect<A>(int x, int y, int w, int h, Func<T, int, int, A> func)
        {
            for (var _x = (x < 0 ? 0 : x); _x < (x + w) && _x < width; ++_x)
                for (var _y = (y < 0 ? 0 : y); _y < (y + h) && _y < height; ++_y)
                    {
                        var _a = func(this[_x, _y], _x, _y);
                        if (_a != null) return _a;
                    }
            return default(A);
        }

        public T worldIndex(float x, float y)
        {
            return this[(int)System.Math.Floor(x), (int)System.Math.Floor(y)];
        }

        public bool check(int x, int y)
        {
            return x >= 0 && x < width && y >= 0 && y < height;
        }

        public bool worldCheck(float x, float y)
        {
            return check((int)System.Math.Floor(x), (int)System.Math.Floor(y));
        }

        
    }
}