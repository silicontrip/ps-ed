using System;

namespace GNUed {

	public interface Command {
		Command parse (string Line);
	}

		public class CommandMode : Command {
			// A bunch of stuff
			
		}

}