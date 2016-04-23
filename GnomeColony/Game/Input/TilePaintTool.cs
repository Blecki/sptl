using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace Game.Input
{
    public class TilePaintTool : ITool
    {
        private TileTemplate Template;

        public TilePaintTool(TileTemplate Template)
        {
            this.Template = Template;
        }

        public void Update(Play Game, MainInputState InputState)
        {
            InputState.ShowEditableTileRange = true;

            if (Game.Input.Check("ESCAPE"))
            {
                InputState.ActiveTool = new DefaultInteractionTool();
                return;
            }

            if (Game.Input.Check("LEFTCLICK"))
            {
                var cell = Game.Map.GetCellAtWorld((int)Game.MouseWorldPosition.X, (int)Game.MouseWorldPosition.Y);
                cell.Tile = Template;
                var chunk = Game.Map.GetChunkForCellAtWorld((int)Game.MouseWorldPosition.X, (int)Game.MouseWorldPosition.Y);
                chunk.InvalidateMesh();
            }
        }

        public void Render(GraphicsDevice GraphicsDevice, Effect DiffuseEffect, Play Game)
        {
        }
    }
}
