using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Game
{
    public class EntityStateModule : Module
    {
        private List<Actor> Actors = new List<Actor>();

        public override void NewEntity(Actor Actor)
        {
            Actors.Add(Actor);
        }

        public override void Update(float ElapsedSeconds)
        {
            foreach (var actor in Actors)
                actor.UpdateState(ElapsedSeconds);
        }

        public override void RenderDiffuse(GraphicsDevice Device, Effect Effect)
        {
            foreach (var actor in Actors)
            {

                if (actor.Mesh == null)
                {
                    actor.Mesh = Mesh.CreateSpriteQuad();
                    Mesh.Transform(actor.Mesh, Matrix.CreateTranslation(-0.5f, -0.5f, 0));
                    Mesh.Transform(actor.Mesh, Matrix.CreateScale(actor.Width, actor.Height, 1.0f));
                    //Mesh.Transform(actor.Mesh, Matrix.CreateRotationZ((float)System.Math.PI));
                }

                Effect.Parameters["World"].SetValue(Matrix.CreateTranslation(actor.Position.X, actor.Position.Y, 0.0f));

                if (actor.CurrentState != null)
                    Effect.Parameters["UVTransform"].SetValue(Matrix.CreateScale(-actor.Facing, 1.0f, 1.0f) * Matrix.CreateTranslation(actor.Facing > 0 ? 1.0f : 0.0f, 0.0f, 0.0f) * actor.TileSheet.TileMatrix(actor.CurrentState.Frame));
                else
                    Effect.Parameters["UVTransform"].SetValue(Matrix.CreateScale(-actor.Facing, 1.0f, 1.0f));

                Effect.Parameters["Texture"].SetValue(actor.TileSheet.Texture);
                Effect.CurrentTechnique.Passes[0].Apply();

                actor.Mesh.Render(Device);
            }
        }
    }
}
