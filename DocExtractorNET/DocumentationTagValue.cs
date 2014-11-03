namespace DocExtractorNET
{
    public class DocumentationTagValue<T> : IDocumentationTagValue
    {
        public DocumentationTagValue(T value)
        {
            Value = value;
        }

        public T TagValue
        {
            get { return (T) Value; }
            set { Value = value; }
        }

        public object Value { get; set; }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}