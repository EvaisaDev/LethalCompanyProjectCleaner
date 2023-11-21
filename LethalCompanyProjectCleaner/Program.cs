using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LethalCompanyProjectCleaner
{
    class Assembly
    {
        public string Name { get; set; }
        public string Path { get; set; }

        public Assembly(string name, string path)
        {
            Name = name;
            Path = path;
        }
    }


    public class RemoveCtorMethodCalls : CSharpSyntaxRewriter
    {
        public override SyntaxNode? VisitExpressionStatement(ExpressionStatementSyntax node)
        {
            if (node.Expression is InvocationExpressionSyntax invocation &&
                invocation.Expression.ToString().Contains("ctor"))
            {
                // If the expression is a method call containing "ctor", remove it.
                return null;
            }
            else
            {
                // Otherwise, keep the original node.
                return base.VisitExpressionStatement(node);
            }
        }
    }



    class Program
    {
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("Lethal Company Project Cleaner");
            Console.ResetColor();
            Console.WriteLine("");
            Console.WriteLine("Please enter exported project folder.");

            // read console input
            bool endApp = false;
            
            string input = Console.ReadLine();

            Console.WriteLine("");
            Console.WriteLine("Please enter new unity project folder");

            string input2 = Console.ReadLine();



            while (!endApp)
            {

                string projectFolder = input;
                string newProjectFolder = input2;

                if (!Directory.Exists(projectFolder))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[Error] The exported project folder does not exist.");
                    Console.ResetColor();
                    endApp = true;
                    return;
                }

                if (!Directory.Exists(newProjectFolder))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[Error] The new project folder does not exist.");
                    Console.ResetColor();
                    endApp = true;
                    return;
                }

                string assetsFolder = Path.Combine(projectFolder, "Assets");
                string newAssetsFolder = Path.Combine(newProjectFolder, "Assets");

                if (!Directory.Exists(assetsFolder))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[Error] The exported project is not a valid unity project.");
                    Console.ResetColor();
                    endApp = true;
                    return;
                }

                if (!Directory.Exists(newAssetsFolder))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[Error] The new project is not a valid unity project.");
                    Console.ResetColor();
                    endApp = true;
                    return;
                }


                string packagesFolder = Path.Combine(projectFolder, "Packages");
                if (!Directory.Exists(packagesFolder))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[Error] The given export folder is not a valid unity project.");
                    Console.ResetColor();
                    endApp = true;
                    return;
                }

                string newPackagesFolder = Path.Combine(newProjectFolder, "Packages");
                if (!Directory.Exists(newPackagesFolder))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[Error] The given project folder is not a valid unity project.");
                    Console.ResetColor();
                    endApp = true;
                    return;
                }


                // find Assets/Plugins folder
                string pluginsFolder = Path.Combine(assetsFolder, "Plugins");
                if (!Directory.Exists(pluginsFolder))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("[Warning] Plugins folder was not found, you may have exported the project incorrectly, make sure you used the following settings:");
                    Console.WriteLine("Script Export Format: Hybrid");
                    Console.WriteLine("Script Content Level: Level 1");
                    Console.ResetColor();
                }
                else
                {
                    var assemblies = Directory.GetFiles(pluginsFolder, "*.dll").Select(x => x.ToLower());



                    List<Assembly> assemblyPairs = new List<Assembly>();

                    foreach (string assembly in assemblies)
                    {
                        string assemblyName = Path.GetFileNameWithoutExtension(assembly);
                        string assemblyPath = assembly;

                        assemblyPairs.Add(new Assembly(assemblyName, assemblyPath));
                    }


                    //Console.WriteLine("Found assemblies: " + string.Join(", ", assemblyPairs.Select(x => x.Name)));

                    var assembliesFound = (assemblyPairs.Any(x => x.Name.Contains("unity")) && !assemblyPairs.Any(x => x.Name.Contains("assembly-csharp")));

                    Console.WriteLine("Unity assemby found? " + assemblyPairs.Any(x => x.Name.Contains("unity")));
                    Console.WriteLine("Assembly-CSharp assemby found? " + assemblyPairs.Any(x => x.Name.Contains("assembly-csharp")));

                    if (!assembliesFound)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("[Error] The unity project was exported incorrectly, please export with AssetRipper using the following settings:");
                        Console.WriteLine("Script Export Format: Hybrid");
                        Console.WriteLine("Script Content Level: Level 1");
                        Console.ResetColor();
                        endApp = true;
                        return;
                    }

                    // get any assembly with unity in the name, tolowercase the assemblies first
                    string[] unityAssemblies = assemblyPairs.Where(x => x.Name.Contains("unity")).Select(x => x.Path).ToArray();
                    // remove the assemblies
                    foreach (string assembly in unityAssemblies)
                    {
                        File.Delete(assembly);
                        // remove meta file too
                        File.Delete(assembly + ".meta");
                        Console.WriteLine("Removed assembly: " + Path.GetFileName(assembly));
                    }

                    // remove Assembly-CSharp-firstpass folder
                    string firstPassFolder = Path.Combine(pluginsFolder, "Assembly-CSharp-firstpass");

                    if (Directory.Exists(firstPassFolder))
                    {
                        Directory.Delete(firstPassFolder, true);
                        Console.WriteLine("Removed folder: " + Path.GetFileName(firstPassFolder));
                    }
                }

                // find Assets/Scripts folder
                string scriptsFolder = Path.Combine(assetsFolder, "Scripts");
                if (!Directory.Exists(scriptsFolder))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("[Warning] Plugins folder was not found, you may have exported the project incorrectly, make sure you used the following settings:");
                    Console.WriteLine("Script Export Format: Hybrid");
                    Console.WriteLine("Script Content Level: Level 1");
                    Console.ResetColor();
                }
                else
                {
                    // collect every .cs file in this folder and subfolders
                    string[] csFiles = Directory.GetFiles(scriptsFolder, "*.cs", SearchOption.AllDirectories);
                    foreach (string csFile in csFiles)
                    {
                        // if file is named UnitySourceGeneratedAssemblyMonoScriptTypes_v1, remove it
                        if (csFile.Contains("UnitySourceGeneratedAssemblyMonoScriptTypes_v1"))
                        {
                            File.Delete(csFile);
                            endApp = true;
                            continue;
                        }

                        // read the file
                        string fileContents = File.ReadAllText(csFile);

                        // remove any [RuntimeInitializeOnLoadMethod]
                        fileContents = fileContents.Replace("[RuntimeInitializeOnLoadMethod]", "");

                        // remove the methods using code analysis
                        var tree = CSharpSyntaxTree.ParseText(fileContents);
                        var root = tree.GetRoot();

                        var methodsToRemove = root.DescendantNodes()
                            .OfType<MethodDeclarationSyntax>()
                            .Where(m => m.Identifier.Text.StartsWith("__getTypeName") || m.Identifier.Text.StartsWith("__initializeVariables") || m.Identifier.Text.StartsWith("InitializeRPCS_") || m.Identifier.Text.StartsWith("__rpc_handler_"));

                        var newRoot = root.RemoveNodes(methodsToRemove, SyntaxRemoveOptions.KeepNoTrivia);

                        // remove AsyncStateMachine attribute from any methods
                        /*
                        var methods = newRoot.DescendantNodes()
                            .OfType<MethodDeclarationSyntax>();

                        foreach (var method in methods)
                        {
                            var newMethod = method.WithAttributeLists(SyntaxFactory.List<AttributeListSyntax>());
                            newRoot = newRoot.ReplaceNode(method, newMethod);
                        }*/

                        var classes = newRoot.DescendantNodes()
                        .OfType<ClassDeclarationSyntax>();

                        foreach (var classDeclaration in classes)
                        {
                            // check if the class implements INetworkSerializable or IAsyncStateMachine
                            var implementedInterfaces = classDeclaration.DescendantNodes()
                                .OfType<IdentifierNameSyntax>()
                                .Select(x => x.Identifier.Text);

                            ClassDeclarationSyntax newClassDeclaration = classDeclaration;

                            if (implementedInterfaces.Contains("INetworkSerializable"))
                            {
                                // check if the class has the NetworkSerialize method
                                var hasNetworkSerialize = classDeclaration.DescendantNodes()
                                    .OfType<MethodDeclarationSyntax>()
                                    .Any(x => x.Identifier.Text == "NetworkSerialize");

                                if (!hasNetworkSerialize)
                                {
                                    // add the NetworkSerialize method
                                    var method = SyntaxFactory.MethodDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)), "NetworkSerialize")
                                        .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                                        .WithTypeParameterList(SyntaxFactory.TypeParameterList(SyntaxFactory.SingletonSeparatedList<TypeParameterSyntax>(SyntaxFactory.TypeParameter("T"))))
                                        .WithParameterList(SyntaxFactory.ParameterList(SyntaxFactory.SingletonSeparatedList<ParameterSyntax>(SyntaxFactory.Parameter(SyntaxFactory.Identifier("serializer")).WithType(SyntaxFactory.IdentifierName("BufferSerializer<T>")))))
                                        .WithBody(SyntaxFactory.Block(SyntaxFactory.SingletonList<StatementSyntax>(SyntaxFactory.ThrowStatement(SyntaxFactory.ObjectCreationExpression(SyntaxFactory.IdentifierName("System.NotImplementedException")).WithArgumentList(SyntaxFactory.ArgumentList())))))
                                        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));

                                    method = method.AddConstraintClauses(SyntaxFactory.TypeParameterConstraintClause("T").WithConstraints(SyntaxFactory.SingletonSeparatedList<TypeParameterConstraintSyntax>(SyntaxFactory.TypeConstraint(SyntaxFactory.IdentifierName("IReaderWriter")))));

                                    newClassDeclaration = newClassDeclaration.AddMembers(method);
                                }
                            }

                            if (implementedInterfaces.Contains("IAsyncStateMachine"))
                            {
                                // Check if the class has the MoveNext method
                                var hasMoveNext = classDeclaration.DescendantNodes()
                                    .OfType<MethodDeclarationSyntax>()
                                    .Any(x => x.Identifier.Text == "MoveNext");

                                if (!hasMoveNext)
                                {
                                    // Add the MoveNext method
                                    var moveNextMethod = SyntaxFactory.MethodDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)), "MoveNext")
                                        .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                                        .WithBody(SyntaxFactory.Block(SyntaxFactory.SingletonList<StatementSyntax>(SyntaxFactory.ThrowStatement(SyntaxFactory.ObjectCreationExpression(SyntaxFactory.IdentifierName("System.NotImplementedException")).WithArgumentList(SyntaxFactory.ArgumentList())))))
                                        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));

                                    newClassDeclaration = newClassDeclaration.AddMembers(moveNextMethod);
                                }

                                // Check if the class has the SetStateMachine method
                                var hasSetStateMachine = classDeclaration.DescendantNodes()
                                    .OfType<MethodDeclarationSyntax>()
                                    .Any(x => x.Identifier.Text == "SetStateMachine");

                                if (!hasSetStateMachine)
                                {
                                    // Add the SetStateMachine method
                                    var setStateMachineMethod = SyntaxFactory.MethodDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)), "SetStateMachine")
                                        .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                                        .WithParameterList(SyntaxFactory.ParameterList(SyntaxFactory.SingletonSeparatedList<ParameterSyntax>(SyntaxFactory.Parameter(SyntaxFactory.Identifier("stateMachine")).WithType(SyntaxFactory.IdentifierName("IAsyncStateMachine")))))
                                        .WithBody(SyntaxFactory.Block(SyntaxFactory.SingletonList<StatementSyntax>(SyntaxFactory.ThrowStatement(SyntaxFactory.ObjectCreationExpression(SyntaxFactory.IdentifierName("System.NotImplementedException")).WithArgumentList(SyntaxFactory.ArgumentList())))))
                                        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));

                                    newClassDeclaration = newClassDeclaration.AddMembers(setStateMachineMethod);
                                }
                            }

                            newRoot = newRoot.ReplaceNode(classDeclaration, newClassDeclaration);
                        }


                        if (newRoot != null)
                        {
                            var rewriter = new RemoveCtorMethodCalls();
                            newRoot = rewriter.Visit(newRoot);
                        }





                        var structs = newRoot.DescendantNodes()
                           .OfType<StructDeclarationSyntax>()
                           .ToList(); // Collect all struct declarations into a list

                        while (structs.Any(x => x.DescendantNodes().OfType<ConstructorDeclarationSyntax>().ToList().Count > 0))
                        {
                            foreach (var structDeclaration in structs)
                            {
                                var constructors = structDeclaration.DescendantNodes()
                                    .OfType<ConstructorDeclarationSyntax>()
                                    .ToList(); // Collect all constructor declarations into a list

                                var currentStruct = structDeclaration; // Keep track of the current struct

                                foreach (var constructor in constructors)
                                {
                                    var newStruct = currentStruct.RemoveNode(constructor, SyntaxRemoveOptions.KeepNoTrivia);
                                    newRoot = newRoot.ReplaceNode(currentStruct, newStruct);

                                    currentStruct = newStruct; // Update the current struct
                                }
                            }

                            structs = newRoot.DescendantNodes()
                           .OfType<StructDeclarationSyntax>()
                           .ToList();
                        }


                        var newCode = newRoot.ToFullString();

                        // write the new code back to the file
                        File.WriteAllText(csFile, newCode);



                    }
                }

                // install the following unity packages if another version is not installed: 
                /*
                 com.unity.textmeshpro@3.0.6
                com.unity.animation.rigging@1.2.1 
                com.unity.render-pipelines.high-definition@14.0.8 
                com.unity.inputsystem@1.7.0
                com.unity.ai.navigation@1.1.5
                com.unity.netcode.gameobjects@1.5.2
                 */

                        // the manifest file is located in Packages/manifest.json
                        // add the packages to the dependencies section
                        string manifestFile = Path.Combine(newPackagesFolder, "manifest.json");
                if (!File.Exists(manifestFile))
                {
                    // failed to install packages, give warning
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("[Warning] Failed to install packages, please install the following packages manually:");
                    Console.WriteLine("com.unity.textmeshpro@3.0.6");
                    Console.WriteLine("com.unity.animation.rigging@1.2.1");
                    Console.WriteLine("com.unity.render-pipelines.high-definition@14.0.8");
                    Console.WriteLine("com.unity.inputsystem@1.7.0");
                    Console.WriteLine("com.unity.ai.navigation@1.1.5");
                    Console.WriteLine("com.unity.netcode.gameobjects@1.5.2");
                    Console.ResetColor();
                    endApp = true;
                    return;
                }
                else
                {
                    // read the manifest file
                    // json parse
                    // add dependencies
                    // write back to file

                    JObject data = JObject.Parse(File.ReadAllText(manifestFile));

                    // get the dependencies section
                    if (data.ContainsKey("dependencies"))
                    {

                        JObject dependencies = (JObject)data["dependencies"];

                        // add the packages
                        // check if the package is already installed
                        if (!dependencies.ContainsKey("com.unity.textmeshpro"))
                            dependencies.Add("com.unity.textmeshpro", "3.0.6");
                        if (!dependencies.ContainsKey("com.unity.animation.rigging"))
                            dependencies.Add("com.unity.animation.rigging", "1.2.1");
                        if (!dependencies.ContainsKey("com.unity.render-pipelines.high-definition"))
                            dependencies.Add("com.unity.render-pipelines.high-definition", "14.0.8");
                        if (!dependencies.ContainsKey("com.unity.inputsystem"))
                            dependencies.Add("com.unity.inputsystem", "1.7.0");
                        if (!dependencies.ContainsKey("com.unity.ai.navigation"))
                            dependencies.Add("com.unity.ai.navigation", "1.1.5");
                        if (!dependencies.ContainsKey("com.unity.netcode.gameobjects"))
                            dependencies.Add("com.unity.netcode.gameobjects", "1.5.2");

                        // write back to file
                        data["dependencies"] = dependencies;

                        File.WriteAllText(manifestFile, JsonConvert.SerializeObject(data));

                    }
                    else
                    {
                        // failed to install packages, give warning
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("[Warning] Failed to install packages, please install the following packages manually:");
                        Console.WriteLine("com.unity.textmeshpro@3.0.6");
                        Console.WriteLine("com.unity.animation.rigging@1.2.1");
                        Console.WriteLine("com.unity.render-pipelines.high-definition@14.0.8");
                        Console.WriteLine("com.unity.inputsystem@1.7.0");
                        Console.WriteLine("com.unity.ai.navigation@1.1.5");
                        Console.WriteLine("com.unity.netcode.gameobjects@1.5.2");
                        Console.ResetColor();
                        endApp = true;
                        return;
                    }
                }

                // copy plugins folder from the exported project to the new project
                string newPluginsFolder = Path.Combine(newAssetsFolder, "Plugins");
                if (!Directory.Exists(newPluginsFolder))
                {
                    Directory.CreateDirectory(newPluginsFolder);

                    // copy the files
                    string[] files = Directory.GetFiles(pluginsFolder);
                    foreach (string file in files)
                    {
                        File.Copy(file, Path.Combine(newPluginsFolder, Path.GetFileName(file)));
                    }
                }
                else
                {
                    // copy the files
                    string[] files = Directory.GetFiles(pluginsFolder);
                    foreach (string file in files)
                    {
                        File.Copy(file, Path.Combine(newPluginsFolder, Path.GetFileName(file)), true);
                    }
                }

                // copy scripts folder from the exported project to the new project, include all files and subfolders
                string newScriptsFolder = Path.Combine(newAssetsFolder, "Scripts");
                if (!Directory.Exists(newScriptsFolder))
                {
                    Directory.CreateDirectory(newScriptsFolder);
                }

                // copy the files
                string[] scriptFiles = Directory.GetFiles(scriptsFolder, "*", SearchOption.AllDirectories);
                foreach (string file in scriptFiles)
                {
                    string relativePath = file.Substring(scriptsFolder.Length + 1); // get the relative path
                    string destinationPath = Path.Combine(newScriptsFolder, relativePath); // create the destination path

                    // if contains dissonance in relative path, skip.

                    if (relativePath.Contains("Dissonance"))
                        continue;

                    // ensure the destination directory exists
                    string destinationDir = Path.GetDirectoryName(destinationPath);
                    if (!Directory.Exists(destinationDir))
                    {
                        Directory.CreateDirectory(destinationDir);
                    }

                    File.Copy(file, destinationPath, true);
                }




                // if ProjectSettings folder exists on exported project
                string projectSettingsFolder = Path.Combine(projectFolder, "ProjectSettings");
                if (Directory.Exists(projectSettingsFolder))
                {
                    // take TagManager.asset and copy it to the new project
                    string tagManagerFile = Path.Combine(projectSettingsFolder, "TagManager.asset");
                    // create project settings folder if it doesn't exist
                    string newProjectSettingsFolder = Path.Combine(newProjectFolder, "ProjectSettings");
                    if (!Directory.Exists(newProjectSettingsFolder))
                    {
                        Directory.CreateDirectory(newProjectSettingsFolder);
                    }

                    File.Copy(tagManagerFile, Path.Combine(newProjectSettingsFolder, "TagManager.asset"), true);
                }

                endApp = true;


            }
            return;
        }
    }
}