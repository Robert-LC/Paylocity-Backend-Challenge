using Api.Dtos.Dependent;
using Api.Interfaces.Services;
using Api.Models;
using Api.Utilities;

using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class DependentsController : ControllerBase
{

    private readonly IEmployeeService _employeeService;
    private readonly IDependentService _dependentService;
    public DependentsController(IEmployeeService employeeService, IDependentService dependentService)
    {
        _employeeService = employeeService;
        _dependentService = dependentService;
    }

    [SwaggerOperation(Summary = "Add a dependent to an employee")]
    [HttpPost("")]
    public async Task<ActionResult<ApiResponse<AddDependentDto>>> Create([FromBody] AddDependentDto dependent, int employeeId)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(
                ApiResponseUtil.CreateResponse<AddDependentDto>
                (false, null, "Add validation attributes to model, put errors here", "INFO-KEY")
            );
        }

        var employee = await _employeeService.Get(employeeId);

        if (employee == null)
        {
            return NotFound(
                ApiResponseUtil.CreateResponse<AddDependentDto>
                (false, null, "The employee in which you are trying to add a dependent does not exist.", "INFO-KEY")
            );
        }

        var dependentAddResult =  await _dependentService.Add(dependent, employeeId);
        if (dependentAddResult.Success)
        {
            return Ok(dependentAddResult);
        }
        else
        {
            // Bubble up friendly 500 error to caller for server side errors.
            return StatusCode(500, dependentAddResult);
        }
    }

    [SwaggerOperation(Summary = "Get dependent by id")]
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<GetDependentDto>>> Get(int id)
    {
        throw new NotImplementedException();
    }

    [SwaggerOperation(Summary = "Get all dependents")]
    [HttpGet("")]
    public async Task<ActionResult<ApiResponse<List<GetDependentDto>>>> GetAll()
    {
        var dependents = await _dependentService.GetAll();

        if (dependents.Data == null || dependents.Data.Count == 0)
        {
            return Ok(
                ApiResponseUtil.CreateResponse
                (true, new List<GetDependentDto>(), "No dependents found")
            );
        }

        return Ok(
            ApiResponseUtil.CreateResponse
            (true, dependents.Data.ToList(), $"{dependents.Data.Count} dependents returned")
        );
    }
}
