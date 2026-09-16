namespace AudioStation.Event
{
    public static class DialogEventHandlers
    {
        /// <summary>
        /// Delegate for updating dialog progress. The parameters translate into the view model for the LoadingView.
        /// </summary>
        /// <param name="taskCount">Total # of tasks to run (not Task instances - sub tasks.. usually files to process)</param>
        /// <param name="tasksComplete">Current number of completed tasks</param>
        /// <param name="subTaskCount">Current number of sub-tasks</param>
        /// <param name="subTasksComplete">Current number of sub-tasks that finished</param>
        /// <param name="message">Current message to the user</param>
        public delegate void DialogProgressHandler(int taskCount, int tasksComplete, int subTaskCount, int subTasksComplete, string message);
    }
}
