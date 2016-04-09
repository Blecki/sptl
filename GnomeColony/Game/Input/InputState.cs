using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Gem;
using Gem.Render;

namespace Game
{
    public class InputState
    {
        public virtual void Entered(Play Game) { }
        public virtual void Covered(Play Game) { }
        public virtual void Update(Play Game) { }
        public virtual void Render(GraphicsDevice Device, Effect DiffuseEffect, Play Game) { }
        public virtual void Exposed(Play Game) { }
        public virtual void Left(Play Game) { }
    }
}