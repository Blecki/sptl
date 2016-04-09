using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Game
{
    public class DeviceCell
    {
        public int TileIndex = 0;
        public bool Terminal = false;

        // Internal connections?
        
    }

    public class Device
    {
        public int Width;
        public int Height;

        public Device Rotated;

        public DeviceCell[] Cells;

        public Mesh CreateMesh(TileSheet Tiles)
        {
            var parts = new List<Mesh>();
            for (var x = 0; x < Width; ++x)
                for (var y = 0; y < Height; ++y)
                {
                    var cell = Cells[(y * Width) + x];
                    var part = Mesh.CreateSpriteQuad();
                    var uvM = Tiles.TileMatrix(cell.TileIndex);
                    Mesh.MorphEx(part, (v) => new Vertex { Position = v.Position, TextureCoordinate = Vector2.Transform(v.TextureCoordinate, uvM) });
                    Mesh.Transform(part, Matrix.CreateTranslation(x, y, 0.0f));
                    parts.Add(part);
                }

            return Mesh.Merge(parts.ToArray());
        }
    }

    public static class StaticDevices
    {
        public static List<Device> RootDevices;

        public static void InitializeStaticDevices()
        {
            var battery1 = new Device
            {
                Width = 1,
                Height = 2,
                Cells = new DeviceCell[2]
            {
                new DeviceCell { TileIndex = 16, Terminal = true },
                new DeviceCell { TileIndex = 32, Terminal = true }
            }
            };

            var battery2 = new Device
            {
                Width = 2,
                Height = 1,
                Cells = new DeviceCell[2]
            {
                new DeviceCell { TileIndex = 17, Terminal = true },
                new DeviceCell { TileIndex = 18, Terminal = true }
            }
            };

            var battery3 = new Device
            {
                Width = 1,
                Height = 2,
                Cells = new DeviceCell[2]
            {
                new DeviceCell { TileIndex = 19, Terminal = true },
                new DeviceCell { TileIndex = 35, Terminal = true }
            }
            };

            var battery4 = new Device
            {
                Width = 2,
                Height = 1,
                Cells = new DeviceCell[2]
            {
                new DeviceCell { TileIndex = 33, Terminal = true },
                new DeviceCell { TileIndex = 34, Terminal = true }
            }
            };

            battery1.Rotated = battery2;
            battery2.Rotated = battery3;
            battery3.Rotated = battery4;
            battery4.Rotated = battery1;

            RootDevices = new List<Device>();
            RootDevices.Add(battery1);
        }
    }
}
