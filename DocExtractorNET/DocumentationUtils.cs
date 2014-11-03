using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.MSBuild;

namespace DocExtractorNET
{
    public static class DocumentationUtils
    {
        public static DocumentationBase GetDocumentation(string code)
        {
            var tree = CSharpSyntaxTree.ParseText(code);

            var walker = new DocumentationWalker();
            walker.Visit(tree.GetRoot());

            return walker.Documentation;
        }

        public static async Task<ProjectDocumentation> GetDocumentationFromProject(string projFile)
        {
            MSBuildWorkspace workspace = MSBuildWorkspace.Create();
            var project = await workspace.OpenProjectAsync(projFile);

            return await GetProjectDocumentation(project);
        }

        public static async Task<List<ProjectDocumentation>> GetDocumentationFromSolution(string slnFile)
        {
            var projectDocs = new List<ProjectDocumentation>();

            MSBuildWorkspace workspace = MSBuildWorkspace.Create();

            Solution solution = await workspace.OpenSolutionAsync(slnFile);
            var projects = solution.Projects;

            foreach (Project project in projects)
            {
                projectDocs.Add(await GetProjectDocumentation(project));
            }

            return projectDocs;
        }

        private static async Task<ProjectDocumentation> GetProjectDocumentation(Project project)
        {
            ProjectDocumentation pd = new ProjectDocumentation();
            pd.ProjectName = project.Name;

            Compilation compilation = await project.GetCompilationAsync();
            var trees = compilation.SyntaxTrees.ToList();

            var sourceDocs = new List<DocumentationBase>();
            foreach (SyntaxTree tree in trees)
            {
                var walker = new DocumentationWalker();
                walker.Visit(tree.GetRoot());

                sourceDocs.Add(walker.Documentation);
            }

            var namespaces = (from p in sourceDocs
                              group p by p.Name into g
                              select g);

            foreach (var ns in namespaces)
            {
                if (!ns.Any())
                    continue;

                NamespaceDocumentation nd = new NamespaceDocumentation();
                nd.Name = ns.First().Name;

                if (string.IsNullOrEmpty(nd.Name))
                    continue;

                foreach (DocumentationBase doc in ns)
                {
                    nd.Documentation.Add(doc);
                }

                pd.NamespaceDocs.Add(nd);
            }

            return pd;
        }
    }
}