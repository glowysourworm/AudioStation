namespace AudioStation.EventHandler
{
    public static class DialogEventHandlers
    {
        /// <summary>
        /// Delegate for updating dialog progress. The parameters translate into the view model for the LoadingView.
        /// </summary>
        /// <param name="taskCount">Total # of tasks to run (not Task instances - sub tasks.. usually files to process)</param>
        /// <param name="tasksComplete">Current number of completed tasks (either error or success)</param>
        /// <param name="tasksError">Current number of tasks that had an error and finished un-successfully</param>
        /// <param name="message">Current message to the user</param>
        public delegate void DialogProgressHandler(int taskCount, int tasksComplete, int tasksError, string message);
    }
}
