using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DocExtractorNET
{
    public class DocumentationWalker : CSharpSyntaxWalker
    {
        public Dictionary<string, Func<DocumentationAttribute>> TagAttributes = new Dictionary
            <string, Func<DocumentationAttribute>>
        {
            { "cref", () => new DocumentationCrefAttribute() },
            { "name", () => new DocumentationNameAttribute() }
        };

        public Dictionary<string, Func<DocumentationTag>> Tags = new Dictionary<string, Func<DocumentationTag>>
        {
            { "see", () => new DocumentationSee() },
            { "seealso", () => new DocumentationSeeAlso() },
            { "typeparamref", () => new DocumentationTypeParamRef() },
            { "summary", () => new DocumentationSummary() },
            { "para", () => new DocumentationPara() },
            { "c", () => new DocumentationC() },
            { "code", () => new DocumentationCode() },
            { "remarks", () => new DocumentationRemarks() },
            { "param", () => new DocumentationParam() },
            { "example", () => new DocumentationExample() },
            { "exception", () => new DocumentationException() },
            { "include", () => new DocumentationInclude() },
            { "list", () => new DocumentationList() },
            { "paramref", () => new DocumentationParamRef() },
            { "permission", () => new DocumentationPermission() },
            { "typeparam", () => new DocumentationTypeParam() },
            { "value", () => new DocumentationValue() }
        };

        public DocumentationWalker()
            : base(SyntaxWalkerDepth.StructuredTrivia)
        {
            Documentation = new DocumentationBase { Type = DocumentationBaseType.Namespace };
        }

        public DocumentationBase Documentation { get; set; }

        public override void VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
        {
            base.VisitNamespaceDeclaration(node);

            Documentation.Name = node.Name.ToString();
            Visit(node.ChildNodes(), Documentation);
        }

        private void Visit(IEnumerable<SyntaxNode> nodes, DocumentationBase root)
        {
            foreach (SyntaxNode child in nodes)
            {
                SyntaxKind kind = child.CSharpKind();

                DocumentationBaseType type;

                switch (kind)
                {
                    case SyntaxKind.StructDeclaration:
                    case SyntaxKind.ClassDeclaration:
                    case SyntaxKind.InterfaceDeclaration:
                    case SyntaxKind.EnumDeclaration:
                    case SyntaxKind.DelegateDeclaration:
                        type = DocumentationBaseType.Type;
                        break;
                    case SyntaxKind.FieldDeclaration:
                        type = DocumentationBaseType.Field;
                        break;
                    case SyntaxKind.PropertyDeclaration:
                        type = DocumentationBaseType.Property;
                        break;
                    case SyntaxKind.MethodDeclaration:
                        type = DocumentationBaseType.Method;
                        break;
                    case SyntaxKind.EventDeclaration:
                        type = DocumentationBaseType.Event;
                        break;
                    default:
                        continue;
                }

                var doc = new DocumentationBase();
                doc.Name = GetName(child);
                doc.Type = type;
                doc.Tags.AddRange(GetDocComments(child));

                Visit(child.ChildNodes(), doc);

                root.Docs.Add(doc);
            }
        }

        private string GetName(SyntaxNode node)
        {
            string name;

            switch (node.CSharpKind())
            {
                case SyntaxKind.StructDeclaration:
                    var structDec = (StructDeclarationSyntax)node;
                    name = structDec.Identifier.Text;
                    break;
                case SyntaxKind.ClassDeclaration:
                    var classDec = (ClassDeclarationSyntax)node;
                    name = classDec.Identifier.Text;
                    break;
                case SyntaxKind.InterfaceDeclaration:
                    var interDec = (InterfaceDeclarationSyntax)node;
                    name = interDec.Identifier.Text;
                    break;
                case SyntaxKind.MethodDeclaration:
                    var methodDec = (MethodDeclarationSyntax)node;
                    name = methodDec.Identifier.Text;
                    break;
                case SyntaxKind.PropertyDeclaration:
                    var propDec = (PropertyDeclarationSyntax)node;
                    name = propDec.Identifier.Text;
                    break;
                case SyntaxKind.FieldDeclaration:
                    var fieldDec = (FieldDeclarationSyntax)node;
                    name = fieldDec.Declaration.Variables.First().Identifier.Text;
                    break;
                case SyntaxKind.EnumDeclaration:
                    var enumDec = (EnumDeclarationSyntax)node;
                    name = enumDec.Identifier.Text;
                    break;
                default:
                    name = String.Empty;
                    break;
            }

            name = name.Trim();

            return name;
        }

        protected List<DocumentationTag> GetDocTags(IEnumerable<SyntaxNode> content)
        {
            var tags = new List<DocumentationTag>();

            foreach (SyntaxNode c in content)
            {
                DocumentationTag tag = GetDocTag(c, null);

                if (tag != null)
                {
                    tags.Add(tag);
                }
            }

            return tags;
        }

        protected DocumentationTag GetDocTag(SyntaxNode node, DocumentationTag root)
        {

            switch (node.CSharpKind())
            {
                case SyntaxKind.XmlElement:

                    var xmlElement = (XmlElementSyntax)node;
                    string tagName = xmlElement.StartTag.Name.LocalName.Text;

                    var tag = Tags.ContainsKey(tagName) ? Tags[tagName]() : new DocumentationTag();
                    tag.Name = tagName;
                    tag.Attributes.AddRange(GetAttributes(xmlElement.StartTag.Attributes));

                    if (root == null)
                        root = tag;
                    else
                        root.Values.Add(tag);

                    foreach (SyntaxNode child in node.ChildNodes())
                    {
                        GetDocTag(child, tag);
                    }

                    break;
                case SyntaxKind.XmlEmptyElement:
                    var xmlEmpty = (XmlEmptyElementSyntax)node;
                    string name = xmlEmpty.Name.LocalName.Text;

                    var emptyTag = Tags.ContainsKey(name) ? Tags[name]() : new DocumentationTag();
                    emptyTag.Name = name;
                    emptyTag.Attributes.AddRange(GetAttributes(xmlEmpty.Attributes));

                    root.Values.Add(emptyTag);
                    break;
                case SyntaxKind.XmlText:

                    var xmlTxt = (XmlTextSyntax)node;

                    foreach (SyntaxToken txtToken in xmlTxt.TextTokens)
                    {
                        switch (txtToken.CSharpKind())
                        {
                            case SyntaxKind.XmlTextLiteralNewLineToken:
                                root.Values.Add(new DocumentationNewLineValue());
                                break;
                            case SyntaxKind.XmlTextLiteralToken:

                                string text = txtToken.Text.Trim();

                                if (string.IsNullOrEmpty(text))
                                    continue;

                                root.Values.Add(new DocumentationTextValue(text));
                                break;
                        }
                    }

                    break;
            }

            return root;
        }

        protected List<DocumentationAttribute> GetAttributes(IEnumerable<XmlAttributeSyntax> attributes)
        {
            var tagValues = new List<DocumentationAttribute>();

            foreach (XmlAttributeSyntax attrib in attributes)
            {
                var attribChildren = attrib.ChildNodes().ToList();

                //some attribute types do not provide a value for some reason
                //TODO: figure out why
                if (attribChildren.Count == 1)
                    continue;

                XmlNameSyntax xmlName = attribChildren.Where(p => p.CSharpKind() == SyntaxKind.XmlName)
                        .Cast<XmlNameSyntax>()
                        .Single();

                SyntaxNode attribVal = attribChildren.Except(new[] { xmlName }).Single();

                if (!TagAttributes.ContainsKey(xmlName.LocalName.Text))
                    continue;

                DocumentationAttribute val = TagAttributes[xmlName.LocalName.Text]();
                val.Name = xmlName.LocalName.Text;
                val.Value = attribVal.ToString();

                tagValues.Add(val);
            }

            return tagValues;
        }

        protected List<DocumentationTag> GetDocComments(SyntaxNode node)
        {
            foreach (var trivia in node.GetLeadingTrivia())
            {
                switch (trivia.CSharpKind())
                {
                    case SyntaxKind.SingleLineDocumentationCommentTrivia:
                    case SyntaxKind.MultiLineDocumentationCommentTrivia:

                        var syntax = (DocumentationCommentTriviaSyntax)trivia.GetStructure();
                        var content = syntax.Content.Where(p => p.CSharpKind() == SyntaxKind.XmlElement).Cast<XmlElementSyntax>();

                        return GetDocTags(content);
                }
            }

            return new List<DocumentationTag>();
        }
    }
}