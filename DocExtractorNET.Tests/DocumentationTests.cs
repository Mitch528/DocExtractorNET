using System.Linq;
using Xunit;

namespace DocExtractorNET.Tests
{
    public class DocumentationTests
    {
        [Fact]
        public void test_basic_class_doc()
        {
            const string code = @"
namespace MyProject 
{
    ///<summary>
    ///This is a test summary
    ///123
    ///</summary>
    public class Class1 
    {
    }
}
";
            DocumentationBase docBase = DocumentationUtils.GetDocumentation(code);

            Assert.Equal(1, docBase.Docs.Count);
            Assert.Equal(1, docBase.Docs[0].Tags.Count);

            DocumentationSummary summary = docBase.Docs[0].Tags.OfType<DocumentationSummary>().Single();

            Assert.IsType(typeof(DocumentationTextValue), summary.Values[1]);

            string actualText = string.Join("", summary.Values.OfType<DocumentationTextValue>().Select(p => p.TagValue));

            Assert.Equal(@"
This is a test summary
123
", actualText);

        }

        [Fact]
        public void test_class_doc_with_tag()
        {

            const string code = @"
namespace MyProject 
{
    ///<summary>
    ///<para>
    ///A test
    ///paragraph
    ///</para>
    ///</summary>
    public class Class1 
    {
    }
}
";
            DocumentationBase docBase = DocumentationUtils.GetDocumentation(code);

            Assert.Equal(1, docBase.Docs.Count);
            Assert.Equal(1, docBase.Docs[0].Tags.Count);

            DocumentationSummary summary = docBase.Docs[0].Tags.OfType<DocumentationSummary>().Single();

            Assert.IsType(typeof(DocumentationPara), summary.Values[1]);

            DocumentationPara para = (DocumentationPara)summary.Values[1];

            string actualText = string.Join("", para.Values.OfType<DocumentationTextValue>().Select(p => p.TagValue));

            Assert.Equal(@"
A test
paragraph
", actualText);
        }

        [Fact]
        public void test_multiple_class_basic_doc()
        {
            const string code = @"
namespace MyProject 
{
    ///<summary>
    ///Class1 summary
    ///</summary>
    public class Class1
    {
    }
    
    ///<summary>
    ///Class2 summary
    ///</summary>
    public class Class2
    {
    }
}
";
            DocumentationBase docBase = DocumentationUtils.GetDocumentation(code);

            Assert.Equal(2, docBase.Docs.Count);
            Assert.Equal(1, docBase.Docs[0].Tags.Count);

            DocumentationBase classOne = docBase.Docs[0];
            DocumentationBase classTwo = docBase.Docs[1];

            Assert.Equal("Class1", classOne.Name);
            Assert.Equal("Class2", classTwo.Name);
            Assert.Equal(1, classOne.Tags.Count);
            Assert.Equal(1, classTwo.Tags.Count);

            DocumentationSummary summaryOne = (DocumentationSummary)classOne.Tags[0];
            DocumentationSummary summaryTwo = (DocumentationSummary)classTwo.Tags[0];

            string actualTextOne = string.Join("", summaryOne.Values.Select(p => p.Value));
            string actualTextTwo = string.Join("", summaryTwo.Values.Select(p => p.Value));

            Assert.Equal(@"
Class1 summary
", actualTextOne);
            Assert.Equal(@"
Class2 summary
", actualTextTwo);

        }

        [Fact]
        public void test_multiple_class_basic_nested_doc()
        {
            const string code = @"
namespace MyProject
{
    ///<summary>
    ///Class 1 summary
    ///</summary>
    public class ClassOne
    {
        ///<summary>
        ///Class 2 summary
        ///</summary>
        public class ClassTwo
        {
        }
    }
}
";

            DocumentationBase docBase = DocumentationUtils.GetDocumentation(code);

            Assert.Equal(1, docBase.Docs.Count);

            DocumentationBase classOne = docBase.Docs[0];

            Assert.Equal(1, classOne.Docs.Count);

            DocumentationSummary summaryOne = (DocumentationSummary)classOne.Tags[0];

            string actualTextOne = string.Join("", summaryOne.Values.OfType<DocumentationTextValue>().Select(p => p.TagValue));

            Assert.Equal(@"
Class 1 summary
", actualTextOne);

            Assert.Equal(1, classOne.Docs.Count);

            DocumentationBase classTwo = classOne.Docs[0];

            Assert.Equal(1, classTwo.Tags.Count);

            DocumentationSummary summaryTwo = (DocumentationSummary)classTwo.Tags[0];

            string actualTextTwo = string.Join("", summaryTwo.Values.OfType<DocumentationTextValue>().Select(p => p.TagValue));
        
            Assert.Equal(@"
Class 2 summary
", actualTextTwo);
        }

        [Fact]
        public void test_method_basic_doc()
        {
            const string code = @"
namespace MyProject
{
    public class MyClass
    {
        ///<summary>
        ///A test method
        ///</summary>
        public void MyMethod()
        {
        }
    }
}
";

            DocumentationBase docBase = DocumentationUtils.GetDocumentation(code);

            Assert.Empty(docBase.Tags);
            Assert.Equal(1, docBase.Docs.Count);

            DocumentationBase classDoc = docBase.Docs[0];

            Assert.Equal(0, classDoc.Tags.Count);
            Assert.Equal(1, classDoc.Docs.Count);

            DocumentationBase methodDoc = classDoc.Docs[0];

            Assert.Empty(methodDoc.Docs);
            Assert.Equal(1, methodDoc.Tags.Count);

            DocumentationSummary summary = (DocumentationSummary)methodDoc.Tags[0];

            string actualText = string.Join("", summary.Values.OfType<DocumentationTextValue>().Select(p => p.TagValue));

            Assert.Equal(@"
A test method
", actualText);
        }

        [Fact]
        public void test_interface_basic_doc()
        {
            const string code = @"
namespace MyProject
{
    ///<summary>
    /// Interface summary
    ///</summary>
    public interface MyInterface
    {
    }
}
";
            DocumentationBase docBase = DocumentationUtils.GetDocumentation(code);

            Assert.Equal(1, docBase.Docs.Count);

            DocumentationBase interfaceDoc = docBase.Docs[0];

            Assert.Empty(interfaceDoc.Docs);
            Assert.Equal(1, interfaceDoc.Tags.Count);

            DocumentationSummary summary = (DocumentationSummary)interfaceDoc.Tags[0];

            string actualText = string.Join("", summary.Values.OfType<DocumentationTextValue>().Select(p => p.TagValue));

            Assert.Equal(@"
Interface summary
", actualText);

        }

        [Fact]
        public void test_class_complex_doc()
        {
            const string code = @"
namespace MyProject
{
    ///<summary>
    ///This is a block of code:
    ///<c>
    ///MyClass.CallMethod();
    ///</c>
    ///</summary>
    ///<remarks>
    ///This is a very lonely class
    ///</remarks>
    public class MyClass
    {
    }
}
";

            DocumentationBase docBase = DocumentationUtils.GetDocumentation(code);

            Assert.Equal(1, docBase.Docs.Count);

            DocumentationBase classDoc = docBase.Docs[0];

            Assert.Empty(classDoc.Docs);
            Assert.Equal(2, classDoc.Tags.Count);

            Assert.IsType(typeof(DocumentationSummary), classDoc.Tags[0]);

            DocumentationSummary summary = (DocumentationSummary)classDoc.Tags[0];

            Assert.IsType(typeof(DocumentationTextValue), summary.Values[1]);

            Assert.Equal(5, summary.Values.Count);

            DocumentationTextValue firstText = (DocumentationTextValue)summary.Values[1];

            Assert.Equal("This is a block of code:", firstText.TagValue);

            Assert.IsType(typeof(DocumentationC), summary.Values[3]);

            DocumentationC docCode = (DocumentationC)summary.Values[3];

            Assert.Equal(3, docCode.Values.Count);
            Assert.IsType(typeof(DocumentationTextValue), docCode.Values[1]);

            DocumentationTextValue docText = (DocumentationTextValue)docCode.Values[1];

            Assert.Equal("MyClass.CallMethod();", docText.TagValue);

            Assert.IsType(typeof(DocumentationRemarks), classDoc.Tags[1]);

            DocumentationRemarks remarks = (DocumentationRemarks)classDoc.Tags[1];

            Assert.Equal(3, remarks.Values.Count);
            Assert.IsType(typeof(DocumentationTextValue), remarks.Values[1]);

            DocumentationTextValue docTextTwo = (DocumentationTextValue)remarks.Values[1];

            Assert.Equal("This is a very lonely class", docTextTwo.TagValue);

        }

        [Fact]
        public void test_no_close_tag_class_doc()
        {
            const string code = @"
namespace MyProject
{
    ///<summary>
    ///A test class
    ///<remarks>
    ///Some remarks
    public class Test
    {
    }
}
";

            DocumentationBase docBase = DocumentationUtils.GetDocumentation(code);

            Assert.Equal(1, docBase.Docs.Count);
            Assert.Equal(1, docBase.Docs[0].Tags.Count);

            Assert.Equal(4, docBase.Docs[0].Tags[0].Values.Count);
            Assert.IsType(typeof(DocumentationTextValue), docBase.Docs[0].Tags[0].Values[1]);

            DocumentationTextValue docText = (DocumentationTextValue)docBase.Docs[0].Tags[0].Values[1];

            Assert.Equal("A test class", docText.TagValue);

            Assert.IsType(typeof(DocumentationRemarks), docBase.Docs[0].Tags[0].Values[3]);

            DocumentationRemarks remarks = (DocumentationRemarks)docBase.Docs[0].Tags[0].Values[3];

            Assert.Equal(3, remarks.Values.Count);
            Assert.IsType(typeof(DocumentationTextValue), remarks.Values[1]);

            DocumentationTextValue docTextTwo = (DocumentationTextValue)remarks.Values[1];

            Assert.Equal("Some remarks", docTextTwo.TagValue);

        }

        [Fact]
        public void test_class_basic_doc_no_namespace()
        {
            const string code = @"
///<summary>
///A test class
///</summary>
public class MyClass
{
}
";

            DocumentationBase docBase = DocumentationUtils.GetDocumentation(code);

            Assert.Empty(docBase.Tags);
            Assert.Empty(docBase.Docs);
        }

        [Fact]
        public void test_method_param_doc()
        {
            const string code = @"
namespace MyProject
{
    public class MyClass
    {
        /// <summary>
        /// A test method
        /// </summary>
        /// <param name=""str"">A test string</param>
        public void MyMethod(string str)
        {
        }
    }
}
";

            DocumentationBase docBase = DocumentationUtils.GetDocumentation(code);

            Assert.Equal(1, docBase.Docs.Count);

            DocumentationBase classDoc = docBase.Docs[0];

            Assert.Equal(1, classDoc.Docs.Count);

            DocumentationBase methodDoc = classDoc.Docs[0];

            Assert.Equal(2, methodDoc.Tags.Count);

            Assert.IsType(typeof(DocumentationSummary), methodDoc.Tags[0]);

            DocumentationSummary summary = (DocumentationSummary)methodDoc.Tags[0];

            Assert.Equal(3, summary.Values.Count);
            Assert.IsType(typeof(DocumentationTextValue), summary.Values[1]);

            DocumentationTextValue docText = (DocumentationTextValue)summary.Values[1];

            Assert.Equal("A test method", docText.TagValue);

            Assert.IsType(typeof(DocumentationParam), methodDoc.Tags[1]);

            DocumentationParam param = (DocumentationParam)methodDoc.Tags[1];

            Assert.Equal(1, param.Attributes.Count);
            Assert.Equal("str", param.Attributes[0].Value);

            Assert.Equal(1, param.Values.Count);
            Assert.IsType(typeof(DocumentationTextValue), param.Values[0]);

            DocumentationTextValue docTextParam = (DocumentationTextValue)param.Values[0];

            Assert.Equal("A test string", docTextParam.TagValue);

        }

        [Fact]
        public void test_no_namespace_basic_class_doc()
        {
            const string code = @"
///<summary>
///A test class with no namespace
///</summary>
public class MyClass
{
}
";

            DocumentationBase docBase = DocumentationUtils.GetDocumentation(code);

            Assert.Null(docBase.Name);
            Assert.Equal(0, docBase.Docs.Count);

        }

        [Fact]
        public void test_more_complex_class_doc()
        {
            const string code = @"
namespace MyProject
{
    /// <summary>
    ///   Some summary 
    ///   here
    ///   <see cref=""MyClass""/>
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     Some <c>code</c> in a
    ///     paragraph here
    ///   </para>
    /// </remarks>
    public class MyClass
    {
    }
}
";
            DocumentationBase docBase = DocumentationUtils.GetDocumentation(code);

            Assert.Equal(1, docBase.Docs.Count);

            DocumentationBase classDoc = docBase.Docs[0];

            Assert.Empty(classDoc.Docs);

            Assert.Equal(2, classDoc.Tags.Count);

            Assert.IsType(typeof(DocumentationSummary), classDoc.Tags[0]);

            DocumentationSummary summaryTag = (DocumentationSummary)classDoc.Tags[0];

            Assert.Equal(7, summaryTag.Values.Count);
            Assert.IsType(typeof(DocumentationNewLineValue), summaryTag.Values[0]);
            Assert.IsType(typeof(DocumentationTextValue), summaryTag.Values[1]);
            Assert.IsType(typeof(DocumentationNewLineValue), summaryTag.Values[2]);
            Assert.IsType(typeof(DocumentationTextValue), summaryTag.Values[3]);
            Assert.IsType(typeof(DocumentationNewLineValue), summaryTag.Values[4]);
            Assert.IsType(typeof(DocumentationSee), summaryTag.Values[5]);
            Assert.IsType(typeof(DocumentationNewLineValue), summaryTag.Values[6]);

            DocumentationTextValue textOne = (DocumentationTextValue)summaryTag.Values[1];
            Assert.Equal("Some summary", textOne.TagValue);

            DocumentationTextValue textTwo = (DocumentationTextValue)summaryTag.Values[3];
            Assert.Equal("here", textTwo.TagValue);

            Assert.IsType(typeof(DocumentationRemarks), classDoc.Tags[1]);

            DocumentationRemarks remarks = (DocumentationRemarks)classDoc.Tags[1];

            Assert.Equal(3, remarks.Values.Count);
            Assert.IsType(typeof(DocumentationNewLineValue), remarks.Values[0]);
            Assert.IsType(typeof(DocumentationPara), remarks.Values[1]);
            Assert.IsType(typeof(DocumentationNewLineValue), remarks.Values[2]);

            DocumentationPara paraOne = (DocumentationPara)remarks.Values[1];

            Assert.Equal(7, paraOne.Values.Count);
            Assert.IsType(typeof(DocumentationNewLineValue), paraOne.Values[0]);
            Assert.IsType(typeof(DocumentationTextValue), paraOne.Values[1]);
            Assert.IsType(typeof(DocumentationC), paraOne.Values[2]);
            Assert.IsType(typeof(DocumentationNewLineValue), paraOne.Values[4]);
            Assert.IsType(typeof(DocumentationTextValue), paraOne.Values[5]);
            Assert.IsType(typeof(DocumentationNewLineValue), paraOne.Values[6]);

            DocumentationTextValue textThree = (DocumentationTextValue)paraOne.Values[1];
            Assert.Equal("Some", textThree.TagValue);

            DocumentationC docC = (DocumentationC)paraOne.Values[2];

            Assert.Equal(1, docC.Values.Count);
            Assert.IsType(typeof(DocumentationTextValue), docC.Values[0]);

            DocumentationTextValue textFour = (DocumentationTextValue)docC.Values[0];

            Assert.Equal("code", textFour.TagValue);

        }
    }
}
