using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace org.gnu.ed {

	public abstract class Command {
		public abstract void init (string start, string param);
		public abstract Command parse (string Line);

		protected Int32 startLine;
		protected Int32 endLine;

		protected Int32[] addressRange;

		protected Controller con;
		protected Document doc;

		protected void NoParam(string param) { if (!String.IsNullOrEmpty(param)) throw new Exception("invalid command suffix"); }
		protected void NoAddress(string param) { if (!String.IsNullOrEmpty(param)) throw new Exception("invalid address"); }

		// void run (string start, string end, string param);
		// bool exit();  // yes or no ???
	//	List<Int32> getRange(string address);
	}

		public class CommandMode : Command {
			// A bunch of stuff

			public CommandMode(Controller c, Document d) {

				// singleAddress = new Regex("\.|\$|\d+|\+\d+|-\d+|\++|\-+|/[^,;]*/|\?[^,;]*\?|'[a-z]");
				// rangeAddress = new Regex(@"^(?<start>\,|\;|\.|\$|\d+|\+\d+|-\d+|\++|\-+|/[^,;]*/|\?[^,;]*\?|'[a-z])*(?<seperator>[,;])*(?<end>\.|\$|\d+|\+\d+|-\d+|\++|\-+|/[^,;]*/|\?[^,;]*\?|'[a-z])*",RegexOptions.Compiled);
				// exitReady = false;
				// CommandList = new Dictionary<string,Command>();

			// do we need these?
				con = c;  // yes
			//	thisDocument = d;  // maybe // not

			}

			public override void init (string start, string param)
			{
				;  // we don't need no initialisation.
				// we don't need no state control.
				// hey caller, leave these methods alone.
				// all in all you're just another, trace in the stack.
			}

			public override Command parse(string line)
			{
					GroupCollection gc = con.ParseCommand(line);

					string cmd = gc["command"].Value;
					string cmdRange = gc["range"].Value;
					// string cmdEnd = gc["end"].Value;
					string cmdParam = gc["parameter"].Value;

					Command newCurrent = con.GetCommand(cmd);

					newCurrent.init(cmdRange,cmdParam);
					return newCurrent.parse("");  //  not sure if line should be passed in here
			}
		}

//  *** APPEND ***

		public class CommandAppend : Command {
			private List<string> buffer;

			public CommandAppend(Controller c, Document d) {
				doc = d;
				con = c; 
			}

			public override void init (string addr, string param)
			{

				NoParam(param);
				addressRange = con.ParseRange(addr,".",1);
				// startLine = address[0];

				buffer = new List<string>(); 

			}

			public override Command parse (string line) {
				if (!String.IsNullOrEmpty(line))
				{
					if (line == ".") {
						// update document
						doc.Append(buffer,addressRange[0]);
						// update current line
						return con.GetCommand("commandmode");
					} else {
						buffer.Add(line);
						return this;
					}
				}
				return this;
			}
		}

// *** change ***

		public class CommandChange : Command {

			private List<string> buffer;

			public CommandChange(Controller c, Document d) {
				doc = d;
				con = c; 
			}

			public override void init (string addr, string param)
			{

				NoParam(param);

				addressRange = con.ParseRange(addr,".,.");
				
				// put Int32[] in the ivar ?
				if ( addressRange.Length == 1 )
					addressRange[1] = address[0];

				buffer = new List<string>(); 

			}

			public override Command parse (string line) {
				if (!String.IsNullOrEmpty(line))
				{
					if (line == ".") {
						// update document
						doc.Delete(addressRange[0],addressRange[1]);
						doc.Append(buffer,addressRange[0]);
						// update cut buffer
						// update current line
						return con.GetCommand("commandmode");
					} else {
						buffer.Add(line);
						return this;
					}
				}
				return this;
			}
		}

		public class CommandDelete : Command {

			public CommandDelete(Controller c, Document d) {
				doc = d;
				con = c; 
			}

			public override void init (string addr, string param)
			{
				NoParam(param);

				addressRange = con.ParseRange(addr,".,.");
				if (addressRange.Length == 1)
					addressRange[1] = addressRange[0];
				/*
				if (String.IsNullOrEmpty(addr))
					address = con.ParseRange(".,.");
				else 
					address = con.ParseRange(addr);
				
				if ( address.Length == 1 )
				{
					startLine = address[0];
					endLine = address[0];
				} else {
					startLine = address[0];
					endLine = address[1];
				}
				*/
				// cut buffer

				// current line
				con.SetCurrentLine(startLine);
				// delete
				doc.Delete(startLine,endLine);

			}
			public override Command parse (string line) {
				return con.GetCommand("commandmode");
			}
		}

		public class CommandEdit : Command {

			public CommandEdit(Controller c, Document d) {
				doc = d;
				con = c; 
			}

			public override void init (string addr, string param)
			{
				NoAddress(addr);
				doc.Edit(param);
			}
			public override Command parse (string line) {
				return con.GetCommand("commandmode");
			}
		}

}