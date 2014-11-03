DocExtractorNET
===============

DocExtractorNET is a library that can extract C# XML documentation comments from source code.

Features
----------------
- Extract C# XML documentation comments from source code
- Extract from text
- Extract from a .sln
	- Grouped by project -> namespace
- Extract from a .csproj
	- Grouped by namespace

Example
-----------------
```csharp
DocumentationBase docBase = DocumentationUtils.GetDocumentation(@"
namespace MyProject
{
	///<summary>
	///Some summary
	///</summary>
	public class Class1
	{
	}
}
");

DocumentationBase ns = docBase.Docs[0]; //Class1
DocumentationSummary summary = (DocumentationSummary)docBase.Tags[0];
DocmentationText summaryText = (DocumentationText)summary.Values[1];

Console.WriteLine(summaryText.TagValue);
```