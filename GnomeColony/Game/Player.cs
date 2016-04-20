using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Game
{ 
    public class Player : Actor
	{
        public enum StateNames
        {
            STAND,

            WALK_0,
            WALK_1,
            WALK_2,
            WALK_3,
            WALK_4,
            WALK_5,
            WALK_6,
            WALK_7,

            JUMP,
            FLOAT,
            FALL,
            LAND,
        }

        private static Action<Actor> _JumpAirControl()
        {
            return (p) =>
            {
                if (p.Input.Check("LEFT")) p.Velocity.X -= 2.0f;
                if (p.Input.Check("RIGHT")) p.Velocity.X += 2.0f;
            };
        }

        private static void _applyRunForce(Actor A)
        {
            A.Velocity.X += (A.Facing * 16.0f);
        }

        private static Func<Actor, bool> _Facing(float X)
        {
            return (p) =>
            {
                if (p.Facing > 0 && X > 0) return true;
                else if (p.Facing < 0 && X < 0) return true;
                return false;
            };
        }

        public Player()
        {
            AddState((int)StateNames.STAND, new Game.State(41, _ApplyFriction(128.0f),
                new Transition(_Not(_OnGround()), (int)StateNames.FLOAT, null),
                new Transition(_Input("LEFT"), (int)StateNames.WALK_0, _SetXVelocity(-16.0f)),
                new Transition(_Input("RIGHT"), (int)StateNames.WALK_0, _SetXVelocity(16.0f)),
                new Transition(_Input("JUMP"), (int)StateNames.JUMP, null)));

            AddState((int)StateNames.WALK_0, new State(22, _applyRunForce,
                new Transition(_And(_Facing(-1.0f), _And(_Input("LEFT"), _Time(0.25f))), (int)StateNames.WALK_1, null),
                new Transition(_And(_Facing(-1.0f), _Not(_Input("LEFT"))), (int)StateNames.STAND, null),

                new Transition(_And(_Facing(1.0f), _And(_Input("RIGHT"), _Time(0.25f))), (int)StateNames.WALK_1, null),
                new Transition(_And(_Facing(1.0f), _Not(_Input("RIGHT"))), (int)StateNames.STAND, null),
                
                new Transition(_Input("JUMP"), (int)StateNames.JUMP, null),
                new Transition(_Not(_OnGround()), (int)StateNames.FLOAT, null)));

            AddState((int)StateNames.WALK_1, new State(21, _applyRunForce,
                new Transition(_And(_Facing(-1.0f), _And(_Input("LEFT"), _Time(0.25f))), (int)StateNames.WALK_2, null),
                new Transition(_And(_Facing(-1.0f), _Not(_Input("LEFT"))), (int)StateNames.STAND, null),
                new Transition(_And(_Facing(1.0f), _And(_Input("RIGHT"), _Time(0.25f))), (int)StateNames.WALK_2, null),
                new Transition(_And(_Facing(1.0f), _Not(_Input("RIGHT"))), (int)StateNames.STAND, null),
                new Transition(_Input("JUMP"), (int)StateNames.JUMP, null),
                new Transition(_Not(_OnGround()), (int)StateNames.FLOAT, null)));

            AddState((int)StateNames.WALK_2, new State(20, _applyRunForce,
                new Transition(_And(_Facing(-1.0f), _And(_Input("LEFT"), _Time(0.25f))), (int)StateNames.WALK_3, null),
                new Transition(_And(_Facing(-1.0f), _Not(_Input("LEFT"))), (int)StateNames.STAND, null),
                new Transition(_And(_Facing(1.0f), _And(_Input("RIGHT"), _Time(0.25f))), (int)StateNames.WALK_3, null),
                new Transition(_And(_Facing(1.0f), _Not(_Input("RIGHT"))), (int)StateNames.STAND, null),
                new Transition(_Input("JUMP"), (int)StateNames.JUMP, null),
                new Transition(_Not(_OnGround()), (int)StateNames.FLOAT, null)));

            AddState((int)StateNames.WALK_3, new State(19, _applyRunForce,
                new Transition(_And(_Facing(-1.0f), _And(_Input("LEFT"), _Time(0.25f))), (int)StateNames.WALK_4, null),
                new Transition(_And(_Facing(-1.0f), _Not(_Input("LEFT"))), (int)StateNames.STAND, null),
                new Transition(_And(_Facing(1.0f), _And(_Input("RIGHT"), _Time(0.25f))), (int)StateNames.WALK_4, null),
                new Transition(_And(_Facing(1.0f), _Not(_Input("RIGHT"))), (int)StateNames.STAND, null),
                new Transition(_Input("JUMP"), (int)StateNames.JUMP, null),
                new Transition(_Not(_OnGround()), (int)StateNames.FLOAT, null)));

            AddState((int)StateNames.WALK_4, new State(18, _applyRunForce,
                new Transition(_And(_Facing(-1.0f), _And(_Input("LEFT"), _Time(0.25f))), (int)StateNames.WALK_5, null),
                new Transition(_And(_Facing(-1.0f), _Not(_Input("LEFT"))), (int)StateNames.STAND, null),
                new Transition(_And(_Facing(1.0f), _And(_Input("RIGHT"), _Time(0.25f))), (int)StateNames.WALK_5, null),
                new Transition(_And(_Facing(1.0f), _Not(_Input("RIGHT"))), (int)StateNames.STAND, null),
                new Transition(_Input("JUMP"), (int)StateNames.JUMP, null),
                new Transition(_Not(_OnGround()), (int)StateNames.FLOAT, null)));

            AddState((int)StateNames.WALK_5, new State(17, _applyRunForce,
                new Transition(_And(_Facing(-1.0f), _And(_Input("LEFT"), _Time(0.25f))), (int)StateNames.WALK_6, null),
                new Transition(_And(_Facing(-1.0f), _Not(_Input("LEFT"))), (int)StateNames.STAND, null),
                new Transition(_And(_Facing(1.0f), _And(_Input("RIGHT"), _Time(0.25f))), (int)StateNames.WALK_6, null),
                new Transition(_And(_Facing(1.0f), _Not(_Input("RIGHT"))), (int)StateNames.STAND, null),
                new Transition(_Input("JUMP"), (int)StateNames.JUMP, null),
                new Transition(_Not(_OnGround()), (int)StateNames.FLOAT, null)));

            AddState((int)StateNames.WALK_6, new State(16, _applyRunForce,
                new Transition(_And(_Facing(-1.0f), _And(_Input("LEFT"), _Time(0.25f))), (int)StateNames.WALK_7, null),
                new Transition(_And(_Facing(-1.0f), _Not(_Input("LEFT"))), (int)StateNames.STAND, null),
                new Transition(_And(_Facing(1.0f), _And(_Input("RIGHT"), _Time(0.25f))), (int)StateNames.WALK_7, null),
                new Transition(_And(_Facing(1.0f), _Not(_Input("RIGHT"))), (int)StateNames.STAND, null),
                new Transition(_Input("JUMP"), (int)StateNames.JUMP, null),
                new Transition(_Not(_OnGround()), (int)StateNames.FLOAT, null)));

            AddState((int)StateNames.WALK_7, new State(23, _applyRunForce,
                new Transition(_And(_Facing(-1.0f), _And(_Input("LEFT"), _Time(0.25f))), (int)StateNames.WALK_0, null),
                new Transition(_And(_Facing(-1.0f), _Not(_Input("LEFT"))), (int)StateNames.STAND, null),
                new Transition(_And(_Facing(1.0f), _And(_Input("RIGHT"), _Time(0.25f))), (int)StateNames.WALK_0, null),
                new Transition(_And(_Facing(1.0f), _Not(_Input("RIGHT"))), (int)StateNames.STAND, null),
                new Transition(_Input("JUMP"), (int)StateNames.JUMP, null),
                new Transition(_Not(_OnGround()), (int)StateNames.FLOAT, null)));

            AddState((int)StateNames.JUMP, new State(39, _JumpAirControl(),
                new Transition(_Or(_OnGround(), _YVelocityGreaterThan(0)), (int)StateNames.FLOAT, null)));
            GetState((int)StateNames.JUMP)._Enter = (p) => p.Velocity.Y = -320.0f;
            AddState((int)StateNames.FLOAT, new State(38, _JumpAirControl(),
                new Transition(_YVelocityGreaterThan(0.1f), (int)StateNames.FALL, null),
                new Transition(_OnGround(), (int)StateNames.STAND, null)));
            AddState((int)StateNames.FALL, new State(37, _JumpAirControl(),
                new Transition(_OnGround(), (int)StateNames.LAND, null)));
            AddState((int)StateNames.LAND, new State(36, null,
                new Transition(_Time(0.25f), (int)StateNames.STAND, null)));

            CurrentState = GetState((int)StateNames.STAND);

        }
      
    }

}