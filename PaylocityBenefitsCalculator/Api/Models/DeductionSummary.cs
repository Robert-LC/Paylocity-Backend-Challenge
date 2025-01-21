namespace Api.Models
{

    /*
     * I made the Deduction class instead of keeping it as a single decimal property that way the user can be 
     * displayed a detailed breakdown of their deductions
     */

    /// <summary>
    /// Shows a more detailed breakdown of deductions/costs on a per paycheck basis
    /// </summary>
    public class DeductionSummary
    {
        /// <summary>
        /// The base deduction cost for the check
        /// </summary>
        public decimal BaseDeduction { get; set; }

        // I could break this further into another object and show the base dependent deduction and the surcharged one if more detailed was needed
        /// <summary>
        /// The deduction the employee is charged for all their dependents
        /// </summary>
        public decimal DependentDeduction { get; set; }

        /// <summary>
        /// The deduction the employee is charged if they make more than a certain salary threshold
        /// </summary>
        public decimal SalarySurchargeDeduction { get; set; }

        /// <summary>
        /// The total deduction/cost for the paycheck. All previous deductions added together.
        /// </summary>
        public decimal TotalDeduction { get; set; }
    }
}
