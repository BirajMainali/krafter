using System.Reflection;
using Krafter.Factory;
using Krafter.Types;

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
            var entityType = GetEntityType(entityName);
            Console.WriteLine("Generating source code for entity: " + entityName);

            var properties = entityType.GetProperties().ToList();
            properties.ForEach(PrintProperty);

            foreach (var propertyInfo in properties)
            {
                var krafterAttribute = propertyInfo.GetCustomAttribute<KrafterAttribute>();
                if (krafterAttribute is null) continue;
                AddPropertyToMap(maps, krafterAttribute.IncludeInInsert, propertyInfo, KrafterAttributeType.Insert);
                AddPropertyToMap(maps, krafterAttribute.IncludeInUpdate, propertyInfo, KrafterAttributeType.Update);
                AddPropertyToMap(maps, krafterAttribute.IncludeInInput, propertyInfo, KrafterAttributeType.Input);
                AddPropertyToMap(maps, krafterAttribute.IncludeInOutput, propertyInfo, KrafterAttributeType.Output);
            }

            GenerateSource(maps, entityName, "/output");
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

    private Type GetEntityType(string entityName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var entityType = assembly.GetType(entityName);
        if (entityType is null)
            throw new Exception("Krafter could not find the entity type in the assembly, Path: " + entityPath);

        return entityType;
    }

    private void AddPropertyToMap(Dictionary<KrafterAttributeType, List<KrafterProperty>> maps, bool include,
        PropertyInfo propertyInfo, KrafterAttributeType attributeType)
    {
        if (!include) return;

        Console.WriteLine($"Property {propertyInfo.Name} is included in {attributeType.ToString().ToLower()}");

        if (!maps.ContainsKey(attributeType)) maps[attributeType] = new List<KrafterProperty>();

        maps[attributeType].Add(new KrafterProperty(propertyInfo.PropertyType.Name, propertyInfo.Name));
    }

    private void GenerateSource(Dictionary<KrafterAttributeType, List<KrafterProperty>> maps, string entityName,
        string outputPath)
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

    private void PrintProperty(PropertyInfo propertyInfo)
    {
        Console.WriteLine($"Property Name: {propertyInfo.Name}");
        Console.WriteLine($"Property Type: {propertyInfo.PropertyType.Name}");
        Console.WriteLine($"Property Value: {propertyInfo.GetValue(propertyInfo)}");
    }
}