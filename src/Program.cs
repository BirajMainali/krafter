using System.Text.Json.Serialization;

namespace Krafter;

public class Program
{
    private static void Main(string[] args)
    {
        try
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Please provide the entity path as an argument.");
                return;
            }

            var entityPath = args[0];

            var krafter = new Krafter(entityPath);
            krafter.EntityContextGenerator();
        }
        catch (Exception e)
        {
            Console.WriteLine("An error occurred while starting Krafter: " + e.Message);
        }
    }
}