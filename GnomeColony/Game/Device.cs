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
        public bool Source = false;
        public bool Sink = false;
        public int ID = 0;
    }

    public class DeviceSignalActivation
    {
        public Func<Play, bool> Condition;
        public Coordinate Location;
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

        public Func<int, Wire, Play, List<DeviceSignalActivation>> OnSignal = null;
    }

    public static class StaticDevices
    {
        public static List<Device> RootDevices;

        private static List<DeviceSignalActivation> ActivationList(params DeviceSignalActivation[] Activations)
        {
            return Activations.ToList();
        }

        private static bool _always(Play Game)
        {
            return true;
        }
        
        public static void InitializeStaticDevices()
        {            
            var crossover = new Device
            {
                Width = 2,
                Height = 2,
                Cells = new DeviceCell[4]
                {
                    new DeviceCell { TileIndex = 32, Terminal = true, ID = 0 },
                    new DeviceCell { TileIndex = 33, Terminal = true, ID = 1 },
                    new DeviceCell { TileIndex = 48, Terminal = true, ID = 2 },
                    new DeviceCell { TileIndex = 49, Terminal = true, ID = 3 }
                },
                OnSignal = (signal, wire, game) =>
                {
                    if (wire.Cell.ID == 0) return ActivationList(new DeviceSignalActivation { Condition = _always, Location = wire.Coordinate.Offset(1, 1) });
                    if (wire.Cell.ID == 1) return ActivationList(new DeviceSignalActivation { Condition = _always, Location = wire.Coordinate.Offset(-1, 1) });
                    if (wire.Cell.ID == 2) return ActivationList(new DeviceSignalActivation { Condition = _always, Location = wire.Coordinate.Offset(1, -1) });
                    if (wire.Cell.ID == 3) return ActivationList(new DeviceSignalActivation { Condition = _always, Location = wire.Coordinate.Offset(-1, -1) });
                    else return ActivationList();
                }
            };

            crossover.Rotated = crossover;

            var verticalTransistor = new Device
            {
                Width = 1,
                Height = 3,
                Cells = new DeviceCell[3]
                {
                    new DeviceCell { TileIndex = 34, Terminal = true, ID = 0 },
                    new DeviceCell { TileIndex = 50, Terminal = true, ID = 1 },
                    new DeviceCell { TileIndex = 66, Terminal = true, ID = 2 }
                },
                OnSignal = (signal, wire, game) =>
                {
                    if (wire.Cell.ID == 0 || wire.Cell.ID == 2)
                    {
                        var oppositeEndOffset = wire.Cell.ID == 0 ? 2 : -2;
                        return ActivationList(new DeviceSignalActivation
                        {
                            Location = wire.Coordinate.Offset(0, oppositeEndOffset),
                            Condition = (g) => g.WireMap.GetCellUnsafe(wire.Coordinate.Offset(0, wire.Cell.ID == 0 ? 1 : -1)).Signal == signal
                        });
                    }
                    else return ActivationList();
                }
            };

            var horizontalTransistor = new Device
            {
                Width = 3,
                Height = 1,
                Cells = new DeviceCell[3]
                {
                    new DeviceCell { TileIndex = 35, Terminal = true, ID = 0 },
                    new DeviceCell { TileIndex = 36, Terminal = true, ID = 1 },
                    new DeviceCell { TileIndex = 37, Terminal = true, ID = 2 }
                },
                OnSignal = (signal, wire, game) =>
                {
                    if (wire.Cell.ID == 0 || wire.Cell.ID == 2)
                    {
                        var oppositeEndOffset = wire.Cell.ID == 0 ? 2 : -2;
                        return ActivationList(new DeviceSignalActivation
                        {
                            Location = wire.Coordinate.Offset(oppositeEndOffset, 0),
                            Condition = (g) => g.WireMap.GetCellUnsafe(wire.Coordinate.Offset(wire.Cell.ID == 0 ? 1 : -1, 0)).Signal == signal
                        });
                    }
                    else return ActivationList();
                }
            };

            verticalTransistor.Rotated = horizontalTransistor;
            horizontalTransistor.Rotated = verticalTransistor;
            
            var negative = new Device
            {
                Width = 1,
                Height = 1,
                Cells = new DeviceCell[1]
                {
                    new DeviceCell { TileIndex = 51, Terminal = true, Source = true }
                }
            };

            negative.Rotated = negative;

            RootDevices = new List<Device>();
            RootDevices.Add(crossover);
            RootDevices.Add(negative);
            RootDevices.Add(verticalTransistor);
        }
    }
}
