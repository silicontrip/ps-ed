using System;
using System.Collections.Generic;
using System.IO;

namespace GNUed {

public class Document {

	private List<string> buffer;
	private string filename;
	private bool unsaved;

	public Document()
	{
		buffer = new List<string>();
		filename="";
		unsaved=false;
	}

	public Document(string filename)
	{
		Edit(filename);
	}

	public IEnumerable<Int32> Append (List<string> append,Int32 after)
	{
		unsaved = true;
		// insert lines after
		// after++;  // line numbers 1 indexed, not 0 
		SortedSet<Int32> modified = new SortedSet<Int32>();
		foreach (string line in append)
		{
			modified.Add(after+1);
			buffer.Insert(after++,line);
			}
			return modified;
		// return array with after+1 + append.length
	}

	public IEnumerable<Int32> Change (List<string> append,Int32 startLine, Int32 toLine)
	{
				unsaved = true;

		// delete startLine to toLine
		buffer.RemoveRange(startLine-1, toLine - startLine + 1);
		// append after startLine - 1
		buffer.InsertRange(toLine-2,append);
		// // range 1 = startLine - toLine
		// // range 2 = startLine + append.length
		Int32 end = Math.Max(toLine, startLine + append.Count);
		// return array startLine TO emd
		return Enumerable.Range(startLine, end - startLine+1);
	}

	public IEnumerable<Int32> Delete (Int32 startLine, Int32 endLine)
	{
				unsaved = true;

		//buffer.RemoveRange(startLine-1,endLine-1);
		SortedSet<Int32> modified = new SortedSet<Int32>();
		for (Int32 index = endLine; index>=startLine;--index) {
						modified.Add(index);
						buffer.RemoveAt(index-1);
		}
			return modified;
				// delete startLine to toLine
		// return array startLine to toline
	}

	public IEnumerable<Int32> Edit (string filename)
	{
		// new buffer
				buffer = new List<string>();

		// unsaved = false
				unsaved=false;

		// read file or command
		if (filename.StartsWith('!'))
		{
			; // do something
		} else {
			// System.Text.UTF8Encoding
// System.Text.ASCIIEncoding
			buffer = ReadAllLines(filename,System.Text.ASCIIEncoding);  // yes we'll allow it to throw.
			this.filename = filename;
		}
		// set filename
	
		// return array 1 to buffer.length
		return Enumerable.Range(1, buffer.Count);

	}

	public IEnumerable<Int32> Insert (List<string> append,Int32 before)
	{
				unsaved = true;

		// insert lines before 
		buffer.InsertRange(before-1,append);
		// return array with after+1 + append.length
				return Enumerable.Range(before, append.Count);

	}

	public IEnumerable<Int32> Join (Int32 startLine, Int32 toLine)
	{
				unsaved = true;

		string singleLine = buffer[startLine-1];
		for (Int32 line=startLine; line<=toLine;  line++)
			singleLine += " " + buffer[line];
		return Change ( singleLine , startLine, toLine)
	}

	public IEnumerable<Int32> Move (Int32 startLine, Int32 endLine, Int32 toLine)
	{
				unsaved = true;

		// lines = buffer[startLine,endLine]
		List<string> lines = buffer.GetRange(startLine-1,endLine-startLine+1);
		buffer.RemoveRange(startLine-1, endLine - startLine + 1);

		// Delete (startLine,endLine)
		return Append (lines,toLine);
	}

	public IEnumerable<Int32> Read (string filename, Int32 startLine)
	{
				unsaved = true;

		// if (!this.filename &&  filename)
		//		this.filename = filename
		// if (!filename)
		//	filename = this.filename

		// lines = read(filename)
		// return Append (lines,startLine)

	}

	public IEnumerable<Int32> Replace (string regex, string replacement, Int32 startLine,Int32 endline, int count)
	{
				unsaved = true;

		// global = (count == 0)
		// make regex
		// new modified array
		// for line = startline to endLine
		// 	if match 
		//		replace
		//		modified += line
		// return modified
	}

	public IEnumerable<Int32> Transfer (Int32 startLine, Int32 endLine, Int32 toLine)
	{
				unsaved = true;

		// lines = buffer[startLine,endLine]
		// Append (lines,toLine)
		// return [startLine,toLine]
	}

		public IEnumerable<Int32> Write (string file, Int32 startLine, Int32 endLine)
	{
		// default filename voodoo
		// lines = buffer[startLine,endLine]
		// write file or command pipe
		// if file 
		unsaved = false;
	}

		public IEnumerable<Int32> WriteAppend (Int32 startLine, Int32 endLine, Int32 toLine)
	{
		//

		// lines = buffer[startLine,endLine]
		// append file 
		// if file 
		unsaved = false;
	}

	public string GetFilename() { return this.filename; }
	public List<string>GetLines() { return buffer; }
	public Int32 GetCharacterLength() 
	{
		Int32 total = 0;
		foreach (string ss in buffer)
			total += ss.length + 1;
		return total;
	}
	public Int32 GetLineLength() { return buffer.count; }

	public bool isUnsaved() { return unsaved; }

}
}