using System;
using System.Management.Automation;
using System.Management.Automation.Host;

 namespace GNUed {
 
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
			PSHostUserInterface ui = Host.UI;

	 //Console.WriteLine("controller isntance");
			cc = Controller.Instance;
		//		Console.WriteLine("set ui");

			cc.SetUI(ui);
		//					Console.WriteLine("set prompt");

			cc.SetPrompt("");
		//								Console.WriteLine("path length");

			if (!String.IsNullOrEmpty(path))
				cc.SetDocument(new Document(path));
							Console.WriteLine("start");

			cc.Start();
		}
	}
 }
