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

        [Parameter()]
        public string Prompt
        {
            get { return prompt; }
            set { prompt = value; }
        }
        private string prompt;

        protected override void BeginProcessing()
        {

			Document doc;

		//	Console.WriteLine ("path argument: {0}",path);

			if (String.IsNullOrEmpty(path))
			{
		//		Console.WriteLine ("new doc empty");
				doc = new Document();
			} else {
		//		Console.WriteLine ("new doc path");
				doc= new Document(path);
			}

		//	Console.WriteLine ("new doc: {0}",doc);


			PSHostUserInterface ui = Host.UI;
			cc = new Controller(ui,doc);  // if this were a multi document app, this would be new
			// I must be preparing for a multi document app...
		//	Console.WriteLine ("new controller: {0}",cc);

			if (String.IsNullOrEmpty(prompt))
			{
				cc.SetPrompt("");
			} else {
				cc.SetPrompt(prompt);
			}
			//Console.WriteLine("start");
			cc.Start();
		}
	}
 }
