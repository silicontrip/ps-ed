using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace org.gnu.ed {

/*
		public class CommandExample : Command {

			public CommandExample (Controller c, Document d) {
				doc = d;
				con = c; 
			}

			public override void init (string addr, string param)
			{
				NoParam(param);
				addressRange = con.ParseRange(addr,".",1);
			}

			public override Command parse (string line) {
				if (!String.IsNullOrEmpty(line)) {
				}
				return con.GetCommand("command");
			}
		}
 */

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

/** APPEND | (.)a |
 ** Appends text to the buffer after the addressed line. 
 ** The address '0' (zero) is valid for this command; it places the entered text at the beginning of the buffer. 
 ** Text is entered in input mode.
 ** The current address is set to the address of the last line entered or, 
 ** if there were none, to the addressed line. 
 **/
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

/** CHANGE | (.,.)c | 
 ** Changes lines in the buffer. 
 ** The addressed lines are deleted from the buffer, and text is inserted in their place. 
 ** Text is entered in input mode. 
 ** The current address is set to the address of the last line entered or, 
 ** if there were none, to the new address of the line after the last line deleted; 
 ** if the lines deleted were originally at the end of the buffer, 
 ** the current address is set to the address of the new last line; 
 ** if no lines remain in the buffer, the current address is set to zero. 
 ** The lines deleted are copied to the cut buffer.
 **/

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

/** DELETE | (.,.)d | 
 ** Deletes the addressed lines from the buffer. 
 ** The current address is set to the new address of the line after the last line deleted; 
 ** if the lines deleted were originally at the end of the buffer, 
 ** the current address is set to the address of the new last line; 
 ** if no lines remain in the buffer, the current address is set to zero. 
 ** The lines deleted are copied to the cut buffer.
 **/

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
				con.SetCurrentLine(addressRange[0]);
				con.SetCutBuffer(doc.GetRange(addressRange[0],addressRange[01]))
				doc.Delete(addressRange[0],addressRange[01]);
				return con.GetCommand("command");
			}
		}

/** EDIT | e *file* | 
 ** Edits file, and sets the default filename. 
 ** If file is not specified, then the default filename is used. 
 ** Any lines in the buffer are deleted before the new file is read. 
 ** The current address is set to the address of the last line in the buffer.
 ** If file is prefixed with a bang (!), 
 ** then it is interpreted as a shell command whose output is to be read, 
 ** (see shell escape command '!' below). 
 ** In this case the default filename is unchanged.
 ** A warning is printed if any changes have been made 
 ** in the buffer since the last 'w' command that wrote 
 ** the entire buffer to a file. |
 **/
		public class CommandEdit : Command {

			public CommandEdit(Controller c, Document d) {
				doc = d;
				con = c; 
			}

			public override void init (string addr, string param)
			{
				NoAddress(addr);

					if (doc.isUnsaved())
						throw new Exception("warning: file modified");
				if (!String.IsNullOrEmpty(param))
				{
					doc.Edit(param);
				}
				Console.WriteLine(doc.GetLineLength());
			}

			public override Command parse (string line) {
				return con.GetCommand("command");
			}
		}

/** EDIT UNCONDITIONAL | E *file* | 
 ** Edits file unconditionally. 
 ** This is similar to the 'e' command, 
 ** except that unwritten changes are discarded without warning. |
 **/

		public class CommandEditUnconditional : Command {

			public CommandEditUnconditional(Controller c, Document d) {
				doc = d;
				con = c; 
			}

			public override void init (string addr, string param)
			{
				NoAddress(addr);

				//	if (doc.isUnsaved())
				//		throw new Exception("warning: file modified");

				if (!String.IsNullOrEmpty(param))
				{
					doc.Edit(param);
				}
				Console.WriteLine(doc.GetLineLength());
			}

			public override Command parse (string line) {
				return con.GetCommand("command");
			}
		}

