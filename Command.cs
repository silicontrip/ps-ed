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
		protected void OrderAddress(Int32[] a) { if (a[1]<a[0]) throw new Exception("invalid address"); }

		// void run (string start, string end, string param);
		// bool exit();  // yes or no ???
	//	List<Int32> getRange(string address);
	}

		public class CommandMode : Command {
			// A bunch of stuff

			public CommandMode(Controller c, Document d) {
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

				//	Console.WriteLine("command parser -> "+ line);

					GroupCollection gc = con.ParseCommand(line);

					string cmd = gc["command"].Value;
					string cmdRange = gc["range"].Value;
					// string cmdEnd = gc["end"].Value;
					string cmdParam = gc["parameter"].Value;

					Console.WriteLine("command -> " + cmd);
					Console.WriteLine("range -> " + cmdRange);
					Console.WriteLine("param -> " + cmdParam);

					Command newCurrent = con.GetCommand(cmd);

					newCurrent.init(cmdRange,cmdParam);
					return newCurrent.parse(null);  //  not sure if line should be passed in here
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

				Console.WriteLine("Command Append Initialise");

				NoParam(param);
				addressRange = con.ParseRange(addr,".",1);
				// startLine = address[0];

				buffer = new List<string>(); 

			}

			public override Command parse (string line) {

				Console.WriteLine("Command Append parse -> " + line);

				if (!String.IsNullOrEmpty(line))
				{
					if (line == ".") {
						// update document
						doc.Append(buffer,addressRange[0]);
						if(buffer.Count ==0)
							con.SetCurrentLine(addressRange[0]);
						else
							con.SetCurrentLine(addressRange[0]+buffer.Count - 1);
						// update current line
						return con.GetCommand("command");
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
					addressRange[1] = addressRange[0];

				OrderAddress(addressRange);
				buffer = new List<string>(); 

			}

			public override Command parse (string line) {
				if (!String.IsNullOrEmpty(line))
				{
					if (line == ".") {
						// update cut buffer

						con.SetCutBuffer(doc.GetRange(addressRange[0],addressRange[1]));
						// update document
						doc.Delete(addressRange[0],addressRange[1]);
						doc.Append(buffer,addressRange[0]);
						// update current line
						return con.GetCommand("command");
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
				OrderAddress(addressRange);

			}
			public override Command parse (string line) {
				con.SetCurrentLine(startLine);
				doc.Delete(startLine,endLine);
				return con.GetCommand("command");
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
				return con.GetCommand("command");
			}
		}

		public class CommandHelp : Command {

			public CommandHelp(Controller c, Document d) {
				doc = d;
				con = c; 
			}

			public override void init (string addr, string param)
			{
				NoAddress(addr);
				NoParam(param);
			}

			public override Command parse (string line) {
				Console.WriteLine(con.GetError());
				return con.GetCommand("command");
			}
		}

		public class CommandHelpToggle : Command {

			public CommandHelpToggle(Controller c, Document d) {
				doc = d;
				con = c; 
			}

			public override void init (string addr, string param)
			{
				NoAddress(addr);
				NoParam(param);
			}

			public override Command parse (string line) {
				con.ToggleVerboseHelp();
				return con.GetCommand("command");
			}
		}

		public class CommandInsert : Command {
			private List<string> buffer;

			public CommandInsert(Controller c, Document d) {
				doc = d;
				con = c; 
			}

			public override void init (string addr, string param)
			{

				//Console.WriteLine("Command Append Initialise");

				NoParam(param);
				addressRange = con.ParseRange(addr,".",1);

				buffer = new List<string>(); 

			}

			public override Command parse (string line) {
				if (!String.IsNullOrEmpty(line))
				{
					if (line == ".") {
						// update document
						doc.Insert(buffer,addressRange[0]);
						// update current line
						return con.GetCommand("command");
					} else {
						buffer.Add(line);
						return this;
					}
				}
				return this;
			}
		}

		public class CommandJoin : Command {

			public CommandJoin(Controller c, Document d) {
				doc = d;
				con = c; 
			}

			public override void init (string addr, string param)
			{
				NoParam(param);

				// default = .,.+1
				// single; does nothing, no error
				addressRange = con.ParseRange(addr,".,.+1");

				OrderAddress(addressRange);

			}
			public override Command parse (string line) {
				if(addressRange.Length == 2)
				{
					con.SetCurrentLine(startLine);
					doc.Join(startLine,endLine);
				}
				return con.GetCommand("command");
			}
		}

		public class CommandMark : Command {

			public CommandMark(Controller c, Document d) {
				doc = d;
				con = c; 
			}

			public override void init (string addr, string param)
			{
				//NoParam(param);
				if (param.Length != 1) 
					throw new Exception("invalid command suffix"); 

				// default = .,.+1
				// single; does nothing, no error
				addressRange = con.ParseRange(addr,".",1);

				// OrderAddress(addressRange);
					con.SetMark(param,addressRange[0]);

			}
			public override Command parse (string line) {
				return con.GetCommand("command");
			}
		}

		public class CommandQuit : Command {

			public CommandQuit(Controller c, Document d) {
				doc = d;
				con = c; 
			}

			public override void init (string addr, string param)
			{
				NoAddress(addr);
				NoParam(param);

			}
			public override Command parse (string line) {
				if (doc.isUnsaved())
					throw new Exception("warning: file modified");
				con.SetExit(true);
				return null;
			}
		}

		public class CommandQuitForce : Command {

			public CommandQuitForce(Controller c, Document d) {
				doc = d;
				con = c; 
			}

			public override void init (string addr, string param)
			{
				NoAddress(addr);
				NoParam(param);

			}
			public override Command parse (string line) {
				con.SetExit(true);
				return null;
			}
		}


}