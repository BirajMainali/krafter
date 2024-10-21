namespace Krafter.Extensions;

public static class StringExtensions
{
    public static string ToInstanceName(this string str)
    {
        return string.IsNullOrEmpty(str) || char.IsLower(str[0]) ? str : char.ToLower(str[0]) + str.Substring(1);
    }

    public static string GetVmName(this string str)
    {
        return string.Concat(str, "Vm");
    }

    public static string GetDtoName(this string str)
    {
        return string.Concat(str, "Dto");
    }

    public static string GetServiceName(this string str)
    {
        return string.Concat(str, "Service");
    }

    public static string GetInterfaceName(this string str)
    {
        return string.Concat("I", str.GetServiceName());
    }

    public static string GetControllerName(this string str)
    {
        return string.Concat(str, "Controller");
    }
    
}