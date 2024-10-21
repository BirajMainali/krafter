using Krafter.Types;

namespace Krafter.Extensions;

public static class KrafterContextExtensions
{
    public static string GetClassPrefix(this KrafterAttributeType key)
    {
        return key switch
        {
            KrafterAttributeType.Insert or KrafterAttributeType.Update => "Dto",
            KrafterAttributeType.Input or KrafterAttributeType.Output => "Vm",
            _ => throw new Exception("The provided krafter attribute implementation is not supported as of now")
        };
    }
    public static KrafterProperty[] GetInsertProperties(
        this Dictionary<KrafterAttributeType, KrafterProperty[]> properties)
    {
        return properties.Where(p => p.Key.Equals(KrafterAttributeType.Insert))
            .SelectMany(p => p.Value).Select(p => p).ToArray();
    }

    public static KrafterProperty[] GetUpdateProperties(
        this Dictionary<KrafterAttributeType, KrafterProperty[]> properties)
    {
        return properties.Where(p => p.Key.Equals(KrafterAttributeType.Update))
            .SelectMany(p => p.Value).Select(p => p).ToArray();
    }

    public static KrafterProperty[] GetInputProperties(
        this Dictionary<KrafterAttributeType, KrafterProperty[]> properties)
    {
        return properties.Where(p => p.Key.Equals(KrafterAttributeType.Input))
            .SelectMany(p => p.Value).Select(p => p).ToArray();
    }

    public static KrafterProperty[] GetOutputProperties(Dictionary<string, KrafterProperty[]> properties)
    {
        return properties.Where(p => p.Key.Contains(KrafterAttributeType.Output.ToString()))
            .SelectMany(p => p.Value).Select(p => p).ToArray();
    }
}