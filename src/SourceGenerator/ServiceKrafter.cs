using Krafter.Extensions;
using Krafter.Interfaces;

namespace Krafter.SourceGenerator;

public class ServiceKrafter : IKrafter
{
    private static readonly List<string> MethodNames = ["CreateAsync", "UpdateAsync", "DeleteAsync"];

    public void Generate(List<KrafterProperty> properties, string entity, string outputPath)
    {
        var interfaceContent = GenerateInterface(entity);
        var serviceContent = GenerateService(entity, properties);

        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }

        var interfacePath = Path.Combine(outputPath, $"{entity.GetInterfaceName()}.cs");
        var servicePath = Path.Combine(outputPath, $"{entity.GetServiceName()}.cs");

        File.WriteAllText(interfacePath, interfaceContent);
        File.WriteAllText(servicePath, serviceContent);

        Console.WriteLine("Service successfully generated");
    }


    private string GenerateInterface(string entity)
    {
        var classInstanceName = entity.ToInstanceName();

        var methodDeclarations = MethodNames.Select(method => method switch
            {
                "CreateAsync" => $"Task<{entity}> {method}({entity.GetDtoName()} dto);",
                "UpdateAsync" =>
                    $"Task<{entity}> {method}({entity} {classInstanceName}, {entity.GetDtoName()} dto);",
                "DeleteAsync" => $"Task {method}({entity} {classInstanceName});",
                _ => throw new Exception(
                    "Krafter cannot generate service interface method declaration for the provided method name => " +
                    method)
            })
            .ToList();

        return $$"""

                 using System.Threading.Tasks;

                 namespace Services
                 {
                     public interface {{entity.GetInterfaceName()}}
                     {
                         {{string.Join(Environment.NewLine, methodDeclarations)}}
                     }
                 }
                 """;
    }

    private string GenerateService(string entity, List<KrafterProperty> properties)
    {
        var classInstanceName = entity.ToInstanceName();
        var repoInstanceName = classInstanceName + "Repository";
        var uowInstanceName = "unitOfWorkManager";

        var methodImplementations = MethodNames.Select(methodName => methodName switch
            {
                "CreateAsync" => $$"""
                                       public async Task<{{entity}}> {{methodName}}({{entity.GetDtoName()}} dto)
                                       {
                                           using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                           {
                                               var entity = new {{entity}} 
                                               {
                                                   {{string.Join(", ", properties.Select(p => $"{p.Name} = dto.{p.Name}"))}}
                                               };
                                   
                                               await _unitOfWorkManager.AddAsync(entity);
                                               await _unitOfWorkManager.SaveChangesAsync();
                                               scope.Complete();
                                               return entity;
                                           }
                                       }
                                   """,
                "UpdateAsync" => $$"""
                                       public async Task<{{entity}}> {{methodName}}({{entity}} {{classInstanceName}}, {{entity.GetDtoName()}} dto)
                                       {
                                           using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                           {
                                               {{string.Join(Environment.NewLine, properties.Select(p => $"{classInstanceName}.{p.Name} = dto.{p.Name};"))}}
                                   
                                               await _unitOfWorkManager.UpdateAsync({{classInstanceName}});
                                               await _unitOfWorkManager.SaveChangesAsync();
                                               scope.Complete();
                                               return {{classInstanceName}};
                                           }
                                       }
                                   """,
                "DeleteAsync" => $$"""
                                       public async Task {{methodName}}({{entity}} {{classInstanceName}})
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
                    "Krafter cannot generate service method implementation for the provided method name => " +
                    methodName)
            })
            .ToList();

        return $$"""
                     using System.Threading.Tasks;
                     using System.Transactions;
                 
                     namespace Services
                     {
                         public class {{entity.GetServiceName()}} : {{entity.GetInterfaceName()}}
                         {
                             private readonly IRepository<{{entity}}> _{{repoInstanceName}};
                             private readonly IUnitOfWorkManager _{{uowInstanceName}};
                 
                             public {{entity.GetServiceName()}}(IRepository<{{entity}}> {{repoInstanceName}}, IUnitOfWorkManager {{uowInstanceName}})
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