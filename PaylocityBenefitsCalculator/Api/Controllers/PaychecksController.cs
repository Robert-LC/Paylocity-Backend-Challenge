using Api.Dtos.Paycheck;
using Api.Interfaces.Services;
using Api.Models;
using Api.Utilities;

using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Api.Controllers
{
    public class PaychecksController : ControllerBase
    {

        private readonly IPaycheckService _paycheckService;
        private readonly IEmployeeService _employeeService;

        public PaychecksController(IPaycheckService paycheckService, IEmployeeService employeeService)
        {
            _paycheckService = paycheckService;
            _employeeService = employeeService;
        }

        [SwaggerOperation(Summary = "Generate this pay period's check")]
        [HttpPost("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> GeneratePaycheck(int employeeId)
        {
            /*
             * My idea is that this endpoint isn't hit by the UI client, 
             * instead an automatic process/integration hits this endpoint and generates checks on the last day of the pay period
             * Then when the user views their current or previous checks in UI, its already all stored in the DB and the UI only hits the Get() endpoints to view them
             * 
             * On GeneratePaycheck day if there is a lot of employees the system might get overloaded, we would have to implement some type of queueing and batching, even though method is async 
             * and multiple paychecks can be generated simulatenously we want to limit total strain on system/db
             * 
             * We could add a scope/or auth attribute to this method where UI can't access this endpoint, but an integration/event client can.
             * 
             * 
             */

            var employee = await _employeeService.Get(employeeId);

            if (employee == null)
            {
                return NotFound(
                    ApiResponseUtil.CreateResponse<object>
                    (false, null, "Employee not found", "INFO-KEY")
                );
            }

            var paycheckCreateResult = await _paycheckService.Create(employeeId);
            paycheckCreateResult.Data = null;

            if (paycheckCreateResult.Success)
            {
                /*
                 * I made return type object, and set its data property to null, but its Message property is filled with something
                 * I did that because I assume the integration/event client should not need to see personal/private paycheck values
                 * it only needs to know if creation was successful or not
                 */
                
                return Ok(paycheckCreateResult); 
            }
            else
            {
                return StatusCode(500, paycheckCreateResult);
            }
            
        }

        [SwaggerOperation(Summary = "Get paycheck by id")]
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<GetPaycheckDto>>> Get(int id)
        {
            throw new NotImplementedException();
        }

        [SwaggerOperation(Summary = "Get all paychecks")]
        [HttpGet("")]
        public async Task<ActionResult<ApiResponse<GetPaycheckDto>>> GetAll()
        {
            throw new NotImplementedException();
        }

        [SwaggerOperation(Summary = "Get all paychecks tied to an Employee")]
        [HttpGet("employee/{employeeId}")]
        public async Task<ActionResult<ApiResponse<ICollection<GetPaycheckDto>>>> GetByEmployee(int employeeId)
        {
            throw new NotImplementedException();
        }
    }
}
