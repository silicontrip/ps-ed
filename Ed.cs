using System;
using System.Management.Automation;
using System.Management.Automation.Host;

 namespace org.gnu.ed {
 
    [Cmdlet(VerbsData.Edit, "Item")]
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

        protected override void BeginProcessing()
        {

			Document doc;

			if (!String.IsNullOrEmpty(path))
			{
				doc= new Document(path);
			} else {
				doc = new Document();
			}

			PSHostUserInterface ui = Host.UI;
			cc = new Controller(ui,doc);  // if this were a multi document app, this would be new
			cc.SetPrompt("");

			//Console.WriteLine("start");
			cc.Start();
		}
	}
 }
