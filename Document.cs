using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace org.gnu.ed {

	public class Document {

		private List<string> buffer;
		private string filename;
	//	private bool unsaved;
		private Encoding readWriteEncoding;

		public Document()
		{
			buffer = new List<string>();
			this.filename="";
			readWriteEncoding = new ASCIIEncoding();

		//	unsaved=false;
		}

		public Document(string filename)
		{
			readWriteEncoding = new ASCIIEncoding();
			buffer = new List<string>(File.ReadAllLines(filename,readWriteEncoding));
		}

		public void ReadFromFile (string fn)
		{
			buffer = new List<string>(File.ReadAllLines(fn,readWriteEncoding));
		}

		public void Append (List<string> append, Int32 after)
		{
			// unsaved = true;
			// insert lines after
			// after++;  // line numbers 1 indexed, not 0 
			//SortedSet<Int32> modified = new SortedSet<Int32>();
			foreach (string line in append)
			{
			//	modified.Add(after);
				buffer.Insert(after++,line);
			}
			//return modified;
			// return array with after+1 + append.length
		}

		public void Delete (Int32 startLine, Int32 endLine)
		{
			//unsaved = true;

			buffer.RemoveRange(startLine-1,endLine-1);
		//	SortedSet<Int32> modified = new SortedSet<Int32>();
			// delete startLine to toLine
			// for (Int32 index = endLine; index>=startLine;--index) {
				//modified.Add(index);
			//	buffer.RemoveAt(index-1);
		//	}
			//return modified;
			// return array startLine to toline
		}

		public void Delete (Int32[] range)
		{
		//	unsaved = true;
			 Delete(range[0],range[1]);
		}

		public void Insert (List<string> append, Int32 before)
		{
			//unsaved = true;

			// insert lines before 
			buffer.InsertRange(before-1,append);
			// return array with after+1 + append.length
			// return Enumerable.Range(before, append.Count);

		}

		public string GetLine(Int32 l) { return buffer[l-1]; }
		public string GetFilename() { return this.filename; }
		public void SetFilename(string f) { this.filename = f; }

		public List<string>GetLines() { return buffer; }
		public Int32 GetCharacterLength() 
		{
			Int32 total = 0;
			foreach (string ss in buffer)
				total += ss.Length + 1;
			return total;
		}
		public Int32 GetLineLength() { return buffer.Count; }
	//	public bool isUnsaved() { return unsaved; }

		public List<string> GetRange (Int32 startLine, Int32 endLine) {
			return buffer.GetRange(startLine-1,endLine-startLine+1);
		}

		public List<string> GetRange (Int32[] range) {
			return buffer.GetRange(range[0],range[1]);
		}

	}
}