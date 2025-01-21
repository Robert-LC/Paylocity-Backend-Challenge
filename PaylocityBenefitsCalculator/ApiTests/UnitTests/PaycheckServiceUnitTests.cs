using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Api.Configuration;
using Api.Dtos.Dependent;
using Api.Dtos.Employee;
using Api.Interfaces.Repositories;
using Api.Interfaces.Services;
using Api.Models;
using Api.Services;

using AutoMapper;

using Microsoft.Extensions.Configuration;

using Moq;

using Xunit;

namespace ApiTests.UnitTests
{
    /// <summary>
    /// Test coverage for cost calculations
    /// </summary>
    public class PaycheckServiceUnitTests
    {
        private readonly Mock<IConfiguration> _mockConfig;
        private readonly Mock<IPaycheckRepository> _mockPaycheckRepository;
        private readonly Mock<IEmployeeService> _mockEmployeeService;
        private readonly Mock<IMapper> _mockMapper;
        private readonly PaycheckService _paycheckService;

        // work around since mocking IConfig wasnt working
        public const int PaychecksPerYear = 26;
        public const decimal MonthlyBaseCost = 1000m;
        public const decimal MonthlyBaseDependentCost = 600m;
        public const decimal AnnualSurchargeSalaryThreshold = 80000m;
        public const int AnnualSurchargePercent = 2;
        public const int DependentSurchargeAgeThreshold = 50;
        public const decimal MonthlyDependentSurchargeAgeCost = 200m;

        const int MONTHS_IN_YEAR = 12;

        public PaycheckServiceUnitTests()
        {
            _mockConfig = new Mock<IConfiguration>();
            _mockPaycheckRepository = new Mock<IPaycheckRepository>();
            _mockEmployeeService = new Mock<IEmployeeService>();
            _mockMapper = new Mock<IMapper>();


            _paycheckService = new PaycheckService(
                _mockConfig.Object,
                _mockPaycheckRepository.Object,
                _mockEmployeeService.Object,
                _mockMapper.Object
            );
        }

        /*
         * Discuss how this test revealed some problems with using DateTime.Now.Today in my method, and how it makes testing harder (didnt implement fix)
         * makes hard to test edge case of last pay period of year, and leap year, good learning experience on complex reqs
         * 
         * For this test I just tested the base calculation of any pay period besides the last, the way this code is right now
         * this test will pass anytime except if u run this on a date that would be the last pay period of the year
         */


