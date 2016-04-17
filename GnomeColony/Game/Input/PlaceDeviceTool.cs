using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Game.Input
{
    public class PlaceDeviceTool : ITool, Play.ITransientRenderItem
    {
        private Device Device;
        private Mesh DeviceMesh;

        public PlaceDeviceTool(Device Device, Play Game)
        {
            this.Device = Device;
            Game.AddTransientRenderItem(this);
        }      

        public void Render(GraphicsDevice GraphicsDevice, Effect DiffuseEffect, Play Game)
        {
            if (DeviceMesh == null)
            {
                DeviceMesh = Device.CreateMesh(Game.WireSet);
                Mesh.Transform(DeviceMesh, Matrix.CreateScale(Game.WireMap.CellWidth));
            }

            var tileUnderMouse = Game.WireMap.WorldToCell(
                            new ChunkedMap<Wire, MapChunkRenderBuffer<Wire>>.WorldCoordinate((int)Game.MouseWorldPosition.X, (int)Game.MouseWorldPosition.Y));

            var rejectPlacement = false;

            Game.WireMap.ForEachCellInWorldRect(
                   tileUnderMouse.X * Game.WireMap.CellWidth,
                   tileUnderMouse.Y * Game.WireMap.CellHeight,
                   Device.Width * Game.WireMap.CellWidth,
                   Device.Height * Game.WireMap.CellHeight,
                   (w, x, y) =>
                   {
                       if (w.Device != null)
                           rejectPlacement = true;
                   });

            if (!rejectPlacement)
            {
                DiffuseEffect.Parameters["World"].SetValue(Matrix.CreateTranslation(tileUnderMouse.X * Game.WireMap.CellWidth, tileUnderMouse.Y * Game.WireMap.CellHeight, 0.0f));
                DiffuseEffect.Parameters["Texture"].SetValue(Game.WireSet.Texture);
                DiffuseEffect.Parameters["UVTransform"].SetValue(Matrix.Identity);
                DiffuseEffect.CurrentTechnique.Passes[0].Apply();

                DeviceMesh.Render(GraphicsDevice);
            }
        }

        public void Update(Play Game, MainInputState InputState)
        {
            if (Game.Input.Check("ESCAPE"))
            {
                InputState.ActiveTool = new DefaultInteractionTool();
                return;
            }

            if (Game.Input.Check("LEFTCLICK"))
            {
                // Place device on grid.
                var tileUnderMouse = Game.WireMap.WorldToCell(
                    new ChunkedMap<Wire, MapChunkRenderBuffer<Wire>>.WorldCoordinate((int)Game.MouseWorldPosition.X, (int)Game.MouseWorldPosition.Y));

                var rejectPlacement = false;

                Game.WireMap.ForEachCellInWorldRect(
                   tileUnderMouse.X * Game.WireMap.CellWidth,
                   tileUnderMouse.Y * Game.WireMap.CellHeight,
                   Device.Width * Game.WireMap.CellWidth,
                   Device.Height * Game.WireMap.CellHeight,
                   (w, x, y) =>
                   {
                       if (w.Device != null)
                           rejectPlacement = true;
                   });

                if (!rejectPlacement)
                {
                    Game.WireMap.ForEachCellInWorldRect(
                        tileUnderMouse.X * Game.WireMap.CellWidth,
                        tileUnderMouse.Y * Game.WireMap.CellHeight,
                        Device.Width * Game.WireMap.CellWidth,
                        Device.Height * Game.WireMap.CellHeight,
                        (w, x, y) =>
                        {
                            w.Device = Device;
                            var cellIndex = ((y - tileUnderMouse.Y) * Device.Width) + (x - tileUnderMouse.X);
                            w.Cell = Device.Cells[cellIndex];
                            var chunk = Game.WireMap.GetChunkForCellAt(x, y);
                            w.DeviceRoot = new Coordinate(tileUnderMouse.X, tileUnderMouse.Y);
                            chunk.InvalidateMesh();
                        });
                }

                InputState.ActiveTool = new DefaultInteractionTool();
                Game.RemoveTransientRenderItem(this);
            }
            else if (Game.Input.Check("ROTATEDEVICE"))
            {
                Device = Device.Rotated;
                DeviceMesh = null;
            }
        }
    }
}
