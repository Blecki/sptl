using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Game
{
    public class DeviceSelectInputState : GuiInputState
    {
        public Action<Device> OnSelection = null;

        public DeviceSelectInputState(GraphicsDevice Device, Play Game) : base(Device)
        {
            var frame = new Gui.UIItem();
            frame.Shape = Gui.Shape.CreateQuad(16, 600 - 128 - 16, 800 - 32, 128);
            frame.Texture = Blank;
            frame.Color = new Vector4(1, 0, 0, 1);

            GuiRoot.Children.Add(frame);

            for (int x = 0; x < 7 && x < StaticDevices.RootDevices.Count; ++x)
            {
                var device = StaticDevices.RootDevices[x];
                var icon = new Gui.UIItem();
                icon.Shape = Gui.Shape.CreateQuad(16 + 8 + (40 * x), 600 - 128, 32 * device.Width, 32 * device.Height);
                var deviceMesh = device.CreateMesh(Game.WireSet);
                Mesh.Transform(deviceMesh, Matrix.CreateScale(32));
                Mesh.Transform(deviceMesh, Matrix.CreateTranslation(24 + (40 * x), 600 - 128, 0.0f));

                icon.OnRender += (graphicsDevice, effect) =>
                {
                    effect.Parameters["DiffuseColor"].SetValue(Vector4.One);
                    effect.Parameters["Texture"].SetValue(Game.WireSet.Texture);
                    effect.Parameters["UVTransform"].SetValue(Matrix.Identity);
                    effect.CurrentTechnique.Passes[0].Apply();

                    deviceMesh.Render(graphicsDevice);
                };

                icon.OnClick += p =>
                {
                    if (OnSelection != null) OnSelection(device);
                };

                frame.Children.Add(icon);
            }
        }
    }
}
