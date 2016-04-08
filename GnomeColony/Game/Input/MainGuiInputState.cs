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
        private Input.ITool ActiveTool = null;

        public MainInputState(GraphicsDevice Device, Gem.EpisodeContentManager Content) : base(Device)
        {
            this.Device = Device;
            Text = new TextDisplay(22, 32, new Gem.Gui.BitmapFont(Content.Load<Texture2D>("small-font"), 6, 8, 6), Device, Content);

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
        }

        public override void Render(GraphicsDevice Device, Effect DiffuseEffect)
        {
            Text.SetString("Hello World!", 0, 0, Color.White, Color.TransparentBlack);
            Text.Draw(Device, new Viewport(8, 8, 132, 256));

            base.Render(Device, DiffuseEffect);
        }

        public override void Update(Play Game)
        {
            base.Update(Game);
            if (!MouseOverGui && ActiveTool != null)
                ActiveTool.Update(Game);
        }
    }
}
