using System;
using System.Collections.Generic;
using System.Management.Automation.Host;
// using System.Runtime;
// using System.Text.RegularExpressions;
// using Microsoft.PowerShell;


//using Document;

namespace GNUed {

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
	//	private Regex commandMatch;
		private bool exitControl;

		private Controller()
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

		public void Start()
		{
			while (!exitControl)
			{
				// prompt
				Console.Write(prompt);
				//string result = Host.UI.ReadLine();

				string result = ui.ReadLine();
				try {
					currentMode.parse(result);  // circular dependancy
				} catch (Exception e) {
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