/** FILENAME | f *file* | 
 ** Sets the default filename to file. 
 ** If file is not specified, 
 ** then the default unescaped filename is printed. |
 **/

		public class CommandFilename : Command {

			public CommandFilename (Controller c, Document d) {
				doc = d;
				con = c; 
			}

			public override void init (string addr, string param)
			{
				NoAddress(addr);

				if (!String.IsNullOrEmpty(param))
				{
					doc.SetFilename(param);
				} else {
					Console.WriteLine(doc.GetFilename());
				}
			}

			public override Command parse (string line) {
				return con.GetCommand("command");
			}
		}

/** GLOBAL | (1,$)g/re/command-list | 
 **Global command.** 
 ** The global command makes two passes over the file. 
 ** On the first pass, all the addressed lines matching
 ** the regular expression re are marked. 
 ** Then, going sequentially from the beginning of the file to the end of the file, 
 ** the given command-list is executed for each marked line, 
 ** with the current address set to the address of that line. 
 ** Any line modified by the command-list is unmarked. 
 ** The final value of the current address is the value assigned by the
 ** last command in the last command-list executed. 
 ** If there were no matching lines, the current address is unchanged. 
 ** The execution of command-list stops on the first error.
 ** The first command of command-list must appear on the same line as 
 ** the 'g' command. The other commands of command-list must appear on separate lines. All lines of a multi-line command-list except the last line must be terminated with a backslash ('\'). Any commands are allowed, except for 'g', 'G', 'v', and 'V'. The '.' terminating the input mode of commands 'a', 'c', and 'i' can be omitted if it would be the last line of command-list. By default, a newline alone in command-list is equivalent to a 'p' command. If ed is invoked with the command-line option '-G', then a newline in command-list is equivalent to a '.+1p' command. |
 **/

/** GLOBAL INTERACTIVE | (1,$)G/re/ |
 **Interactive global command.** 
 ** Interactively edits the addressed lines matching a regular expression re. 
 ** For each matching line, the line is printed, the current address is set, 
 ** and the user is prompted to enter a command-list. The final value of the 
 ** current address is the value assigned by the last command executed. 
 ** If there were no matching lines, the current address is unchanged.
 ** The format of command-list is the same as that of the 'g' command. 
 ** A newline alone acts as an empty command list. 
 ** A single '&' repeats the last non-empty command list. |
 **/

/** HELP | h | 
 **Help.** 
 Prints an explanation of the last error. 
 **/

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

/** HELP TOGGLE | H | 
 ** Toggles the printing of error explanations. 
 ** By default, explanations are not printed. 
 ** It is recommended that ed scripts begin with this command to aid in debugging. 
 **/

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

/** INSERT | (.)i | 
 ** Inserts text in the buffer before the addressed line. 
 ** The address '0' (zero) is valid for this command; 
 ** it places the entered text at the beginning of the buffer. 
 ** Text is entered in input mode. 
 ** The current address is set to the address of the last line entered or, 
 ** if there were none, to the addressed line. |
 **/

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

/** JOIN | (.,.+1)j | 
 ** Joins the addressed lines, 
 ** replacing them by a single line containing their joined text. 
 ** If only one address is given, this command does nothing. 
 ** If lines are joined, the lines replaced are copied to the 
 ** cut buffer and the current address is set to the address of the joined line. 
 ** Else, the current address is unchanged. |
 **/

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

/** MARK | (.)kx | 
 ** Marks a line with a lower case letter 'x'. 
 ** The line can then be addressed as ''x' (i.e., a single quote followed by 'x') 
 ** in subsequent commands. The mark is not cleared until the line is 
 ** deleted or otherwise modified. 
 ** The current address is unchanged. |
 **/

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

/** LIST | (.,.)l |
 **List command.** 
 Prints the addressed lines unambiguously. 
 The end of each line is marked with a '$', 
 and every '$' character within the text is printed with a preceding backslash. 
 Special characters are printed as escape sequences. 
 The current address is set to the address of the last line printed. |
 **/

