using System;
using System.Collections.Generic;
using System.Management.Automation.Host;
// using System.Runtime;
using System.Text.RegularExpressions;
// using Microsoft.PowerShell;


//using Document;

namespace org.gnu.ed {

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
		// private static Controller instance=null;
		private PSHostUserInterface ui;
	//	private Regex commandMatch;
		private bool exitControl;
		private Dictionary<string,Command> commandList;
		Regex parseRegex ;

		public Controller(PSHostUserInterface u, Document d)
		{
			exitControl = false;
			cutBuffer = new List<string>();
			markBuffer = new Dictionary<string,Int32>();
			lastReplaceRegex = "";
			lastSearchRegex = "";
			lastError="";
			verboseErrorMode = false;
			//	mode = command;
			prompt = "";
			ui = u;
			buffer = d;

			//Regex r = new Regex(@"^(?<start>\,|\;|\.|\$|\d+|\+\d+|-\d+|\++|\-+|/[^,;]*/|\?[^,;]*\?|'[a-z])*(?<seperator>[,;])*(?<end>\.|\$|\d+|\+\d+|-\d+|\++|\-+|/[^,;]*/|\?[^,;]*\?|'[a-z])*(?<command>[acdeEfghHijklmnpPqQrstuvVwWxyz!#=])(?<parameter> .*)*$",RegexOptions.Compiled);
			 parseRegex = new Regex(@"^(?<range>(\,|\;|\.|\$|\d+|\+\d+|-\d+|\++|\-+|/[^,;]*/|\?[^,;]*\?|'[a-z])*([,;])*(\.|\$|\d+|\+\d+|-\d+|\++|\-+|/[^,;]*/|\?[^,;]*\?|'[a-z])*)(?<command>[acdeEfghHijklmnpPqQrstuvVwWxyz!#=])(?<parameter> .*)*$",RegexOptions.Compiled);

			currentMode = new CommandMode(this, buffer);

			commandList = new Dictionary<string,Command>();
			commandList["command"] = currentMode;
			commandList["a"] = new CommandAppend(this,buffer);
			commandList["c"] = new CommandChange(this,buffer);
			commandList["d"] = new CommandDelete(this,buffer);
			commandList["e"] = new CommandEdit(this,buffer);

			commandList["q"] = new CommandQuit(this,buffer);
			  // needs document, controller object
		}

		public Command GetCommand(string s)
		{
			return commandList[s];
		}

		public void SetUI (PSHostUserInterface u) { ui = u; }

		public void SetDocument (Document d)
		{
			buffer = d;
			currentLine = buffer.GetLineLength();
		}

		public Document GetDocument() { return buffer; }
		public void SetPrompt (string p) { prompt = p; }
		public void SetCurrentLine(Int32 l) { currentLine = l; }
		public Int32 GetCurrentLine() { return currentLine; }
		public void SetExit (bool e) { exitControl = e; }

		public GroupCollection ParseCommand (string line)
		{
			MatchCollection commandParameters = parseRegex.Matches(line);
			//MatchCollection address = rangeAddress.Matches(line);
			// Report the number of matches found.

			if (commandParameters.Count == 1) {
				Match command = commandParameters[0];
				return command.Groups;
			}
			throw new Exception("invalid address");

		}

		public Int32[] ParseRange (string addr, string addrdef, Int32 addrlim)
		{
			Int32[] response = ParseRange(addr,addrdef);
			if (response.Length == addrlim)
				return response;

			throw new Exception("invalid address");

		}

		public Int32[] ParseRange (string addr, string addrdef)
		{
			if (String.IsNullOrEmpty(addr))
				return ParseRange(addrdef);
			else
				return ParseRange(addr);
		}

		public Int32[] ParseRange (string addrRange)
		{

			// Int32[] intRange = new Int32[2];
			
			if (addrRange == ",")
				return new int[] {1,buffer.GetLineLength()};
			if (addrRange == ";")
				return new int[] {currentLine,buffer.GetLineLength()};

			string[] rangeSplit = addrRange.Split(',');

			if (rangeSplit.Length == 1)
				return new int[] { parseAddress(rangeSplit[0]) };
			if (rangeSplit.Length == 2)
				return new int[] { parseAddress(rangeSplit[0]), parseAddress(rangeSplit[1]) };

			throw new Exception("invalid address");

		}

		private Int32 parseAddress (string addr)
		{
			if (addr == ".")
				return currentLine;
			if (addr == "$")
				return buffer.GetLineLength();
			
			// begins with + or -

			if (addr.StartsWith("+"))
			{
				string nn = addr.Substring(1);

			}

			if (addr.StartsWith("-"))
			{
				string nn = addr.Substring(1);

			}

			// is int

			// next regex

			// prev regex

			// mark
			if (addr.StartsWith("'"))
			{
				string mm = addr.Substring(1);
				try {
					return markBuffer[mm];
				} catch (KeyNotFoundException) {
					throw new Exception("invalid address");
				}
			}

			throw new Exception("invalid address");

		}

		public void Start()
		{
			while (!exitControl)
			{
				// prompt
				Console.Write(prompt);
				//string result = Host.UI.ReadLine();

				string result = ui.ReadLine();
				try {
					Console.WriteLine("Read: " + result);
					currentMode = currentMode.parse(result);  // circular dependancy
				} 
				catch (Exception e) 
				{
					lastError = e.Message;
					if (verboseErrorMode) {
						Console.WriteLine(lastError);
					} else {
						Console.WriteLine("?");
					}
				}
			}
		}

	}
}