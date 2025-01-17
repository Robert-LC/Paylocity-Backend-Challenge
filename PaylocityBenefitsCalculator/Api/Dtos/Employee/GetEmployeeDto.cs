using Api.Dtos.Dependent;
using Api.Dtos.Paycheck;

namespace Api.Dtos.Employee;

public class GetEmployeeDto
{
    public int Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public decimal Salary { get; set; }
    public DateTime DateOfBirth { get; set; }
    public ICollection<GetDependentDto> Dependents { get; set; } = new List<GetDependentDto>();
    public ICollection<GetPaycheckDto> Paychecks { get; set; } = new List<GetPaycheckDto>();
}
