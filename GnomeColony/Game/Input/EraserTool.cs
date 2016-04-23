using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Game.Input
{
    public class EraserTool : ITool
    {
        public void Update(Play Game, MainInputState InputState)
        {
            InputState.ShowEditableWireRange = true;

            if (Game.Input.Check("ESCAPE"))
            {
                InputState.ActiveTool = new DefaultInteractionTool();
                return;
            }

            if (Game.Input.Check("LEFTPRESS"))
            {
                var TileUnderMouse = Game.WireMap.GetWrappedCellCoordinate(
                        Game.WireMap.WorldToCell(
                            new ChunkedMap<Wire, MapChunkRenderBuffer<Wire>>.WorldCoordinate((int)Game.MouseWorldPosition.X, (int)Game.MouseWorldPosition.Y)));

                var cell = Game.WireMap.GetCellAtWorld((int)Game.MouseWorldPosition.X, (int)Game.MouseWorldPosition.Y);
                
                if (cell.Device != null)
                {
                    var device = cell.Device;
                    for (var x = 0; x < device.Width; ++x)
                        for (var y = 0; y < device.Height; ++y)
                        {
                            var dCell = Game.WireMap.GetCell(x + cell.DeviceRoot.X, y + cell.DeviceRoot.Y);
                            dCell.Device = null;
                            dCell.Cell = null;
                            Game.WireMap.GetChunkForCellAt(x + cell.DeviceRoot.X, y + cell.DeviceRoot.Y).InvalidateMesh();
                        }
                }

                cell.Connections = 0;

                Game.WireMap.GetCell(cell.Coordinate.X, cell.Coordinate.Y - 1).Connections &= ~Wire.DOWN;
                Game.WireMap.GetCell(cell.Coordinate.X, cell.Coordinate.Y + 1).Connections &= ~Wire.UP;
                Game.WireMap.GetCell(cell.Coordinate.X + 1, cell.Coordinate.Y).Connections &= ~Wire.LEFT;
                Game.WireMap.GetCell(cell.Coordinate.X - 1, cell.Coordinate.Y).Connections &= ~Wire.RIGHT;

            }
        }        
    }
}
