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
                Mesh.Transform(DeviceMesh, Matrix.CreateScale(8));
            }
            
            DiffuseEffect.Parameters["World"].SetValue(Matrix.CreateTranslation(Game.MouseWorldPosition.X, Game.MouseWorldPosition.Y, 1.0f));
            DiffuseEffect.Parameters["Texture"].SetValue(Game.WireSet.Texture);
            DiffuseEffect.Parameters["UVTransform"].SetValue(Matrix.Identity);
            DiffuseEffect.CurrentTechnique.Passes[0].Apply();

            DeviceMesh.Render(GraphicsDevice);
        }

        public void Update(Play Game, MainInputState InputState)
        {
            if (Game.Input.Check("LEFTCLICK"))
            {
                // Place device on grid.
                InputState.ActiveTool = null;
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
