using System.Collections.Generic;

namespace DocExtractorNET
{
    public class ProjectDocumentation
    {

        public string ProjectName { get; set; }

        public List<NamespaceDocumentation> NamespaceDocs { get; set; }

        public ProjectDocumentation()
        {
            NamespaceDocs = new List<NamespaceDocumentation>();
        }

    }
}
