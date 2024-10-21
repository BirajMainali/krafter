using Krafter.Extensions;
using Krafter.Interfaces;
using Krafter.Types;

namespace Krafter.SourceGenerator;

public class DtoKrafter : IKrafter
{
    public void Generate(Dictionary<KrafterAttributeType, KrafterProperty[]> properties, string identifier,
        string outputPath)
    {
        foreach (var propertiesMap in properties)
        {
            if (!propertiesMap.Value.Any()) continue;
            var className = string.Concat(identifier, propertiesMap.Key.GetClassPrefix());
            var classProperties = propertiesMap.Value.Select(p => $"public {p.Type} {p.Name} {{ get; set; }}");
            var classContent = string.Join(Environment.NewLine, classProperties);


            var classTemplate = $$"""

                                  using System;

                                  namespace Krafter.{{propertiesMap.Key}}
                                  {
                                      public class {{className}}
                                      {
                                          {{classContent}}
                                      }
                                  }

                                  """;
            var outputFilePath = Path.Combine(outputPath, $"{className}.cs");
            File.WriteAllText(outputFilePath, classTemplate);
            Console.WriteLine($"Generated {outputFilePath}");
        }
    }
}