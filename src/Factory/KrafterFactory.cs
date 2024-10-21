using Krafter.Interfaces;
using Krafter.SourceGenerator;

namespace Krafter.Factory;

public static class KrafterFactory
{
    public static IKrafter CreateKrafter(KrafterType krafterType)
    {
        return krafterType switch
        {
            KrafterType.Controller => new ControllerKrafter(),
            KrafterType.Service => new ServiceKrafter(),
            KrafterType.Dto => new DtoKrafter(),
            _ => throw new Exception(
                "Invalid krafter type could produce requested krafter object from krafter factory.")
        };
    }
}