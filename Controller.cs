using System;
using System.Management.Automation;
using System.Management.Automation.Host;

namespace GNUed {

	public interface Command {
		Command parse (string Line);
	}

	public class Controller {

		enum InputMode {
			command,
			input,
			commandlist,
			interactiveglobal
		}

		private Document buffer;
		private Command currentMode;
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
		private static Controller instance=null;

		private  Controller()
		{
			cutBuffer = new List<string>();
			markBuffer = new Dictionary<string,Int32>();
			lastReplaceRegex = "";
			lastSearchRegex = "";
			lastError="";
			verboseErrorMode = false;
			mode = command;
			prompt = "";

			currentMode = new CommandMode(this);
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


		public void setDocument (Document d)
		{
			buffer = d;
			currentLine = buffer.GetLineLength();
		}

		public Document getDocument() { return buffer; }

		public void setPrompt (string p) { prompt = p; }

		public void setCurrentLine(Int32 l) { currentLine = l; }
		public Int32 getCurrentLine() { return currentLine; }

		public string

		public void Start()
		{
			while (!currentMode.exit())
			{
				// prompt
				Console.write(prompt);
				string result = Host.UI.ReadLine();

				try {
					currentMode = currentMode.parse(result);  // circular dependancy
				} catch (Exception e) {
					lastError = e.Message;
					if (verboseErrorMode) {
						Console.writeline(lastError);
					}

				}
			}
		}

		public class CommandMode : Command {
		// A bunch of stuff
			public Command parse (string line)
			{


			}
	}
	}
}