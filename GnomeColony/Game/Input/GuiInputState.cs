using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Game
{
    public class GuiInputState : InputState
    {
        public Gui.UIItem GuiRoot;
        public Gem.Render.OrthographicCamera Camera;
        public bool MouseOverGui = false;

        protected Texture2D Blank;

        public GuiInputState(GraphicsDevice Device)
        {
            Blank = new Texture2D(Device, 1, 1, false, SurfaceFormat.Color);
            Blank.SetData(new Color[] { new Color(255, 255, 255, 255) });

            Camera = new Gem.Render.OrthographicCamera(Device.Viewport);

            GuiRoot = new Gui.UIItem();
        }

        public override void Update(Play Game)
        {
            var topUnderMouse = GuiRoot.FindTopAtPoint(Game.MousePosition);
            MouseOverGui = topUnderMouse != null;

            if (topUnderMouse != null && topUnderMouse.OnClick != null && Game.Input.Check("LEFTCLICK"))
                topUnderMouse.OnClick(Game);
        }

        public override void Render(GraphicsDevice Device, Effect DiffuseEffect, Play Game)
        {
            DiffuseEffect.Parameters["World"].SetValue(Matrix.Identity);
            DiffuseEffect.Parameters["View"].SetValue(Camera.View);
            DiffuseEffect.Parameters["Projection"].SetValue(Camera.Projection);
            DiffuseEffect.Parameters["Texture"].SetValue(Blank);
            DiffuseEffect.Parameters["DiffuseColor"].SetValue(Vector4.One);
            DiffuseEffect.Parameters["Alpha"].SetValue(1.0f);
            DiffuseEffect.Parameters["ClipAlpha"].SetValue(0.2f);
            DiffuseEffect.Parameters["UVTransform"].SetValue(Matrix.Identity);
            GuiRoot.Render(Device, DiffuseEffect);
        }
    }
}
