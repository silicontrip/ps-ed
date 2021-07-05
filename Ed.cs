using System;
using Controller;
using Document;

 namespace GNUed {
 
    [Cmdlet(VerbsCommon.Edit, "Item")]
    public class edMain : PSCmdlet
    {

		Controller cc;

        [Alias("FullName")]
        [Parameter(Position = 0, ValueFromPipelineByPropertyName = true)]
        public string Path
        {
            get { return path; }
            set { path = value; }
        }
        private string path;

        protected override void ProcessRecord()
        {
			cc = new Controller();
		}
	}
 }