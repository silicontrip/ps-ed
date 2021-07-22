using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

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
			try {
				string absoluteFn = Path.GetFullPath(filename);
				buffer = new List<string>(File.ReadAllLines(absoluteFn,readWriteEncoding));
			} catch (FileNotFoundException) {
				buffer = new List<string>();
			}
			this.filename = filename;
		}

		public void ReadFromFile (string fn)
		{
			string absoluteFn = Path.GetFullPath(fn);
			buffer = new List<string>(File.ReadAllLines(fn,readWriteEncoding));
		}

		public void Append (List<string> append, Int32 after)
		{

			buffer.InsertRange(after,append);

		}

		public void Delete (Int32 startLine, Int32 endLine)
		{
			Int32 count = endLine - startLine + 1;
			buffer.RemoveRange(startLine-1,count);
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
		public string GetAbsoluteFilename() { return Path.GetFullPath(this.filename); }
		public void SetFilename(string f) { this.filename = f; }

		public Int32 GetCharacterLength() 
		{
			Int32 total = 0;
			foreach (string ss in buffer)
				total += ss.Length + 1;
			return total;
		}
		public Int32 GetLineLength() { return buffer.Count; }
	//	public bool isUnsaved() { return unsaved; }

		public List<string>GetLines() { return buffer; }

		public List<string> GetLines (Int32 startLine, Int32 endLine) {
			Int32 count = endLine - startLine + 1;
			return buffer.GetRange(startLine-1,count);
		}

		public List<string> GetLines (Int32[] range) {
			return GetLines(range[0],range[1]);
		}

	}
}