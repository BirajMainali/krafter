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
        var interfaceContent = GenerateInterface(identifier);
        var serviceContent = GenerateService(identifier, properties);

        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }

        var interfacePath = Path.Combine(outputPath, $"{GetInterfaceName(identifier)}.cs");
        var servicePath = Path.Combine(outputPath, $"{GetClassName(identifier)}.cs");

        File.WriteAllText(interfacePath, interfaceContent);
        File.WriteAllText(servicePath, serviceContent);

        Console.WriteLine("Service successfully generated");
    }

    private static string GetClassName(string identifier)
    {
        return string.Concat(identifier, "Service");
    }

    private string GetInterfaceName(string identifier)
    {
        return string.Concat("I", identifier, "Service");
    }

    private string GenerateInterface(string identifier)
    {
        var classInstanceName = identifier;

        var methodDeclarations = MethodNames.Select(method => method switch
            {
                "CreateAsync" => $"Task<{identifier}> {method}({string.Concat(identifier, "Dto")} dto);",
                "UpdateAsync" =>
                    $"Task<{identifier}> {method}({identifier} {classInstanceName}, {string.Concat(classInstanceName, "UpdateDto")} updateDto);",
                "DeleteAsync" => $"Task {method}({identifier} {classInstanceName});",
                _ => throw new Exception(
                    "Krafter cannot generate service interface method declaration for the provided method name => " +
                    method)
            })
            .ToList();

        return $$"""

                 using System.Threading.Tasks;

                 namespace Services
                 {
                     public interface {{GetInterfaceName(identifier)}}
                     {
                         {{string.Join(Environment.NewLine, methodDeclarations)}}
                     }
                 }
                 """;
    }

    private string GenerateService(string entity, Dictionary<KrafterAttributeType, KrafterProperty[]> properties)
    {
        var classInstanceName = entity.FirstLetterToLower();

        var insertProperties = properties.GetInsertProperties();
        var updateProperties = properties.GetUpdateProperties();

        var repoInstanceName = classInstanceName + "Repository";
        var uowInstanceName = "unitOfWorkManager";

        var methodImplementations = MethodNames.Select(method => method switch
            {
                "CreateAsync" => $$"""
                                       public async Task<{{entity}}> {{method}}({{string.Concat(entity, "Dto")}} dto)
                                       {
                                           using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                           {
                                               var entity = new {{entity}} 
                                               {
                                                   {{string.Join(", ", insertProperties.Select(p => $"{p.Name} = dto.{p.Name}"))}}
                                               };
                                   
                                               await _unitOfWorkManager.AddAsync(entity);
                                               await _unitOfWorkManager.SaveChangesAsync();
                                               scope.Complete();
                                               return entity;
                                           }
                                       }
                                   """,
                "UpdateAsync" => $$"""
                                       public async Task<{{entity}}> {{method}}({{entity}} {{classInstanceName}}, {{string.Concat(entity, "UpdateDto")}} updateDto)
                                       {
                                           using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                           {
                                               {{string.Join(Environment.NewLine, updateProperties.Select(p => $"{classInstanceName}.{p.Name} = updateDto.{p.Name};"))}}
                                   
                                               await _unitOfWorkManager.UpdateAsync({{classInstanceName}});
                                               await _unitOfWorkManager.SaveChangesAsync();
                                               scope.Complete();
                                               return {{classInstanceName}};
                                           }
                                       }
                                   """,
                "DeleteAsync" => $$"""
                                       public async Task {{method}}({{entity}} {{classInstanceName}})
                                       {
                                           using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                           {
                                               await _unitOfWorkManager.DeleteAsync({{classInstanceName}});
                                               await _unitOfWorkManager.SaveChangesAsync();
                                               scope.Complete();
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
                         public class {{GetClassName(entity)}} : {{GetInterfaceName(entity)}}
                         {
                             private readonly IRepository<{{entity}}> _{{repoInstanceName}};
                             private readonly IUnitOfWorkManager _{{uowInstanceName}};
                 
                             public {{GetClassName(entity)}}(IRepository<{{entity}}> {{repoInstanceName}}, IUnitOfWorkManager {{uowInstanceName}})
                             {
                                _{{repoInstanceName}} = {{repoInstanceName}};
                                _{{uowInstanceName}} = {{uowInstanceName}};
                             }
                 
                             {{string.Join(Environment.NewLine, methodImplementations)}}
                         }
                     }
                 """;
    }
}