namespace Api.Configuration
{
    public class PaycheckOptions
    {
        // This is here to allow config to bind to this obj without hardcoding name, see https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-6.0
        // for where Microsoft shows an example of this
        public const string PaycheckConfig = "PaycheckConfig";

        public int PaychecksPerYear { get; set; }

        /// <summary>
        /// The base cost for the employees benefits
        /// </summary>
        public decimal MonthlyBaseCost { get; set; }

        /// <summary>
        /// The additional base cost incurred <b>per dependent</b>
        /// </summary>
        public decimal MonthlyBaseDependentCost { get; set; }

        /// <summary>
        /// The salary threshold at which <see cref="AnnualBenefitSurchargePercent"/> takes effect.
        /// For example, if the threshold is $80,000, employees who make $80,000 or more will be charged and extra cost on their salary
        /// </summary>
        public decimal AnnualSurchargeSalaryThreshold { get; set; }

        /// <summary>
        /// If the salary meets or exceeds the <see cref="AnnualBenefitSurchargeSalaryThreshold"/>, then this percent of the salary will be added
        /// For example, if the percentage is set to 2, 2% of the employees salary will be added as a additonal cost
        /// </summary>
        public decimal AnnualSurchargePercent { get; set; }

        /// <summary>
        /// The age threshold at which an additional surcharge is applied to dependents.
        /// The surcharge is applied to dependents who are at this age or older
        /// </summary>
        public int DependentSurchargeAgeThreshold { get; set; }

        /// <summary>
        /// The surcharge amount that is added to the deduction cost
        /// when the dependents age is over the <see cref="DependentSurchargeAgeThreshold"/>
        /// </summary>
        public decimal MonthlyDependentSurchargeAgeCost { get; set; }
    }
}
