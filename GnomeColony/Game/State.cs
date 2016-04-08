using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Game
{
    public class State 
	{
		public int Frame = 0;
        public List<Transition> Transitions;
        public Action<Actor> _Update;
        public Action<Actor> _Enter;

		public State(int Frame, Action<Actor> Update, params Transition[] Transitions)
		{
			this.Frame = Frame;
			this._Update = Update;
            this.Transitions = new List<Transition>(Transitions);
		}
		
		public void Update(Actor Actor)
		{
			if (_Update != null) _Update(Actor);
		}

        public Transition Check(Actor Actor)
        {
            foreach (var transition in Transitions)
                if (transition.Condition(Actor))
                    return transition;
            return null;
        }		
	}

}