using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace Game.Input
{
    public class DefaultInteractionTool : ITool
    {
        public void Update(Play Game, MainInputState InputState)
        {
            if (Game.Input.Check("LEFTCLICK"))
            {
                var cell = Game.WireMap.GetCellAtWorld((int)Game.MouseWorldPosition.X, (int)Game.MouseWorldPosition.Y);
                if (cell.Cell != null && cell.Device != null && cell.Device.OnClick != null)
                    cell.Device.OnClick(cell, Game);
            }
        }

        public void Render(GraphicsDevice GraphicsDevice, Effect DiffuseEffect, Play Game)
        {
        }
    }
}
