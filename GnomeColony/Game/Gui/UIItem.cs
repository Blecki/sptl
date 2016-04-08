using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Game.Gui
{
    public class UIItem
    {
        public List<UIItem> Children = new List<UIItem>();
        public Shape Shape;
        public Texture2D Texture;
        public Vector4 Color = Vector4.One;
        public Matrix UVTransform = Matrix.Identity;
        public Action<Game.Play> OnClick = null;

        public UIItem FindTopAtPoint(Vector2 Point)
        {
            foreach (var child in Children)
            {
                var top = child.FindTopAtPoint(Point);
                if (top != null) return top;
            }

            if (Shape != null && Shape.PointInside(Point)) return this;
            return null;
        }

        public void Render(GraphicsDevice Device, Effect DiffuseEffect)
        {
            DiffuseEffect.Parameters["DiffuseColor"].SetValue(Color);
            DiffuseEffect.Parameters["Texture"].SetValue(Texture);
            DiffuseEffect.Parameters["UVTransform"].SetValue(UVTransform);
            DiffuseEffect.CurrentTechnique.Passes[0].Apply();

            if (Shape != null) Shape.Render(Device, DiffuseEffect);

            foreach (var child in Children)
                child.Render(Device, DiffuseEffect);
        }
    }
}
