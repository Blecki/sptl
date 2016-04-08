using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Game
{
    public class PhysicsModule : Module
    {
        private List<Actor> Actors = new List<Actor>();
        private float PixelSize = 1.0f;
        private ChunkedMap<Cell, MapChunkRenderBuffer<Cell>> Map;

        public PhysicsModule(ChunkedMap<Cell, MapChunkRenderBuffer<Cell>> Map)
        {
            this.Map = Map;
        }

        public override void NewEntity(Actor Actor)
        {
            Actors.Add(Actor);
        }

        public override void Update(float ElapsedSeconds)
        {
            return;

            foreach (var actor in Actors)
            {
                actor.OnGround = !SpaceOpen(actor, new Vector2(actor.Position.X, actor.Position.Y + PixelSize));
                if (actor.OnGround)
                {
                    actor.ApplyFriction(0.2f);
                    if (actor.Velocity.Y > 0) actor.Velocity.Y = 0;
                }
                else
                {
                    actor.Velocity.Y += 32.0f * ElapsedSeconds;
                }

                MoveActor(actor, actor.Velocity * ElapsedSeconds);

                var x = (int)Math.Round(actor.Position.X / PixelSize);
                var y = (int)Math.Round(actor.Position.Y / PixelSize);
                actor.Position = new Vector2(x * PixelSize, y * PixelSize);
            }
        }


        private bool CollidesWithSolid(Actor A, float X, float Y)
        {
            return !SpaceOpen(A, new Vector2(X, Y));
        }

        private void WrapActorPosition(Actor A)
        {
            while (A.Position.X < 0) A.Position.X += 64.0f;
            while (A.Position.X >= 64.0f) A.Position.X -= 64.0f;
            while (A.Position.Y < 0) A.Position.Y += 64.0f;
            while (A.Position.Y >= 64.0f) A.Position.Y -= 64.0f;
        }

        private void MoveActor(Actor A, Vector2 Delta)
        {
            if (Gem.Math.Utility.AlmostZero(Delta.X) && Gem.Math.Utility.AlmostZero(Delta.Y))
                return;

            if (Math.Abs(Delta.X) > Math.Abs(Delta.Y)) // Step along X.
            {
                var direction = Delta.X > 0 ? PixelSize : -PixelSize;
                var startX = A.Position.X + direction;
                var endX = A.Position.X + Delta.X + direction;
                var stepY = Delta.Y / (Delta.X / PixelSize) * Math.Sign(direction);
                var startY = A.Position.Y;

                for (var counter = 0; counter < Math.Abs((endX - startX) / PixelSize); counter += 1)
                {
                    startY += stepY;

                    // First, try some slope hugging.
                    if (Delta.X < 0.5f // Only if the actor is going slow enough.
                        && CollidesWithSolid(A, A.Position.X, A.Position.Y + PixelSize) // And the actor is currently on the ground...
                        && !CollidesWithSolid(A, A.Position.X + direction, A.Position.Y + PixelSize) // And the actor can move into the space...
                    )
                    {
                        A.Position.X += direction;
                        A.Position.Y += PixelSize;
                        startY += PixelSize;
                    }                       

                    // Try to make the full motion first.
                    else if (!CollidesWithSolid(A, A.Position.X + direction, startY))
                    {
                        A.Position.X += direction;
                        A.Position.Y = startY;
                    }

                    // Can't make full motion, try moving just on the X axis.
                    else if (!CollidesWithSolid(A, A.Position.X + direction, A.Position.Y))
                    {
                        A.Position.X += direction;
                        startY = A.Position.Y;
                        stepY = -(stepY * /*A.Bounc*/ 0.0f);
                        A.Velocity.Y = -(A.Velocity.Y * /*A.Bounce*/ 0.0f);
                        //OnHit...
                    }

                    // Can't move on X axis... try moving UP and on the X axis (Climbs slopes)
                    else if (!CollidesWithSolid(A, A.Position.X + direction, startY - PixelSize))
                    {
                        A.Position.X += direction;
                        A.Position.Y = startY - PixelSize;                        
                    }

                    // Can't move on X axis, try moving on Y axis.
                    else if (!CollidesWithSolid(A, A.Position.X, startY))
                    {
                        A.Position.Y = startY;
                        //if bounce != 0 direction = -direction;
                        //else direction = 0;
                        direction = 0;
                        A.Velocity.X = -(A.Velocity.X * /*A.Bounce*/ 0.0f);
                        //OnHit...
                    }

                    // Couldn't move on either axis.
                    else
                    {
                        //OnHit...
                        break;
                    }

                    WrapActorPosition(A);
                }
            }
            else //step along y
            {
                var direction = Delta.Y > 0 ? PixelSize : -PixelSize;
                var startY = A.Position.Y + direction;
                var endY = A.Position.Y + Delta.Y + direction;
                var stepX = Delta.X / (Delta.Y / PixelSize) * Math.Sign(direction);
                var startX = A.Position.X;

                for (var counter = 0; counter < Math.Abs((endY - startY) / PixelSize); counter += 1)
                {
                    startX += stepX;

                    // Try to make the full motion first.
                    if (!CollidesWithSolid(A, startX, A.Position.Y + direction))
                    {
                        A.Position.Y += direction;
                        A.Position.X = startX;
                    }

                    // Can't make full motion, try moving just on the X axis.
                    else if (!CollidesWithSolid(A, startX, A.Position.Y))
                    {
                        A.Position.X = startX;
                        //if bounce != 0 direction = -direction;
                        //else direction = 0;
                        direction = 0;
                        A.Velocity.Y = -(A.Velocity.Y * /*A.Bounce*/ 0.0f);
                        //OnHit...

                    }

                    // Can't move on X axis, try moving on Y axis.
                    else if (!CollidesWithSolid(A, A.Position.X, A.Position.Y + direction))
                    {
                        A.Position.Y += direction;
                        startX = A.Position.X;
                        stepX = -(stepX * /*A.Bounc*/ 0.0f);
                        A.Velocity.X = -(A.Velocity.X * /*A.Bounce*/ 0.0f);
                        //OnHit...
                    }

                    // Couldn't move on either axis.
                    else
                    {
                        //OnHit...
                        break;
                    }

                    WrapActorPosition(A);
                }
            }
        }

        private bool SpaceOpen(Actor A, Vector2 NewPosition)
        {
            var playerBox = new Gem.AABB(NewPosition.X - (A.Width / 2), NewPosition.Y - (A.Height / 2), A.Width, A.Height);
            bool foundSolid = false;
            Map.ForEachCellInWorldRect(NewPosition.X - (A.Width / 2), NewPosition.Y - (A.Height / 2), A.Width, A.Height,
                (cell, x, y) =>
                {
                    if (cell.Tile == null || !cell.Tile.Solid) return;
                    var cellPoints = cell.Tile.CollisionPoints.Select(p => new Vector2(p.X + x, p.Y + y));
                    if (Intersection.PolygonWithAABB(cellPoints.ToArray(), playerBox))
                        foundSolid = true;
                });
            return !foundSolid;
        }


    }
}
