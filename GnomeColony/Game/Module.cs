using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace Game
{
    public class Module
    {
        public virtual void Update(float ElapsedSeconds) { }
        public virtual void NewEntity(Actor Actor) { }

        public virtual void RenderDiffuse(GraphicsDevice Device, Effect Effect) { }
    }
}
