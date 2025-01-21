using Api.Configuration;
using Api.Dtos.Dependent;
using Api.Dtos.Paycheck;
using Api.Interfaces;
using Api.Interfaces.Repositories;
using Api.Interfaces.Services;
using Api.Models;
using Api.Utilities;

using AutoMapper;

namespace Api.Services
{
    public class PaycheckService : IPaycheckService
    {
        private const int MONTHS_IN_YEAR = 12;
        private const int DAYS_IN_YEAR = 365;

        /*
        * I chose to manage all requirement calc values in appsettings.json to avoid magic numbers, and allow them to be changed easily
        * An alternative: we could instead have a 'DeductionsController' perform CRUD operations on these values so that buisness users can change them through a UI
        * With proper validation on both frontend and backend. Then query the DB for the latest values and use those
        * 
        * With the current appsettings.json setup, devs would have to own the requested value changes by changing env variables in something like an Octopus Deploy, 
        * this also allows values to be environment specific
        */

        private readonly IConfiguration _config;
        private readonly IPaycheckRepository _paycheckRepository;
        private readonly IEmployeeService _employeeService;
        private readonly IMapper _mapper;
        private readonly PaycheckOptions paycheckOptions;
        private readonly int daysInPayPeriod;
        

        public PaycheckService(IConfiguration config, IPaycheckRepository paycheckRepository, IEmployeeService employeeService, IMapper mapper)
        {
            _config = config;
            paycheckOptions = _config.GetSection(PaycheckOptions.PaycheckConfig).Get<PaycheckOptions>()
                ?? throw new Exception("Missing a 'PaycheckConfig' object in appsettings.json, see PaycheckOptions.cs for the schema.");

            // Force config to set paychecks per year in valid configurations found in PayFrequency.cs enum, throw exception if num not in enum
            if (!Enum.IsDefined(typeof(PayFrequency), paycheckOptions.PaychecksPerYear))
            {
                var validValues = string.Join(", ", Enum.GetValues(typeof(PayFrequency))
                    .Cast<PayFrequency>()
                    .Select(v => (int)v));
                throw new ArgumentException($"Invalid value for 'PaychecksPerYear'. Valid values are: {validValues}.");
            }

            // Integer division always rounds down, fine for what we need here
            daysInPayPeriod = DAYS_IN_YEAR / paycheckOptions.PaychecksPerYear;

            _paycheckRepository = paycheckRepository;
            _employeeService = employeeService;
            _mapper = mapper;
        }

        /// <summary>
        /// Creates the paycheck for the last pay period and stores in in the database.
        /// </summary>
        /// <returns></returns>
        public async Task<ApiResponse<AddPaycheckDto>> Create(int employeeId)
        {
            var result = new ApiResponse<AddPaycheckDto>();
            var employeeDto = await _employeeService.Get(employeeId);
            var employeeModel = _mapper.Map<Employee>(employeeDto);

            int daysPerPaycheck = DAYS_IN_YEAR / paycheckOptions.PaychecksPerYear;

            var periodEnd = DateTime.Today;
            var periodStart = periodEnd.AddDays(-daysPerPaycheck);

            var grossPay = CalcGrossPay(employeeModel.Salary);
            var deductionSummary = CreateDeductionSummary(employeeModel);
            

            var netPay = employeeModel.Salary - deductionSummary.TotalDeduction;

            var paycheck = new Paycheck
            {
                Employee = employeeModel, // EF converts this into foreign key when saving in db
                PeriodStart = periodStart,
                PeriodEnd = periodEnd,
                GrossPay = grossPay,
                NetPay = netPay,
                DeductionSummary = deductionSummary
            };

            try
            {
                var paycheckModel = await _paycheckRepository.Add(paycheck);
                var addPaycheckDto = _mapper.Map<AddPaycheckDto>(paycheckModel);
                return ApiResponseUtil.CreateResponse(true, addPaycheckDto, "Paycheck created successfully");
            }
            catch (Exception ex)
            {
                // log exception
                // bubble up user friendly response to display api caller
                result = ApiResponseUtil.CreateResponse<AddPaycheckDto>(false, null, "Sorry, something went wrong.", "ERROR-KEY");
                return result;
            }
        }

        public Task<ApiResponse<GetPaycheckDto>> Get(int paycheckId)
        {
            throw new NotImplementedException();
        }

