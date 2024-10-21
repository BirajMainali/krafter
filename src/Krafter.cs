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
            var maps = new Dictionary<KrafterAttributeType, List<KrafterProperty>>();
            var entityName = GetEntityName();
            var krafterPropertyInfos = GetEntityType();
            Console.WriteLine("Generating source code for entity: " + entityName);
            krafterPropertyInfos.ForEach(krafterPropertyInfo => PrintProperty(krafterPropertyInfo.Property));

            foreach (var propertyInfo in krafterPropertyInfos)
            {
                AddPropertyToMap(maps, propertyInfo.IncludeInInsert, propertyInfo.Property, KrafterAttributeType.Insert);
                AddPropertyToMap(maps, propertyInfo.IncludeInUpdate, propertyInfo.Property, KrafterAttributeType.Update);
                AddPropertyToMap(maps, propertyInfo.IncludeInInput, propertyInfo.Property, KrafterAttributeType.Input);
                AddPropertyToMap(maps, propertyInfo.IncludeInOutput, propertyInfo.Property, KrafterAttributeType.Output);
            }

            GenerateSource(maps, entityName, "C:\\Experimental\\PolySoundex\\Krafter\\example\\output");
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

    private List<KrafterPropertyInfo> GetEntityType()
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


        var propertiesInfo = new List<KrafterPropertyInfo>();
        var properties = classDeclaration.Members.OfType<PropertyDeclarationSyntax>();
        foreach (var property in properties)

        {
            var propertyName = property.Identifier.Text;
            var propertyType = property.Type.ToString();
            var attributeSyntax = property.AttributeLists
                .SelectMany(attrList => attrList.Attributes)
                .FirstOrDefault(attr => attr.Name.ToString() == "Krafter");

            if (attributeSyntax == null) continue;
            var includeInInsert = false;
            var includeInUpdate = false;
            var includeInInput = false;
            var includeInOutput = false;


            if (attributeSyntax.ArgumentList == null) continue;
            var arguments = attributeSyntax.ArgumentList.Arguments;
            if (arguments is [_, ..])
            {
                includeInInsert = EvaluateArgument(arguments[0]);
            }

            if (arguments is [_, _, ..])
            {
                includeInUpdate = EvaluateArgument(arguments[1]);
            }

            if (arguments is [_, _, _, ..])
            {
                includeInInput = EvaluateArgument(arguments[2]);
            }

            if (arguments is [_, _, _, _, ..])
            {
                includeInOutput = EvaluateArgument(arguments[3]);
            }


            propertiesInfo.Add(new KrafterPropertyInfo
            {
                Property = new KrafterProperty(propertyType, propertyName),
                IncludeInInsert = includeInInsert,
                IncludeInUpdate = includeInUpdate,
                IncludeInInput = includeInInput,
                IncludeInOutput = includeInOutput
            });
        }

        return propertiesInfo;
    }

    private bool EvaluateArgument(AttributeArgumentSyntax argument)
    {
        if (argument.Expression is LiteralExpressionSyntax literal)

        {
            return literal.Token.Value is true;
        }

        return false;
    }

    private void AddPropertyToMap(Dictionary<KrafterAttributeType, List<KrafterProperty>> maps, bool include,
        KrafterProperty propertyInfo, KrafterAttributeType attributeType)
    {
        if (!include) return;
        Console.WriteLine($"Property {propertyInfo.Name} is included in {attributeType.ToString().ToLower()}");
        if (!maps.ContainsKey(attributeType)) maps[attributeType] = new List<KrafterProperty>();
        maps[attributeType].Add(new KrafterProperty(propertyInfo.Type, propertyInfo.Name));
    }

    private void GenerateSource(Dictionary<KrafterAttributeType, List<KrafterProperty>> maps, string entityName, string outputPath)
    {
        var dtoFactory = KrafterFactory.CreateKrafter(KrafterType.Dto);
        dtoFactory.Generate(maps.ToDictionary(k => k.Key, v => v.Value.ToArray()), entityName, outputPath);

        var serviceFactory = KrafterFactory.CreateKrafter(KrafterType.Service);
        serviceFactory.Generate(maps.ToDictionary(k => k.Key, v => v.Value.ToArray()), entityName, outputPath);

        var controllerFactory = KrafterFactory.CreateKrafter(KrafterType.Controller);
        controllerFactory.Generate(maps.ToDictionary(k => k.Key, v => v.Value.ToArray()), entityName, outputPath);
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

public class KrafterPropertyInfo
{
    public KrafterProperty Property { get; set; }
    public bool IncludeInInsert { get; set; }
    public bool IncludeInUpdate { get; set; }
    public bool IncludeInInput { get; set; }
    public bool IncludeInOutput { get; set; }
}