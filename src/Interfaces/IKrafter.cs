using Krafter.Types;

namespace Krafter.Interfaces;

public interface IKrafter
{
    void Generate(List<KrafterProperty> properties, string entity, string outputPath);
}