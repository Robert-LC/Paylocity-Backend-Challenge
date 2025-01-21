namespace Api.Utilities
{
    /// <summary>
    /// Class for Calculation Utilites which may be used in multiple parts of the application
    /// </summary>
    public static class CalculationUtil
    {
        public static int CalculateAge(DateTime dateOfBirth)
        {
            var today = DateTime.Today;
            var age = today.Year - dateOfBirth.Year;

            // Subtract to the right age if their birthday hasn't passed for the current year
            if (today < dateOfBirth.AddYears(age))
            {
                age--;
            }

            return age;
        }
    }
}