/** MOVE| (.,.)m(.) |
 Moves lines in the buffer. 
 The addressed lines are moved to after the right-hand destination address. 
 The destination address '0' (zero) is valid for this command; 
 it moves the addressed lines to the beginning of the buffer. 
 It is an error if the destination address falls within the range of lines to be moved. 
 The current address is set to the new address of the last line moved. |
 **/

/** NUMBER | (.,.)n |
 **Number command.** 
 Prints the addressed lines, preceding each line by its line number and a `<tab>`. 
 The current address is set to the address of the last line printed. |
 **/

/** PRINT | (.,.)p |
 Prints the addressed lines. 
 The current address is set to the address of the last line printed. |
 **/

/** PROMPT | P |
 Toggles the command prompt on and off. 
 Unless a prompt string is specified with the command-line option '-p', 
 the command prompt is by default turned off. 
 The default prompt string is an asterisk ('*'). 
 **/

/** QUIT | q | 
 ** Quits ed. 
 ** A warning is printed if any changes have been made in the 
 ** buffer since the last 'w' command that wrote the entire buffer to a file. |
 **/

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

/** QUIT UNCONDITIONAL | Q | 
 ** Quits ed unconditionally. 
 ** This is similar to the 'q' command, except that unwritten changes are 
 ** discarded without warning. |
 **/

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

/** READ | ($)r *file* |
 ** Reads file and appends it after the addressed line. 
 ** If file is not specified, then the default filename is used. 
 ** If there is no default filename prior to the command, 
 ** then the default filename is set to file. 
 ** Otherwise, the default filename is unchanged. 
 ** The address '0' (zero) is valid for this command; 
 ** it reads the file at the beginning of the buffer. 
 ** The current address is set to the address of the last line read or, 
 ** if there were none, to the addressed line.
 ** If file is prefixed with a bang (!), then it is interpreted as a
 ** shell command whose output is to be read, 
 ** (see shell escape command '!' below). 
 ** In this case the default filename is unchanged. |
 **/

/** SUBSTITUTE | (.,.)s/re/replacement/ | 
 **Substitute command.** 
 ** Replaces text in the addressed lines matching a regular expression re with 
 ** replacement. By default, only the first match in each line is replaced. 
 ** The 's' command accepts any combination of the suffixes 'g', 'count', 'l', 
 ** 'n', and 'p'. If the 'g' (global) suffix is given, then every match is replaced. 
 ** The 'count' suffix, where count is a positive number, causes only the countth 
 ** match to be replaced. 'g' and 'count' can't be specified in the same command. 
 ** 'l', 'n', and 'p' are the usual print suffixes. 
 ** It is an error if no substitutions are performed on any of the addressed lines. 
 ** The current address is set to the address of the last line on which a 
 ** substitution occurred. 
 ** If a line is split, a substitution is considered to have occurred on 
 ** each of the new lines. If no substitution is performed, the current address is unchanged. 
 ** The last line modified is copied to the cut buffer.
 ** re and replacement may be delimited by any character other than `<space>, <newline>` and the characters used by the form of the 's' command shown below. If the last delimiter is omitted, then the last line affected is printed as if the print suffix 'p' were specified. The last delimiter can't be omitted if the 's' command is part of a 'g' or 'v' command-list and is not the last command in the list, because the meaning of the following escaped newline would become ambiguous.
 ** An unescaped '&' in replacement is replaced by the currently matched text. The character sequence '\m' where m is a number in the range [1,9], is replaced by the mth backreference expression of the matched text. If the corresponding backreference expression does not match, then the character sequence '\m' is replaced by the empty string. If replacement consists of a single '%', then replacement from the last substitution is used.
 ** A line can be split by including a newline escaped with a backslash ('\') in replacement. Each backslash in replacement removes the special meaning (if any) of the following character. |
 ** 
 ** REPEAT | (.,.)s | 
 ** Repeats the last substitution. 
 ** This form of the 's' command accepts the 'g' and 'count' suffixes described 
 ** above, and any combination of the suffixes 'p' and 'r'. The 'g' suffix toggles 
 ** the global suffix of the last substitution and resets count to 1. 
 ** The 'p' suffix toggles the print suffixes of the last substitution. 
 ** The 'r' suffix causes the re of the last search to be used instead of the 
 ** re of the last substitution (if the search happened after the substitution). |
 **/

