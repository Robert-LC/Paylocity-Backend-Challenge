using Api.Dtos.DeductionSummary;
using Api.Dtos.Employee;

namespace Api.Dtos.Paycheck
{
    public class AddPaycheckDto
    {
        public int EmployeeId { get; set; }
        /// <summary>
        /// The date in which the Paycheck / Pay period starts on
        /// </summary>
        public DateTime PeriodStart { get; set; }

        /// <summary>
        /// The date in which the Paycheck / Pay period ends on
        /// </summary>
        public DateTime PeriodEnd { get; set; }

        /// <summary>
        /// The period's pay before any deductions
        /// </summary>
        public decimal GrossPay { get; set; }

        /// <summary>
        /// The period's pay after all deductions are taken out
        /// </summary>
        public decimal NetPay { get; set; }

        /// <summary>
        /// The amount of pay deducted from gross for benefits/dependents
        /// </summary>
        public DeductionSummaryDto DeductionSummary { get; set; }
    }
}
