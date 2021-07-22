using System;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Host;

 namespace org.gnu.ed {
 
 // Get-ChildItem ...  Get-Item ... Get-Content  
 // I really do think that they went overboard with the verb-noun paradigm
    [Cmdlet(VerbsData.Edit, "Content")]
    public class edMain : PSCmdlet
    {

		Controller cc;

        public
        edMain()
        {
            // empty, provided per design guidelines.
        }

        [Alias("FullName")]
        [Parameter(Position = 0, ValueFromPipelineByPropertyName = true)]
        public string Path
        {
            get { return path; }
            set { path = value; }
        }
        private string path;

        [Parameter()]
        public string Prompt
        {
            get { return prompt; }
            set { prompt = value; }
        }
        private string prompt;

        protected override void BeginProcessing()
        {

			Document doc = new Document();
			PSHostUserInterface ui = Host.UI;

			string current = Directory.GetCurrentDirectory();
			SessionState ss = new SessionState();
			Directory.SetCurrentDirectory(ss.Path.CurrentFileSystemLocation.Path);


			if (!String.IsNullOrEmpty(path))
			{
				try {
					doc.SetFilename(path);
					doc.ReadFromFile(path);
					ui.WriteLine(String.Format("{0}",doc.GetCharacterLength()));

				} catch (Exception e) {
					ui.WriteLine(e.Message);
				}
			}

			cc = new Controller(ui,doc);  // if this were a multi document app, this would be new
			// I must be preparing for a multi document app...

			if (String.IsNullOrEmpty(prompt))
			{
				cc.SetPrompt("");
			} else {
				cc.SetPrompt(prompt);
			}
			//Console.WriteLine("start");
			cc.Start();
			Directory.SetCurrentDirectory(current);

		}
	}
 }
