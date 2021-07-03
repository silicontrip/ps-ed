using System;
using System.Management.Automation;
using System.Management.Automation.Host;

namespace GNUed {

	public class Controller {

		enum InputMode {
			command,
			input,
			commandlist,
			interactiveglobal
		}

		private Document buffer;
		private Int32 currentLine;
		private List<string> cutBuffer;
		private Dictionary<string,Int32> markBuffer;
		string lastReplaceRegex;
		string lastSearchRegex;
		string lastError;
		bool verboseErrorMode;
		InputMode mode;
		string prompt;
		List<string> undoCommands; // tricky??

		public Controller()
		{
			cutBuffer = new List<string>();
			markBuffer = new Dictionary<string,Int32>();
			lastReplaceRegex = "";
			lastSearchRegex = "";
			lastError="";
			verboseErrorMode = false;
			mode = command;
			prompt = "";
		}

		public setDocument (Document d)
		{
			buffer = d;
			currentLine = buffer.GetLineLength();
		}

		public void Start()
		{
			while (true)
			{
				string result = Host.UI.ReadLine();

				// conditional logic; which mode
				// input {
				// if result != '.' 
				//	read buffer += result
				// else 
				// mode = command. 
			
			}
		}
	}
}