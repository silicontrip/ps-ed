using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace GNUed {

	public interface Command {
		Command parse (string Line);
		void init (string start, string end, string param);
		bool exit();  // yes or no ???
	//	List<Int32> getRange(string address);
	}

		public class CommandMode : Command {
		// A bunch of stuff

			private Regex commandMatch;
			private bool exitReady;
			// Regex singleAddress;
			private Regex rangeAddress;
			private Dictionary<string,Command> CommandList;

			public CommandMode() {
				commandMatch = new Regex(@"^(?<start>\,|\;|\.|\$|\d+|\+\d+|-\d+|\++|\-+|/[^,;]*/|\?[^,;]*\?|'[a-z])*(?<seperator>[,;])*(?<end>\.|\$|\d+|\+\d+|-\d+|\++|\-+|/[^,;]*/|\?[^,;]*\?|'[a-z])*(?<command>[acdeEfghHijklmnpPqQrstuvVwWxyz!#=])(?<parameter> .*)*$",RegexOptions.Compiled);

				// singleAddress = new Regex("\.|\$|\d+|\+\d+|-\d+|\++|\-+|/[^,;]*/|\?[^,;]*\?|'[a-z]");
				// rangeAddress = new Regex(@"^(?<start>\,|\;|\.|\$|\d+|\+\d+|-\d+|\++|\-+|/[^,;]*/|\?[^,;]*\?|'[a-z])*(?<seperator>[,;])*(?<end>\.|\$|\d+|\+\d+|-\d+|\++|\-+|/[^,;]*/|\?[^,;]*\?|'[a-z])*",RegexOptions.Compiled);
				exitReady = false;
				CommandList = new Dictionary<string,Command>();
			}

			public void init (string start, string end, string param)
			{
				;  // we don't need no initialisation.
				// we don't need no state control.
				// hey caller, leave these methods alone.
				// all in all you're just another, step in the stack.
			}

			public Command parse(string line)
			{
				MatchCollection commandParameters = commandMatch.Matches(line);
				//MatchCollection address = rangeAddress.Matches(line);
				// Report the number of matches found.

				if (commandParameters.Count != 1) {
					throw new Exception("invalid address");
				} else {
					Match command = commandParameters[0];
					GroupCollection gc = command.Groups;

					string cmd = gc["command"].Value;
					string cmdStart = gc["start"].Value;
					string cmdEnd = gc["end"].Value;
					string cmdParam = gc["parameter"].Value;

					Command current = CommandList[cmd];

				}
				Console.WriteLine("{0} matches found in:{1}", commandParameters.Count,line);
				foreach (Match match in commandParameters)
				{
					GroupCollection groups = match.Groups;
					Console.WriteLine("command: {0} param: {1}  range: {2}..{3}",groups["command"],groups["parameter"],groups["start"],groups["end"]);
					
				}

				return this;
			}

			public bool exit() { return exitReady; }
		}

		public class AppendMode : Command {
			private List<string> buffer;
			Int32 startLine;
			public AppendMode() { buffer = new List<string>(); }
			public void init (string start,string end, string param)
			{
				if (end.Length > 0 || param.Length > 0)
						throw new Exception("invalid address");
				if (start.Length > 0)
				{
					try {
						startLine = Int32.Parse(start);  // no it's not this simple
					} catch (FormatException e) {
						throw new Exception("invalid address");
					}
				} else {
					startLine = Controller.Instance.GetCurrentLine();
				}
			}
			public Command parse (string line) {
				if (line == ".") {
					// update document
					return new CommandMode();
				} else {
					buffer.Add(line);
					return this;
				}
			}
			public bool exit() { return false; }
		}

}