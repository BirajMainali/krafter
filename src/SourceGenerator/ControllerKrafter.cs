using Krafter.Interfaces;
using Krafter.Types;

namespace Krafter.SourceGenerator;

public class ControllerKrafter : IKrafter
{
    private static readonly string[] SupportedMethod = { "Create", "Update", "Delete", "GetById", "GetAll" };

    public void Generate(Dictionary<KrafterAttributeType, KrafterProperty[]> properties, string identifier,
        string outputPath)
    {
        var className = identifier;
        var interfaceName = $"I{className}Service";
        var serviceName = $"{className}Service";

        var controllerMethods = new List<string>();

        foreach (var method in SupportedMethod)
        {
            var methodImplementation = method switch
            {
                "Create" => $$"""
                                  [HttpPost]
                                  public async Task<IActionResult> Create([FromBody] {{className}}Dto dto)
                                  {
                                      if (!ModelState.IsValid)
                                      {
                                        return BadRequest(ModelState);
                                      }
                              
                                      var createdEntity = await _{{serviceName}}.CreateAsync(dto);
                                      return CreatedAtAction(nameof(GetById), new { id = createdEntity.Id }, createdEntity);
                                  }
                              """,
                "Update" => $$"""
                                  [HttpPut("{id}")]
                                  public async Task<IActionResult> Update(int id, [FromBody] {{className}}UpdateDto updateDto)
                                  {
                                      if (id != updateDto.Id || !ModelState.IsValid)
                                        {
                                            return BadRequest(ModelState);
                                        }
                              
                                      var updatedEntity = await _{{serviceName}}.UpdateAsync(id, updateDto);
                                      return Ok(updatedEntity);
                                  }
                              """,
                "Delete" => $$"""
                                  [HttpDelete("{id}")]
                                  public async Task<IActionResult> Delete(int id)
                                  {
                                      await _{{serviceName}}.DeleteAsync(id);
                                      {
                                        return NoContent();
                                      }
                                  }
                              """,
                "GetById" => $$"""
                                   [HttpGet("{id}")]
                                   public async Task<IActionResult> GetById(int id)
                                   {
                                       var entity = await _{{serviceName}}.GetByIdAsync(id);
                                       if (entity == null)
                                       {
                                        return BadRequest(ModelState);
                                       }
                               
                                       return Ok(entity);
                                   }
                               """,
                "GetAll" => $$"""
                                  [HttpGet]
                                  public async Task<IActionResult> GetAll()
                                  {
                                      var entities = await _{{serviceName}}.GetAllAsync();
                                      return Ok(entities);
                                  }
                              """,
                _ => throw new NotImplementedException($"Method {method} not implemented.")
            };

            controllerMethods.Add(methodImplementation);
        }

        var controllerClass = $$"""
                                    using Microsoft.AspNetCore.Mvc;
                                    using System.Collections.Generic;
                                    using System.Threading.Tasks;
                                
                                    namespace Controllers
                                    {
                                        [Route("[controller]")]
                                        [ApiController]
                                        public class {{className}}Controller : ControllerBase
                                        {
                                            private readonly {{interfaceName}} _{{serviceName}};
                                
                                            public {{className}}Controller({{interfaceName}} {{serviceName}})
                                            {
                                                _{{serviceName}} = {{serviceName}};
                                            }
                                
                                            {{string.Join(Environment.NewLine, controllerMethods)}}
                                        }
                                    }
                                """;

        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }

        var controllerPath = Path.Combine(outputPath, $"{className}Controller.cs");
        File.WriteAllText(controllerPath, controllerClass);

        Console.WriteLine($"Generated {className}Controller at {controllerPath}");
    }
}