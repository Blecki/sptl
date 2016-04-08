using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Game
{ 
    public class Transition 
	{
        public Func<Actor, bool> Condition;
        public int NextState;
        public Action<Actor> OnChange;

		public Transition(Func<Actor, bool> Condition, int NextState, Action<Actor> OnChange)
		{
            this.Condition = Condition;
            this.NextState = NextState;
            this.OnChange = OnChange;
		}
		
	}

}