/** TRANSFER | (.,.)t(.) | 
 ** Copies (i.e., transfers) the addressed lines to after the 
 ** right-hand destination address. 
 ** If the destination address is '0' (zero), the lines are copied at the 
 ** beginning of the buffer. The current address is set to the address of the last line copied. |
 **/

/** UNDO | u | 
 ** Undoes the effect of the last command that modified anything in the 
 ** buffer and restores the current address to what it was before the command. 
 ** The global commands 'g', 'G', 'v', and 'V' are treated as a single command by undo. 
 ** 'u' is its own inverse; it can undo only the last command. |
 **/

/** INVERSE GLOBAL | (1,$)v/re/command-list | 
 ** This is similar to the 'g' command except that it applies command-list to each of the addressed lines not matching the regular expression re. |
**/

/** INVERE GLOBAL INTERACTIVE | (1,$)V/re/ | 
 ** This is similar to the 'G' command except that it interactively edits the addressed lines not matching the regular expression re. |
 **/

/** WRITE | (1,$)w file | 
 ** Writes the addressed lines to file. 
 ** Any previous contents of file are lost without warning. 
 ** If there is no default filename, then the default filename is set to file, 
 ** otherwise it is unchanged. 
 ** If no filename is specified, then the default filename is used. 
 ** The current address is unchanged.
 ** If file is prefixed with a bang (!), then it is interpreted as a 
 ** shell command and the addressed lines are written to its standard input, 
 ** (see shell escape command '!' below). 
 ** In this case the default filename is unchanged. Writing the buffer to a 
 ** shell command does not prevent the warning to the user if an attempt is 
 ** made to overwrite or discard the buffer via the 'e' or 'q' commands. |
 **/

/** WRITE QUIT | (1,$)wq file | 
 ** Writes the addressed lines to file, and then executes a 'q' command. |
 **/

// I would reverse x and y (more vi like)
// x : CUT
// y : YANK/PASTE

/** PASTE | (.)x | 
 ** Copies (puts) the contents of the cut buffer to after the addressed line. 
 ** The current address is set to the address of the last line copied. |
 **/

/** YANK | (.,.)y | 
 ** Copies (yanks) the addressed lines to the cut buffer. 
 ** The cut buffer is overwritten by subsequent 'c', 'd', 'j', 's', or 'y' commands. The current address is unchanged. |
 **/

/** SCROLL | (.+1)zn |
 **Scroll.** 
 ** Prints n lines at a time starting at addressed line, and sets window size to n. 
 ** If n is not specified, then the current window size is used. 
 ** Window size defaults to screen size minus two lines, 
 ** or to 22 if screen size can't be determined. 
 ** The current address is set to the address of the last line printed.
 **/

/** SHELL | !*command* |
 **Shell escape command.** Executes command via sh (1). 
 ** If the first character of command is '!', 
 ** then it is replaced by the text of the previous '!command'. 
 ** Thus, '!!' repeats the previous '!command'. 
 ** ed does not process command for backslash ('\') escapes. 
 ** However, an unescaped '%' is replaced by the default filename. 
 ** When the shell returns from execution, a '!' is printed to the standard output. 
 ** The current address is unchanged. |
 **/

/** COMMENT | (.,.)# |
 ** Begins a comment; 
 ** the rest of the line, up to a newline, is ignored. 
 ** If a line address followed by a semicolon is given, 
 ** then the current address is set to that address. 
 ** Otherwise, the current address is unchanged. |
 **/

/** LINE | ($)= |
 ** Prints the line number of the addressed line. 
 ** The current address is unchanged. |
 **/

/** NULL | (.+1)`<newline>` |
 **Null command.** 
 ** An address alone prints the addressed line. 
 ** A `<newline>` alone is equivalent to '+1p'. 
 ** The current address is set to the address of the printed line. |
 **/

}