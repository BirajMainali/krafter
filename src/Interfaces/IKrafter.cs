using Krafter.Types;

namespace Krafter.Interfaces;

public interface IKrafter
{
    void Generate(Dictionary<KrafterAttributeType, KrafterProperty[]> properties, string identifier, string outputPath);
}