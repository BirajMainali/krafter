using Krafter.Extensions;
using Krafter.Interfaces;

namespace Krafter.SourceGenerator;

public class ControllerKrafter : IKrafter
{
    private static readonly string[] SupportedMethod = { "Create", "Update", "Delete" };

    public void Generate(List<KrafterProperty> properties, string entity, string outputPath)
    {
        var interfaceName = $"I{entity}Service";
        var serviceName = $"{entity}Service";

        var controllerMethods = new List<string>();

        foreach (var method in SupportedMethod)
        {
            var methodImplementation = method switch
            {
                "Index" => $$"""
                              public async Task<IActionResult> Index()
                              {
                                    return View();
                              }
                             """,

                "Create" => $$"""
                              public async Task<IActionResult> Create()
                              {
                                  var vm = new {{entity.GetVmName()}}(); 
                                  return View(vm);
                              }

                              [HttpPost]
                              public async Task<IActionResult> Create({{entity.GetVmName()}} vm)
                              {
                                  if (!ModelState.IsValid)
                                  {
                                      return View(vm); 
                                  }
                              
                                  try
                                  {
                                      var dto = new {{entity.GetDtoName()}}
                                      {
                                          {{ToDto(properties)}}
                                      };
                              
                                      var createdEntity = await _{{serviceName}}.CreateAsync(dto);
                                      return RedirectToAction(nameof(Index));
                                  }
                                  catch (Exception ex)
                                  {
                                      _logger.LogError(ex, "An error occurred while creating {{entity}}");
                                      return RedirectToAction(nameof(Index)); // Optional: return to Create with error message
                                  }
                              }
                              """,

                "Update" => $$"""
                              public async Task<IActionResult> Update(int id)
                              {
                                  var dto = await _{{serviceName}}.GetByIdAsync(id);
                                  if (dto == null)
                                  {
                                      _logger.LogWarning("Entity with ID {id} not found for Update", id);
                                      return RedirectToAction(nameof(Index));
                                  }
                              
                                  var vm = new {{entity.GetVmName()}}
                                  {
                                      {{ToVm(properties)}}
                                  };
                              
                                  return View(vm);
                              }

                              [HttpPost]
                              public async Task<IActionResult> Update(int id, {{entity.GetVmName()}} vm)
                              {
                                  if (id != vm.Id || !ModelState.IsValid)
                                  {
                                      _logger.LogWarning("Invalid model state or ID mismatch for Update method");
                                      return View(vm); 
                                  }
                              
                                  try
                                  {
                                      var dto = new {{entity.GetDtoName()}}
                                      {
                                          {{ToDto(properties)}}
                                      };
                              
                                      var updatedEntity = await _{{serviceName}}.UpdateAsync(id, dto);
                                      return RedirectToAction(nameof(Index));
                                  }
                                  catch (Exception ex)
                                  {
                                      _logger.LogError(ex, "An error occurred while updating {{entity}} with ID {id}", id);
                                      return RedirectToAction(nameof(Index));
                                  }
                              }
                              """,

                "Delete" => $$"""
                              [HttpPost]
                              public async Task<IActionResult> Delete(int id)
                              {
                                  try
                                  {
                                      await _{{serviceName}}.DeleteAsync(id);
                                      return RedirectToAction(nameof(Index));
                                  }
                                  catch (Exception ex)
                                  {
                                      _logger.LogError(ex, "An error occurred while deleting {{entity}} with ID {id}", id);
                                      return RedirectToAction(nameof(Index));
                                  }
                              }
                              """,

                _ => throw new NotImplementedException($"Method {method} not implemented.")
            };

            controllerMethods.Add(methodImplementation);
        }

        var controllerClass = $$"""
                                    using Microsoft.AspNetCore.Mvc;
                                    using System.Threading.Tasks;
                                    using Microsoft.Extensions.Logging;
                                
                                    namespace Controllers
                                    {
                                        public class {{entity.GetControllerName()}} : Controller
                                        {
                                            private readonly {{interfaceName}} _{{serviceName}};
                                            private readonly ILogger<{{entity}}Controller> _logger;
                                
                                            public {{entity.GetControllerName()}}({{interfaceName}} {{serviceName}}, ILogger<{{entity.GetControllerName()}}> logger)
                                            {
                                                _{{serviceName}} = {{serviceName}};
                                                _logger = logger;
                                            }
                                
                                            {{string.Join(Environment.NewLine, controllerMethods)}}
                                        }
                                    }
                                """;

        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }

        var controllerPath = Path.Combine(outputPath, $"{entity}Controller.cs");
        File.WriteAllText(controllerPath, controllerClass);

        Console.WriteLine($"Generated {entity}Controller at {controllerPath}");
    }

    private string ToDto(List<KrafterProperty> properties)
    {
        return string.Join(Environment.NewLine, properties.Select(p => $"{p.Name} = vm.{p.Name},"));
    }

    private string ToVm(List<KrafterProperty> properties)
    {
        return string.Join(Environment.NewLine, properties.Select(p => $"{p.Name} = dto.{p.Name},"));
    }
}