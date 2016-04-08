using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Game
{
    public class Actor
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public float Width;
        public float Height;
        public TileSheet TileSheet;

        public bool OnGround = false;
        public Gem.Input Input = null;
        public float Facing = 1.0f;

        public Mesh Mesh = null;

        private float StateTime = 0;
        public State CurrentState = null;
        private Dictionary<int, State> States = new Dictionary<int, State>();

        protected static Func<Actor, bool> _Input(String S) { return (p) => p.Input.Check(S); }
        protected static Func<Actor, bool> _And(Func<Actor, bool> A, Func<Actor, bool> B) { return (p) => A(p) && B(p); }
        protected static Func<Actor, bool> _Or(Func<Actor, bool> A, Func<Actor, bool> B) { return (p) => A(p) || B(p); }
        protected static Func<Actor, bool> _Not(Func<Actor, bool> A) { return (p) => !A(p); }
        protected static Func<Actor, bool> _Time(float X) { return (p) => p.StateTime >= X; }
        protected static Func<Actor, bool> _OnGround() { return (p) => p.OnGround; }
        protected static Func<Actor, bool> _YVelocityGreaterThan(float Y) { return (p) => p.Velocity.Y > Y; }

        protected static Action<Actor> _ApplyFriction(float F)
        {
            return (p) => p.ApplyFriction(F);
        }

        public void ApplyFriction(float F)
        {
            if (Velocity.X < 0)
            {
                Velocity.X += F;
                if (Velocity.X > 0) Velocity.X = 0;
            }
            else
            {
                Velocity.X -= F;
                if (Velocity.X < 0) Velocity.X = 0;
            }
        }

        protected static Action<Actor> _ApplyVelocity(Vector2 V)
        {
            return (p) =>
                {
                    p.Velocity += V;
                    if (p.Velocity.X > 0) p.Facing = 1.0f;
                    else p.Facing = -1.0f;
                };
        }

        protected static Action<Actor> _SetXVelocity(float X)
        {
            return (p) =>
                {
                    p.Velocity.X = X;
                    if (X > 0) p.Facing = 1.0f;
                    else p.Facing = -1.0f;
                };
        }

        protected void AddState(int Label, State State)
        {
            States.Upsert(Label, State);
        }

        protected State GetState(int Label)
        {
            return States[Label];
        }

        public void RenderShadow(GraphicsDevice Device, Effect ShadowEffect, Vector2 LightPosition)
        {

        }

        private void ChangeState(State NewState)
        {
            CurrentState = NewState;
            if (CurrentState != null && CurrentState._Enter != null) CurrentState._Enter(this);
            StateTime = 0.0f;
        }

        public void UpdateState(float ElapsedSeconds)
        {
            StateTime += ElapsedSeconds;
            if (CurrentState != null)
            {
                CurrentState.Update(this);
                var transition = CurrentState.Check(this);
                if (transition != null)
                {
                    if (transition.OnChange != null) transition.OnChange(this);
                    if (States.ContainsKey(transition.NextState))
                        ChangeState(States[transition.NextState]);
                }
            }
        }
    }
}
