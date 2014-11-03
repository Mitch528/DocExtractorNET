using System.Collections.Generic;

namespace DocExtractorNET
{
    public class DocumentationTag : IDocumentationTagValue
    {
        public virtual string Name { get; set; }

        public List<DocumentationAttribute> Attributes { get; set; }

        public List<IDocumentationTagValue> Values { get; set; }

        public object Value
        {
            get { return Values; }
            set { Values = (List<IDocumentationTagValue>)value; }
        }

        public DocumentationTag()
        {
            Attributes = new List<DocumentationAttribute>();
            Values = new List<IDocumentationTagValue>();
        }

        public override string ToString()
        {
            return string.Join(" ", Values);
        }
    }
}