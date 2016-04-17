using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Game
{
    public class Wire 
    {
        public const int UP = 1;
        public const int LEFT = 8;
        public const int DOWN = 4;
        public const int RIGHT = 2;

        public int Connections = 0;
        public Coordinate Coordinate;

        public int Signal = 0;

        public Device Device;
        public DeviceCell Cell;
        public Coordinate DeviceRoot;
    }
}
