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

            var layout = new Gem.Gui.FlowLayout(new Rectangle(16, 600 - 128 - 16, 800 - 32, 128), 4);

            GuiRoot.Children.Add(frame);

            for (int x = 0; x < 7 && x < StaticDevices.RootDevices.Count; ++x)
            {
                var device = StaticDevices.RootDevices[x];

                var placement = layout.PositionItem(device.Width * 32, device.Height * 32);

                var icon = new Gui.UIItem();
                icon.Shape = Gui.Shape.CreateQuad(placement.X, placement.Y, placement.Width, placement.Height);

                var deviceMesh = device.CreateMesh(Game.WireSet);
                Mesh.Transform(deviceMesh, Matrix.CreateScale(32));
                Mesh.Transform(deviceMesh, Matrix.CreateTranslation(placement.X, placement.Y, 0.0f));

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

        public override void Update(Play Game)
        {
            if (Game.Input.Check("ESCAPE"))
                Game.PopInputState();
            else
                base.Update(Game);
        }
    }
}
