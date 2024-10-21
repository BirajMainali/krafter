using Krafter.Extensions;
using Krafter.Interfaces;
using Krafter.Types;

namespace Krafter.SourceGenerator;

public class ServiceKrafter : IKrafter
{
    private static readonly List<string> MethodNames = ["CreateAsync", "UpdateAsync", "DeleteAsync"];


    public void Generate(Dictionary<KrafterAttributeType, KrafterProperty[]> properties, string identifier,
        string outputPath)
    {
        var className = string.Concat(identifier, "Service");
        var interfaceName = GetInterfaceName(className);

        var interfaceContent = GenerateInterface(interfaceName);
        var serviceContent = GenerateService(className, properties);

        var interfacePath = Path.Combine(outputPath, $"{interfaceName}.cs");
        var servicePath = Path.Combine(outputPath, $"{className}.cs");

        File.WriteAllText(interfacePath, interfaceContent);
        File.WriteAllText(servicePath, serviceContent);

        Console.WriteLine($"Generated {interfaceName} at {interfacePath}");
        Console.WriteLine($"Generated {className} at {servicePath}");
    }

    private string GetInterfaceName(string className)
    {
        return $"I{className}";
    }

    private string GenerateInterface(string interfaceName)
    {
        var methodDeclarations = new List<string>();
        var classInstanceName = interfaceName.Substring(1).ToLower();

        foreach (var method in MethodNames)
        {
            var declaration = method switch
            {
                "CreateAsync" => $"Task<{interfaceName}> {method}({string.Concat(interfaceName, "Dto")} dto)",
                "UpdateAsync" =>
                    $"Task<{interfaceName}> {method}({interfaceName} {classInstanceName}, {string.Concat(classInstanceName, "UpdateDto")} updateDto)",
                "DeleteAsync" => $"Task {method}({interfaceName} {classInstanceName})",
                _ => throw new Exception(
                    "Krafter cannot generate service interface method declaration for the provided method name => " +
                    method)
            };

            methodDeclarations.Add(declaration);
        }

        return $$"""

                 using System.Threading.Tasks;

                 namespace Services
                 {
                     public interface {{interfaceName}}
                     {
                         {{string.Join(Environment.NewLine, methodDeclarations)}}
                     }
                 }
                 """;
    }

    private string GenerateService(string className, Dictionary<KrafterAttributeType, KrafterProperty[]> properties)
    {
        var classInstanceName = className.Substring(0, 1).ToLower() + className.Substring(1);

        var insertProperties = properties.GetInsertProperties();
        var updateProperties = properties.GetUpdateProperties();

        var repoInstanceName = "_" + classInstanceName + "Repository";
        var uowInstanceName = "_unitOfWorkManager";

        var methodImplementations = MethodNames.Select(method => method switch
            {
                "CreateAsync" => $$"""
                                       public async Task<{{className}}> {{method}}({{string.Concat(className, "Dto")}} dto)
                                       {
                                           using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                           {
                                               var entity = new {{className}} 
                                               {
                                                   {{string.Join(", ", insertProperties.Select(p => $"{p.Name} = dto.{p.Name}"))}}
                                               };
                                   
                                               await {{repoInstanceName}}.AddAsync(entity);
                                               await {{uowInstanceName}}.SaveChangesAsync();
                                               await scope.CompleteAsync();
                                   
                                               return entity;
                                           }
                                       }
                                   """,
                "UpdateAsync" => $$"""
                                       public async Task<{{className}}> {{method}}({{className}} {{classInstanceName}}, {{string.Concat(className, "UpdateDto")}} updateDto)
                                       {
                                           using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                           {
                                               {{string.Join(Environment.NewLine, updateProperties.Select(p => $"{classInstanceName}.{p.Name} = updateDto.{p.Name};"))}}
                                   
                                               await {{repoInstanceName}}.UpdateAsync({{classInstanceName}});
                                               await {{uowInstanceName}}.SaveChangesAsync();
                                               await scope.CompleteAsync();
                                   
                                               return {{classInstanceName}};
                                           }
                                       }
                                   """,
                "DeleteAsync" => $$"""
                                       public async Task {{method}}({{className}} {{classInstanceName}})
                                       {
                                           using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                           {
                                               await {{repoInstanceName}}.DeleteAsync({{classInstanceName}});
                                               await {{uowInstanceName}}.SaveChangesAsync();
                                               await scope.CompleteAsync();
                                           }
                                       }
                                   """,
                _ => throw new Exception(
                    "Krafter cannot generate service method implementation for the provided method name => " + method)
            })
            .ToList();

        return $$"""
                     using System.Threading.Tasks;
                     using System.Transactions;
                 
                     namespace Services
                     {
                         public class {{className}} : {{GetInterfaceName(className)}}
                         {
                             private readonly {{repoInstanceName}} {{repoInstanceName}};
                             private readonly {{uowInstanceName}} {{uowInstanceName}};
                 
                             public {{className}}({{repoInstanceName}} {{repoInstanceName}}, {{repoInstanceName}} {{uowInstanceName}})
                             {
                                 {{repoInstanceName}} = {{repoInstanceName}};
                                 {{uowInstanceName}} = {{uowInstanceName}};
                             }
                 
                             {{string.Join(Environment.NewLine, methodImplementations)}}
                         }
                     }
                 """;
    }
}