using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Game
{
    public class MainInputState : GuiInputState
    {
        private TextDisplay Text;
        private GraphicsDevice Device;
        public Input.ITool ActiveTool = null;

        public MainInputState(Play Game, GraphicsDevice Device, Gem.EpisodeContentManager Content) : base(Device)
        {
            this.Device = Device;
            Text = new TextDisplay(22, 32, new Gem.Gui.BitmapFont(Content.Load<Texture2D>("small-font"), 6, 8, 6), Device, Content);
            ActiveTool = new Input.DefaultInteractionTool();

            var selectorButton = new Gui.UIItem();
            selectorButton.Shape = Gui.Shape.CreateQuad(8, 256 + 16, 32, 32);
            selectorButton.Color = new Vector4(1, 1, 1, 1);
            selectorButton.Texture = Blank;
            selectorButton.OnClick += (p) =>
            {
                var tileSelector = new TileSelectInputState(Device, p);
                p.PushInputState(tileSelector);
                tileSelector.OnSelection += t =>
                {
                    p.PopInputState();
                    ActiveTool = new Input.TilePaintTool(t);
                };
            };

            GuiRoot.Children.Add(selectorButton);

            var wireButton = new Gui.UIItem();
            wireButton.Shape = Gui.Shape.CreateQuad(8, 256 + 16 + 4 + 32, 32, 32);
            wireButton.Color = new Vector4(1, 1, 1, 1);
            wireButton.Texture = Game.GuiSet.Texture;
            wireButton.UVTransform = Game.GuiSet.TileMatrix(0);
            wireButton.OnClick += (p) =>
            {
                ActiveTool = new Input.WirePaintTool(Game);
            };

            GuiRoot.Children.Add(wireButton);

            var deviceButton = new Gui.UIItem();
            deviceButton.Shape = Gui.Shape.CreateQuad(8, 256 + 16 + 4 + 32 + 32 + 4, 32, 32);
            deviceButton.Color = new Vector4(0, 1, 0, 1);
            deviceButton.Texture = Blank;
            deviceButton.OnClick += p =>
            {
                var deviceSelector = new DeviceSelectInputState(Device, p);
                p.PushInputState(deviceSelector);
                deviceSelector.OnSelection += d =>
                {
                    ActiveTool = new Input.PlaceDeviceTool(d, p);
                    p.PopInputState();
                };
            };

            GuiRoot.Children.Add(deviceButton);

            var eraseButton = new Gui.UIItem();
            eraseButton.Shape = Gui.Shape.CreateQuad(8, 256 + 16 + 4 + 32 + 32 + 4 + 36, 32, 32);
            eraseButton.Color = new Vector4(0, 1, 0, 1);
            eraseButton.Texture = Blank;
            eraseButton.OnClick += p => ActiveTool = new Input.EraserTool();

            GuiRoot.Children.Add(eraseButton);
        
    }

        public override void Render(GraphicsDevice Device, Effect DiffuseEffect, Play Game)
        {
            Text.SetString("Hello World!", 0, 0, Color.Black, Color.TransparentBlack);
            Text.Draw(Device, new Viewport(8, 8, 132, 256));

            base.Render(Device, DiffuseEffect, Game);

        }

        public override void Update(Play Game)
        {
            base.Update(Game);
            if (!MouseOverGui && ActiveTool != null)
                ActiveTool.Update(Game, this);
        }
    }
}
