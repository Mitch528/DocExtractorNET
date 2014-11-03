using System.Collections.Generic;

namespace DocExtractorNET
{
    public class NamespaceDocumentation
    {
        public string Name { get; set; }

        public List<DocumentationBase> Documentation { get; set; }

        public NamespaceDocumentation()
        {
            Documentation = new List<DocumentationBase>();
        }
    }
}