        public Task<ApiResponse<ICollection<GetPaycheckDto>>> GetPaychecksByEmployee(int employeeId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Calculate gross pay (pay before deductions) for the paycheck
        /// Factors in an 365th and 366th day if leap year and add those days pay to the last pay period
        /// </summary>
        private decimal CalcGrossPay(decimal salary)
        {
            var grossPay = salary / paycheckOptions.PaychecksPerYear;

            // I use today, because for my design, I assume the GeneratePaycheck event is only ran on the end date of every period
            var today = DateTime.Now;

            // Add days in pay period to today, if the date is in next year, we know this is the last paycheck of the year
            if (today.AddDays(daysInPayPeriod).Year > today.Year)
            {
                /*
                 * On the last paycheck of the year, we need to add an extra day's pay to account for the 365th day, 
                 * since dividing by 365 days creates fractions that round out that last day. So we add it back here. 
                 * If it's a leap year, we also add another day's pay for the 366th day.
                 */

                // Calcualte the gross Pay per day relative to the pay period's length
                var payPerDay = grossPay / daysInPayPeriod;

                // Add one more day of pay for period
                grossPay += payPerDay;

                if (DateTime.IsLeapYear(today.Year))
                {
                    grossPay += payPerDay;
                }
            }

            return grossPay;
        }

        /// <summary>
        /// Calculate the deductions for the current paycheck and place in a <see cref="DeductionSummary"/> object.
        /// </summary>
        /// <param name="employee">The employee whose deductions are being calculated</param>
        private DeductionSummary CreateDeductionSummary(Employee employee)
        {
            /*
             * Calculate the base deduction for this period
             * Since deductions are by month, divide them out to be even across paychecks
             */
            var baseDeduction = (paycheckOptions.MonthlyBaseCost * MONTHS_IN_YEAR) / paycheckOptions.PaychecksPerYear;

            // Calculate total dependent cost / deduction
            var dependentDeduction = CalcDependentCost(employee.Dependents);

            // Calcualte Percent of salary added as extra cost if they meet or are above salary threshold
            var salarySurchargeDeduction = 0m;
            if (employee.Salary >= paycheckOptions.AnnualSurchargeSalaryThreshold)
            {
                var annualSurchargeAsDecimal = paycheckOptions.AnnualSurchargePercent / 100;
                salarySurchargeDeduction = employee.Salary * annualSurchargeAsDecimal;
            }

            var today = DateTime.Now;

            // Add days in pay period to today, if the date is in next year, we know this is the last paycheck of the year
            if (today.AddDays(daysInPayPeriod).Year > today.Year)
            {
                /*
                * On the last paycheck of the year, we need to add an extra day's deduction costs to account for the 365th day, 
                * since dividing by 365 days creates fractions that round out that last day. So we add it back here. 
                * If it's a leap year, we also add another day's deduction costs for the 366th day.
                */

                var baseDeductionPerDay = baseDeduction / daysInPayPeriod;
                var dependentDeductionPerDay = dependentDeduction / daysInPayPeriod;
                var salarySurchargeDeductionPerDay = dependentDeduction / daysInPayPeriod;

                // Add one more day of deductions for the 365th day
                baseDeduction += baseDeductionPerDay;
                dependentDeduction += dependentDeductionPerDay;
                salarySurchargeDeduction += salarySurchargeDeductionPerDay;

                // If leap add another day's worth of deductions
                if (DateTime.IsLeapYear(today.Year))
                {
                    baseDeduction += baseDeductionPerDay;
                    dependentDeduction += dependentDeductionPerDay;
                    salarySurchargeDeduction += salarySurchargeDeductionPerDay;
                }

                // note, just thought of this while writing test,  what if the employee gets paid annually, current implementation would give them free money :)
            }

            var totalDeduction = baseDeduction + dependentDeduction + salarySurchargeDeduction;

            DeductionSummary deductions = new()
            {
                BaseDeduction = baseDeduction,
                DependentDeduction = dependentDeduction,
                SalarySurchargeDeduction = salarySurchargeDeduction,
                TotalDeduction = totalDeduction
            };

            return deductions;
        }

        /// <summary>
        /// Evaluates all the dependents of an employee and calculates the total dependent cost
        /// </summary>
        private decimal CalcDependentCost(ICollection<Dependent> dependents)
        {
            decimal totalDependentCost = 0m;

            // Take the monthly costs for dependents, and divide them by paychecks per year to get the most even split
            var costPerPaycheckBase = (paycheckOptions.MonthlyBaseDependentCost * MONTHS_IN_YEAR) / paycheckOptions.PaychecksPerYear;
            var costPerPaycheckSurcharge = (paycheckOptions.MonthlyDependentSurchargeAgeCost * MONTHS_IN_YEAR) / paycheckOptions.PaychecksPerYear;


            if (dependents.Any())
            {
                // get the periods base dependent cost and multiply by current # of dependents
                totalDependentCost += costPerPaycheckBase * dependents.Count;
                
                // calculate the additonal surcharge for each dependent at or over the age threshold 
                var dependentsOverAgeThresholdCount = dependents.Where(d => CalculationUtil.CalculateAge(d.DateOfBirth) >= paycheckOptions.DependentSurchargeAgeThreshold).Count();
                if (dependentsOverAgeThresholdCount > 0)
                {
                    totalDependentCost += costPerPaycheckSurcharge * dependentsOverAgeThresholdCount;
                }
            }

            return totalDependentCost; 
        }
    }
}
