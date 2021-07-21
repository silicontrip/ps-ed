using System;
using System.Collections.Generic;
using System.Management.Automation;
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
		// private string fileName;
		string lastReplaceRegex;
		string lastSearchRegex;
		string lastError;
		bool verboseErrorMode;
		bool unsaved;
 
		// string filename;

 		string prompt;
		string fixedPrompt;

		List<string> undoCommands; // tricky??
		
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
			unsaved = false;

			fixedPrompt = "*";

			currentLine = d.GetLineLength();

			//ui.WriteLine(String.Format("{0}",d.GetCharacterLength()));

			//Regex r = new Regex(@"^(?<start>\,|\;|\.|\$|\d+|\+\d+|-\d+|\++|\-+|/[^,;]*/|\?[^,;]*\?|'[a-z])*(?<seperator>[,;])*(?<end>\.|\$|\d+|\+\d+|-\d+|\++|\-+|/[^,;]*/|\?[^,;]*\?|'[a-z])*(?<command>[acdeEfghHijklmnpPqQrstuvVwWxyz!#=])(?<parameter> .*)*$",RegexOptions.Compiled);
			parseRegex = new Regex(@"^(?<range>(\,|\;|\.|\$|\d+|\+\d+|-\d+|\++|\-+|/[^,;]*/|\?[^,;]*\?|'[a-z])*([,;])*(\.|\$|\d+|\+\d+|-\d+|\++|\-+|/[^,;]*/|\?[^,;]*\?|'[a-z])*)(?<command>[acdeEfgGhHijklmnpPqQrstuvVwWxyz!#=]|wq|$)(?<parameter>[ /0-9].*)*$",RegexOptions.Compiled);

			currentMode = new CommandMode(this, buffer,ui);

			commandList = new Dictionary<string,Command>();
			commandList["command"] = currentMode;
			commandList["a"] = new CommandAppend(this,buffer,ui);
			commandList["c"] = new CommandChange(this,buffer,ui);
			commandList["d"] = new CommandDelete(this,buffer,ui);
			commandList["e"] = new CommandEdit(this,buffer,ui);
			commandList["E"] = new CommandEditUnconditional(this,buffer,ui);

			commandList["g"] = new CommandGlobal(this,buffer,ui);
			commandList["G"] = new CommandGlobalInteractive(this,buffer,ui);

			commandList["h"] = new CommandHelp(this,buffer,ui);
			commandList["H"] = new CommandHelpToggle(this,buffer,ui);
			commandList["i"] = new CommandInsert(this,buffer,ui);
			commandList["j"] = new CommandJoin(this,buffer,ui);
			commandList["k"] = new CommandMark(this,buffer,ui);
			commandList["l"] = new CommandList(this,buffer,ui);
			commandList["m"] = new CommandMove(this,buffer,ui);
			commandList["n"] = new CommandNumber(this,buffer,ui);
			commandList["p"] = new CommandPrint(this,buffer,ui);
			commandList["P"] = new CommandPrompt(this,buffer,ui);
			commandList["q"] = new CommandQuit(this,buffer,ui);
			commandList["Q"] = new CommandQuitForce(this,buffer,ui);
			commandList["r"] = new CommandRead(this,buffer,ui);

			commandList["s"] = new CommandSubstituteRedirect(this,buffer,ui);

			commandList["t"] = new CommandTransfer(this,buffer,ui);
			commandList["u"] = null;
			commandList["v"] = new CommandInverseGlobal(this,buffer,ui);
			commandList["V"] = new CommandInverseGlobalInteractive(this,buffer,ui);

			commandList["w"] = new CommandWrite(this,buffer,ui);
			commandList["wq"] = new CommandWriteQuit(this,buffer,ui);  // Always an exception

			commandList["x"] = new CommandPaste(this,buffer,ui);
			commandList["y"] = new CommandYank(this,buffer,ui);

			commandList["#"] = new CommandComment(this,buffer,ui);
			commandList["="] = new CommandLine(this,buffer,ui);
			commandList[""] = new CommandNull(this,buffer,ui);

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

		public void SetCurrentLine(Int32 l) { currentLine = l; }
		public void SetCutBuffer(List<string> cb) {cutBuffer = cb; }
		public List<string> GetCutBuffer() { return cutBuffer; }

		public void SetExit (bool e) { exitControl = e; }
		public void SetMark (string s, Int32 i) { markBuffer[s] = i; }
		public void SetPrompt (string p) { prompt = p; }
		public void SetVerboseHelp (bool h) { verboseErrorMode = h; }

		public bool isUnsaved() { return unsaved; }
		public void Saved() { unsaved = false; }
		public void Unsaved() { unsaved = true; }
	//	public void SetFilename(string g) { filename = g; }
