using System;

namespace DocExtractorNET
{
    public class DocumentationNewLineValue : DocumentationTextValue
    {
        public DocumentationNewLineValue()
            : base(Environment.NewLine)
        {
        }
    }
}