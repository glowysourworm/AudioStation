using System.ComponentModel.DataAnnotations;

namespace AudioStation.Core.Component.LibraryLoaderComponent
{
    public enum LibraryWorkerResultLevel : int
    {
        [Display(Name = "Success", Description = "Library worker step was successful")]
        Success = 0,

        [Display(Name = "No Result (service)", Description = "Library worker service call yielded no result(s)")]
        ServiceNoResult = 1,

        [Display(Name = "Data Error", Description = "Library worker step had data error(s) that won't affect application processing")]
        DataError = 2,

        [Display(Name = "Failure (service)", Description = "Library worker service call failed")]
        ServiceFailure = 3,

        [Display(Name = "Failure", Description = "Library worker step had error(s)")]
        Failure = 4,
    }

    public class LibraryWorkerStepResult
    {
        public LibraryWorkerResultLevel Result { get; set; }
        public string Message { get; set; }

        /// <summary>
        /// Indicates that the work step completed. Depending on the worker result type, the library loader
        /// may choose to continue with the next step.
        /// </summary>
        public bool Completed { get; set; }
        public int StepNumber { get; set; }

        public LibraryWorkerStepResult()
        {
            this.Message = string.Empty;
        }

        public static LibraryWorkerStepResult Success(int stepNumber, string message)
        {
            return new LibraryWorkerStepResult()
            {
                Completed = true,
                Message = message,
                StepNumber = stepNumber,
                Result = LibraryWorkerResultLevel.Success
            };
        }

        public static LibraryWorkerStepResult Failure(int stepNumber, string message)
        {
            return new LibraryWorkerStepResult()
            {
                Completed = false,
                Message = message,
                StepNumber = stepNumber,
                Result = LibraryWorkerResultLevel.Failure
            };
        }
    }

}
