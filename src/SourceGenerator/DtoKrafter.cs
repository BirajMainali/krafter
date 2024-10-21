using Krafter.Extensions;
using Krafter.Interfaces;

namespace Krafter.SourceGenerator;

public class DtoKrafter : IKrafter
{
    public void Generate(List<KrafterProperty> properties, string entity, string outputPath)
    {
        var types = new List<string>() { entity.GetVmName(), entity.GetDtoName() };

        foreach (var type in types)
        {
            var classProperties = properties.Select(p => $"public {p.Type} {p.Name} {{ get; set; }}");
            var classContent = string.Join(Environment.NewLine, classProperties);

            var classTemplate = $$"""

                                  using System;

                                  namespace Krafter.{{type}}
                                  {
                                      public class {{type}}Dto
                                      {
                                          {{classContent}}
                                      }
                                  }
                                  """;
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            var outputFilePath = Path.Combine(outputPath, $"{type}.cs");
            File.WriteAllText(outputFilePath, classTemplate);
            Console.WriteLine($"Generated {outputFilePath}");
        }
    }
}