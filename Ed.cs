using System;
using System.Management.Automation;
using System.Management.Automation.Host;

 namespace GNUed {
 
    [Cmdlet(VerbsCommon.Edit, "Item")]
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

			cc = new Controller();
			cc.SetUI(ui);
			cc.SetPrompt("");
			if (path.Length > 0)
				cc.SetDocument(new Document(path));

			cc.Start();
		}
	}
 }