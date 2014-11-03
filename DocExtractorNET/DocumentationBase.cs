using System.Collections.Generic;

namespace DocExtractorNET
{
    public class DocumentationBase
    {
        public DocumentationBase()
        {
            Docs = new List<DocumentationBase>();
            Tags = new List<DocumentationTag>();
        }

        public string Name { get; set; }

        public DocumentationBase Parent { get; set; }

        public List<DocumentationBase> Docs { get; set; }

        public List<DocumentationTag> Tags { get; set; }

        public DocumentationBaseType Type { get; set; }
    }
}