        [Theory]
        [InlineData(50000.00, 0, 0, false)] // No salary surcharge, no dependents, no Leap Year
        [InlineData(65650.25, 2, 0, false)] // No Salary surcharge, 2 child dependents, No Leap Year
        [InlineData(76500.76, 1, 1, false)] // No salary surcharge, 1 child dependent, 1 dependent over age threshold, No Leap Year
        [InlineData(92365.00, 0, 0, false)] // Salary surcharge cost, no dependents, No Leap year
        [InlineData(92365.00, 1, 0, false)] // Salary surcharge cost, 1 child dependent, No Leap year
        [InlineData(99893.00, 3, 1, false)] // Salary surcharge cost, 3 child dependents, 1 dependent over age threshold, No Leap year
        [InlineData(99893.00, 1, 1, false)] // Salary surcharge cost, 1 child dependents, 2 dependent over age threshold, No Leap year
        public async Task Create_VerifyCalculations_DifferentConditions(decimal salary, int dependentUnderAgeThresholdCount, int dependentOverAgeThresholdCount, bool isLeapYear)
        {
            // Arrange
            var employeeId = 1;
            var employeeDto = new GetEmployeeDto
            {
                Id = employeeId,
                Salary = salary,
                Dependents = new List<GetDependentDto>()
            };

            
            var employeeModelDependents = new List<Dependent>();

            // Add child dependents
            for (int i = 0; i < dependentUnderAgeThresholdCount; i++)
            {
                employeeDto.Dependents.Add(
                    new()
                    {
                        Id = i,
                        FirstName = "$Child {i}",
                        LastName = "Smith",
                        DateOfBirth = DateTime.Now.AddYears(-i),
                        Relationship = Relationship.Child
                    }
                );

                employeeModelDependents.Add(
                    new()
                    {
                        Id = i,
                        EmployeeId = employeeId,
                        FirstName = "$Child {i}",
                        LastName = "Smith",
                        DateOfBirth = DateTime.Now.AddYears(-i),
                        Relationship = Relationship.Child
                    }
                );
            }

            // Add dependents over age threshold
            if (dependentOverAgeThresholdCount == 1)
            {
                employeeDto.Dependents.Add(
                    new()
                    {
                        Id = 1,
                        FirstName = "Spouse",
                        LastName = "Smith",
                        DateOfBirth = DateTime.Now.AddYears(-51),
                        Relationship = Relationship.Spouse
                    }
                );

                employeeModelDependents.Add(
                   new()
                   {
                        Id = 1,
                        EmployeeId = employeeId,
                        FirstName = "Spouse",
                        LastName = "Smith",
                        DateOfBirth = DateTime.Now.AddYears(-51),
                        Relationship = Relationship.Spouse
                   }
               );
            }
            else
            {
                for (int i = 0; i < dependentOverAgeThresholdCount; i++)
                {
                    employeeDto.Dependents.Add(
                        new()
                        {
                            Id = i,
                            FirstName = $"Relative {i}",
                            LastName = "Smith",
                            DateOfBirth = DateTime.Now.AddYears(-99 - i),
                            Relationship = Relationship.Other
                        }
                    );

                    employeeModelDependents.Add(
                        new()
                        {
                            Id = i,
                            EmployeeId = employeeId,
                            FirstName = $"Relative {i}",
                            LastName = "Smith",
                            DateOfBirth = DateTime.Now.AddYears(-99 - i),
                            Relationship = Relationship.Other
                        }
                    );
                }
            }

            var employee = new Employee
            {
                Id = employeeId,
                Salary = salary,
                Dependents = employeeModelDependents,
                Paychecks = new List<Paycheck>()
            };

            // Simulate getting dto back from employee service call, and mapping to Employee Model
            _mockEmployeeService.Setup(s => s.Get(employeeId)).ReturnsAsync(employeeDto);
            _mockMapper.Setup(m => m.Map<Employee>(It.IsAny<GetEmployeeDto>())).Returns(employee);

            // Act
            var result = await _paycheckService.Create(employeeId);

            // pass in requirement values
            var paycheckOptions = _mockConfig.Object.GetSection(PaycheckOptions.PaycheckConfig).Get<PaycheckOptions>();

            // Do the calculations here and assert against what the code does

            // ===GROSS PAY ===
            var grossPay = salary / paycheckOptions.PaychecksPerYear;

            // === BASE DEDUCTION ===
            var baseDeduction = (paycheckOptions.MonthlyBaseCost * MONTHS_IN_YEAR) / paycheckOptions.PaychecksPerYear;
            
            // === DEPENDENT DEDUCTIONS ===
            decimal totalDependentCost = 0m;
            decimal baseDependentCost = (paycheckOptions.MonthlyBaseDependentCost * 12) / paycheckOptions.PaychecksPerYear;
            decimal surchargeDependentCost = (paycheckOptions.MonthlyDependentSurchargeAgeCost * 12) / paycheckOptions.PaychecksPerYear;

            // take baseDependentCost and multiply it by total dependent count
            totalDependentCost += baseDependentCost * (dependentUnderAgeThresholdCount + dependentOverAgeThresholdCount);

            // Add surcharge for dependents over age threshold
            totalDependentCost += surchargeDependentCost * dependentOverAgeThresholdCount;

            // Calculate salary surcharge if applicable
            decimal salarySurcharge = 0m;
            if (salary >= paycheckOptions.AnnualSurchargeSalaryThreshold)
            {
                salarySurcharge = (paycheckOptions.AnnualSurchargePercent / 100) * salary;
                salarySurcharge = salarySurcharge / paycheckOptions.PaychecksPerYear;
            }

            // add up total
            var totalDeduction = baseDeduction + salarySurcharge + totalDependentCost;

            // Net pay
            var netPay = grossPay - totalDeduction;

            // Assert
            _mockPaycheckRepository.Verify(r => r.Add(It.IsAny<Paycheck>()), Times.Once);
            Assert.NotNull(result.Data);
            Assert.Equal(netPay, result.Data.NetPay, 2);
            Assert.Equal(grossPay, result.Data.GrossPay, 2);

            var deductionSummary = result.Data.DeductionSummary;

            Assert.Equal(baseDeduction, deductionSummary.BaseDeduction, 2);
            Assert.Equal(totalDependentCost, deductionSummary.DependentDeduction, 2);
            Assert.Equal(salarySurcharge, deductionSummary.SalarySurchargeDeduction, 2);
            Assert.Equal(totalDeduction, deductionSummary.TotalDeduction, 2);
        }

        // have tests for if successful and if exception thrown
    }
}
