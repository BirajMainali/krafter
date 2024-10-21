using Krafter.Factory;
using Krafter.Types;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Krafter;

public class Krafter(string entityPath)
{
    public void EntityContextGenerator()
    {
        try
        {
            Console.WriteLine("Starting Krafter...");
            var entityName = GetEntityName();
            var propertiesInfo = GetEntityType();
            Console.WriteLine("Generating source code for entity: " + entityName);
            GenerateSource(propertiesInfo, entityName, "C:\\Experimental\\PolySoundex\\Krafter\\example\\output");
            Console.WriteLine("Krafter has finished generating source code for entity: " + entityName);
        }
        catch (Exception e)
        {
            Console.WriteLine("An error occurred while generating source code: " + e.Message);
        }
        finally
        {
            Console.WriteLine("Krafter has finished...");
        }
    }

    private List<KrafterProperty> GetEntityType()
    {
        if (string.IsNullOrWhiteSpace(entityPath) || !File.Exists(entityPath))
        {
            throw new ArgumentException("The provided entity path is invalid or does not exist.", nameof(entityPath));
        }


        var code = File.ReadAllText(entityPath);
        var syntaxTree = CSharpSyntaxTree.ParseText(code);
        var classDeclarations = syntaxTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().ToList();
        var classDeclaration = classDeclarations.FirstOrDefault();
        if (classDeclaration == null)
        {
            throw new InvalidOperationException("No class found in the provided C# file.");
        }

        var propertiesInfo = new List<KrafterProperty>();
        var properties = classDeclaration.Members.OfType<PropertyDeclarationSyntax>();
        foreach (var property in properties)
        {
            var propertyName = property.Identifier.Text;
            var propertyType = property.Type.ToString();

            var krafterAttribute = property.AttributeLists.SelectMany(a => a.Attributes)
                .FirstOrDefault(a => a.Name.ToString() == "Krafter");
            if (krafterAttribute == null) continue;

            propertiesInfo.Add(new KrafterProperty(propertyType, propertyName));
        }

        return propertiesInfo;
    }

    private void GenerateSource(List<KrafterProperty> properties, string entityName, string outputPath)
    {
        var dtoFactory = KrafterFactory.CreateKrafter(KrafterType.Dto);
        dtoFactory.Generate(properties, entityName, outputPath);

        var serviceFactory = KrafterFactory.CreateKrafter(KrafterType.Service);
        serviceFactory.Generate(properties, entityName, outputPath);

        var controllerFactory = KrafterFactory.CreateKrafter(KrafterType.Controller);
        controllerFactory.Generate(properties, entityName, outputPath);
    }

    private string GetEntityName()
    {
        return Path.GetFileNameWithoutExtension(entityPath);
    }

    private void PrintProperty(KrafterProperty propertyInfo)
    {
        Console.WriteLine($"Property Name: {propertyInfo.Name}");
        Console.WriteLine($"Property Type: {propertyInfo.Name}");
    }
}