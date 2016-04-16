using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Game
{
    public class TileSelectInputState : GuiInputState
    {
        public Action<TileTemplate> OnSelection = null;

        public TileSelectInputState(GraphicsDevice Device, Play Game) : base(Device)
        {
            var frame = new Gui.UIItem();
            frame.Shape = Gui.Shape.CreateQuad(16, 600 - 128 - 16, 800 - 32, 128);
            frame.Texture = Blank;
            frame.Color = new Vector4(1, 0, 0, 1);

            GuiRoot.Children.Add(frame);

            for (int x = 0; x < 7 && x < Game.TileTemplates.Length; ++x)
            {
                var lTemplate = Game.TileTemplates[x];
                var icon = new Gui.UIItem();
                icon.Shape = Gui.Shape.CreateQuad(16 + 8 + (40 * x), 600 - 128, 32, 32);
                icon.Texture = Game.TileSet.Texture;
                icon.Color = new Vector4(0, 1, 0, 1);
                icon.UVTransform = Game.TileSet.TileMatrix(lTemplate.TileIndex);
                icon.OnClick += p =>
                {
                    if (OnSelection != null) OnSelection(lTemplate);
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
