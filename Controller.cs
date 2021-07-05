using System;
using System.Collections.Generic;
using System.Management.Automation.Host;
// using System.Runtime;
using System.Text.RegularExpressions;
// using Microsoft.PowerShell;


//using Document;

namespace GNUed {

	public interface Command {
		Command parse (string Line);
		bool exit();
	//	List<Int32> getRange(string address);
	}

	public class Controller {

		private Document buffer;
		private Command currentMode;
		private Int32 currentLine;
		private List<string> cutBuffer;
		private Dictionary<string,Int32> markBuffer;
		string lastReplaceRegex;
		string lastSearchRegex;
		string lastError;
		bool verboseErrorMode;
		// InputMode mode;
		string prompt;
		List<string> undoCommands; // tricky??
		private static Controller instance=null;
		private PSHostUserInterface ui;

		private Controller()
		{
			cutBuffer = new List<string>();
			markBuffer = new Dictionary<string,Int32>();
			lastReplaceRegex = "";
			lastSearchRegex = "";
			lastError="";
			verboseErrorMode = false;
		//	mode = command;
			prompt = "";

		// ui = Host.UI;

			currentMode = new CommandMode();
		}

		public static Controller Instance
		{
			get
			{
				if (instance==null)
				{
					instance = new Controller();
				}
				return instance;
			}
		}

		public void SetUI (PSHostUserInterface u) { ui = u;  }

		public void SetDocument (Document d)
		{
			buffer = d;
			currentLine = buffer.GetLineLength();
		}

		public Document GetDocument() { return buffer; }

		public void SetPrompt (string p) { prompt = p; }

		public void SetCurrentLine(Int32 l) { currentLine = l; }
		public Int32 GetCurrentLine() { return currentLine; }

		// public string

		public void Start()
		{
			while (!currentMode.exit())
			{
				// prompt
				Console.Write(prompt);
				//string result = Host.UI.ReadLine();

				string result = ui.ReadLine();
				try {
					currentMode = currentMode.parse(result);  // circular dependancy
				} catch (Exception e) {
					lastError = e.Message;
					if (verboseErrorMode) {
						Console.WriteLine(lastError);
					}

				}
			}
		}

		public class CommandMode : Command {
		// A bunch of stuff

			Regex commandMatch;
			bool exitReady;
			// Regex singleAddress;
			Regex rangeAddress;

			public CommandMode() {
				commandMatch = new Regex(@"^[^']*(?<command>[acdeEfghHijklmnpPqQrstuvVwWxyz!#=])(?<parameter>.*)$",RegexOptions.Compiled);
				// singleAddress = new Regex("\.|\$|\d+|\+\d+|-\d+|\++|\-+|/[^,;]*/|\?[^,;]*\?|'[a-z]");
				rangeAddress = new Regex(@"(?<start>[^,;]*|\.|\$|\d+|\+\d+|-\d+|\++|\-+|/[^,;]*/|\?[^,;]*\?|'[a-z])(?<seperator>[,;]*)(?<end>[^,;]*|\.|\$|\d+|\+\d+|-\d+|\++|\-+|/[^,;]*/|\?[^,;]*\?|'[a-z])",RegexOptions.Compiled);
				exitReady = false;
			}

			public Command parse (string line)
			{
				MatchCollection matches = commandMatch.Matches(line);

				// Report the number of matches found.
				Console.WriteLine("{0} matches found in:\n   {1}",
					matches.Count,
					line);
				foreach (Match match in matches)
				{
					GroupCollection groups = match.Groups;
					Console.WriteLine("'{0}' repeated at positions {1} and {2}",
						groups["word"].Value,
						groups[0].Index,
						groups[1].Index);
				}
				return this;
			}

			public bool exit() { return exitReady; }
		}

		public class InputMode : Command {
			private List<string> buffer;
			public InputMode() { buffer = new List<string>(); }
			public Command parse (string line) {
				if (line == ".") {
					
					return new CommandMode();
				} else {
					buffer.Add(line);
					return this;
				}
			}
			public bool exit() { return false; }
		}
	}
}