//		public string GetFilename() { return filename; }

		public Int32 GetCurrentLine() { return currentLine; }
		public Document GetDocument() { return buffer; }
		public string GetError() { return lastError; }
		public bool GetVerboseHelp () { return verboseErrorMode; }

		public void ToggleVerboseHelp() { verboseErrorMode = !verboseErrorMode; }
		public void TogglePrompt() { 
			if (prompt.Length == 0)
				prompt = fixedPrompt;
			else
				prompt = "";
		}

		public  void NoParam(string param) { if (!String.IsNullOrEmpty(param)) throw new Exception("invalid command suffix"); }
		public  void HasParam(string param) { if (String.IsNullOrEmpty(param)) throw new Exception("invalid command suffix"); }

		public  void NoAddress(string param) { if (!String.IsNullOrEmpty(param)) throw new Exception("invalid address"); }
		public  void OrderAddress(Int32[] a) { if (a[1]<a[0]) throw new Exception("invalid address"); }
		public bool SemiColonAddress(string param) {
			string[] rangeSplit = param.Split(';');
			return (rangeSplit.Length > 1);
		}

		public  IEnumerable<string> EscapedSplit(string input, string separator, char escapeCharacter)
		{
			int startOfSegment = 0;
			int index = 0;

			while (index < input.Length)
			{
				index = input.IndexOf(separator,index);
				if (index > 0 && input[index-1] == escapeCharacter)
				{
					index += separator.Length;
					continue;
				}
				if (index == -1)
				{ 
					break;
				}
				yield return input.Substring(startOfSegment,index-startOfSegment);
				index += separator.Length;
				startOfSegment = index;
			}
			yield return input.Substring (startOfSegment);
		}

		public  GroupCollection ParseCommand (string line)
		{
			MatchCollection commandParameters = parseRegex.Matches(line);
			// MatchCollection address = rangeAddress.Matches(line);
			// Report the number of matches found.

			if (commandParameters.Count == 1) {
				Match command = commandParameters[0];
				return command.Groups;
			}
			throw new Exception("invalid address");
		}

		public Int32[] ParseRangeDuplicate (string addr,string addrdef)
		{
			Int32[] response = ParseRange(addr,addrdef);
			if (response.Length == 1)
				response = new Int32[2] {response[0],response[0]};

			return response;
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
			// Console.WriteLine("parse range (addr,addref)");


			if (String.IsNullOrEmpty(addr))
				return ParseRange(addrdef);
			else
				return ParseRange(addr);
		}

		public Int32[] ParseRange (string addrRange)
		{

		//	Console.WriteLine("parse range (string)");
			// Int32[] intRange = new Int32[2];
			
			if (addrRange == ",")
				return new int[] {1,buffer.GetLineLength()};

			if (addrRange == ";")
				return new int[] {currentLine,buffer.GetLineLength()};

			string[] rangeSplit = addrRange.Split(',');

			//Console.WriteLine("parse range: {0}",rangeSplit);

			if (rangeSplit.Length == 1)
				return new Int32[] { ParseAddress(rangeSplit[0]) };
			if (rangeSplit.Length == 2)
				return new Int32[] { ParseAddress(rangeSplit[0]), ParseAddress(rangeSplit[1]) };

			throw new Exception("invalid address");

		}

		public Int32 ParseAddress (string addr)
		{

			if (addr == ".") {
				return currentLine;
			}
			if (addr == "$")
				return buffer.GetLineLength();
			
			// begins with + or -

			if (addr.StartsWith("+"))
			{
				string nn = addr.Substring(1);
				Regex regex = new Regex("\\++"); // Split on hyphens.
				string[] substrings = regex.Split(addr);
				if (substrings.Length != 2)
					throw new Exception("invalid address");

				if (substrings[0].Length != 0 )
					throw new Exception("invalid address");

				if (substrings[1].Length == 0) {
					Int32 rem = addr.Split('+').Length  -1 ;
					//	Console.WriteLine("({1}) -> sub: {0}",rem,addr);
					return currentLine + rem;
				}
				else
				{
					Int32 rem  = ParseAddress(substrings[1]);
					//	Console.WriteLine("add: {0}",rem);
					return currentLine + rem;
				}
			}

			if (addr.StartsWith("-"))
			{
				string nn = addr.Substring(1);
				Regex regex = new Regex("\\-+"); // Split on hyphens.
				string[] substrings = regex.Split(addr);

				if (substrings.Length != 2)
					throw new Exception("invalid address");

				if (substrings[0].Length != 0 )
					throw new Exception("invalid address");

				if (substrings[1].Length == 0) {
					Int32 rem = addr.Split('-').Length;
					return currentLine - rem;
				}
				else
				{
					Int32 rem  = ParseAddress(substrings[1]);
					return currentLine - rem;
				}
			}


			// is int
			Int32 num;
			if (Int32.TryParse(addr,out num))
			{
				return num;
			}

			//Console.WriteLine("parsing address: {0}",addr);

			// next regex
			if (addr.StartsWith("/") && addr.EndsWith("/"))
			{
				Regex regex = new Regex("/.*/");

				MatchCollection regMatch = regex.Matches(addr);

				if (regMatch.Count == 1) {
					Match command = regMatch[0];
					string cmd = command.Groups["re"].Value;

					Regex lineReg = new Regex(cmd);

					// for line = current to $
					for (int ll = currentLine; ll<=buffer.GetLineLength(); ll++)
					{
						if (lineReg.IsMatch(buffer.GetLine(ll)))
							return ll;
					}

					// if regex match buffer[line]  return line
					for (int ll = 1; ll<currentLine; ll++)
					{
						if (lineReg.IsMatch(buffer.GetLine(ll)))
							return ll;
					}

					throw new Exception("no match");

				}
			}

			// prev regex

			if (addr.StartsWith("?") && addr.EndsWith("?"))
			{
				Regex regex = new Regex("\\?(?<re>.+)\\?");
				// string[] substrings = regex.Split(addr);

				MatchCollection regMatch = regex.Matches(addr);
			// MatchCollection address = rangeAddress.Matches(line);
			// Report the number of matches found.

				if (regMatch.Count == 1) {
					Match command = regMatch[0];
					string cmd = command.Groups["re"].Value;  // I'm sure there's a better way to reference a single match group

					Regex lineReg = new Regex(cmd);

					for (int ll = currentLine-1; ll>0; ll--)
					{
						if (lineReg.IsMatch(buffer.GetLine(ll)))
							return ll;
					}

						// if regex match buffer[line]  return line
					for (int ll = buffer.GetLineLength(); ll>=currentLine; ll--)
					{
						if (lineReg.IsMatch(buffer.GetLine(ll)))
							return ll;
					}


					throw new Exception("no match");

				}
			}

		//	Console.WriteLine("starts with: {0}",addr);

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

			ui.WriteLine(String.Format("no match: {0}",addr));
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
				//	Console.WriteLine("Read: " + result);
					currentMode = currentMode.parse(result);  // circular dependancy
				} 
				catch (Exception e) 
				{
					lastError = e.Message;
					if (verboseErrorMode) {
						ui.WriteLine(lastError);
					} else {
						ui.WriteLine("?");
					}
				}
			}
		}

	}
}