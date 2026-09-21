using AudioStation.Core.Component.LibraryLoaderComponent;
using AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels;
using AudioStation.ViewModels.ComponentViewModels.LogViewModels;

namespace AudioStation.Service
{
    public static class LibraryLoaderHelpers
    {
        public static void ApplyLibraryLoaderWorkItem(LibraryLoaderWorkItemUpdate sender, ref LibraryWorkItemViewModel viewModel)
        {
            // Log Messages
            foreach (var message in sender.Log)
            {
                if (!viewModel.LogMessages.Any(x => x.Timestamp.Equals(message.Timestamp)))
                {
                    viewModel.LogMessages.Add(new LogMessageViewModel()
                    {
                        Level = message.Level,
                        Message = message.Message,
                        Timestamp = message.Timestamp,
                        Type = message.Type
                    });
                }
            }

            // Work Steps
            foreach (var workStep in sender.ResultStepsCompleted)
            {
                if (!viewModel.WorkSteps.Any(x => x.StepNumber == workStep.StepNumber))
                {
                    viewModel.WorkSteps.Add(new LibraryLoaderWorkStepViewModel()
                    {
                        Complete = workStep.Completed,
                        Message = workStep.Message,
                        StepNumber = workStep.StepNumber,
                        Result = workStep.Result
                    });
                }
            }

            viewModel.LastMessage = sender.Log.LastOrDefault()?.Message ?? string.Empty;
            viewModel.ErrorLevel = sender.ResultStepsCompleted.Any() ? sender.ResultStepsCompleted.Max(x => x.Result) : LibraryWorkerResultLevel.None;
            viewModel.State = sender.State;
            viewModel.Progress = !sender.ResultStepsCompleted.Any() ? 0 : (sender.ResultStepsCompleted.Count() / (double)sender.ResultStepCount);
        }

        public static void ApplyLibraryLoaderWorkItem(LibraryLoaderWorkItem sender, ref LibraryWorkItemViewModel viewModel)
        {
            // Log Messages
            foreach (var message in sender.GetOutputItem().CurrentLog)
            {
                if (!viewModel.LogMessages.Any(x => x.Timestamp.Equals(message.Timestamp)))
                {
                    viewModel.LogMessages.Add(new LogMessageViewModel()
                    {
                        Level = message.Level,
                        Message = message.Message,
                        Timestamp = message.Timestamp,
                        Type = message.Type
                    });
                }
            }

            // Work Steps
            foreach (var workStep in sender.GetOutputItem().ResultSteps)
            {
                if (!viewModel.WorkSteps.Any(x => x.StepNumber == workStep.StepNumber))
                {
                    viewModel.WorkSteps.Add(new LibraryLoaderWorkStepViewModel()
                    {
                        Complete = workStep.Completed,
                        Message = workStep.Message,
                        StepNumber = workStep.StepNumber,
                        Result = workStep.Result
                    });
                }
            }

            viewModel.LastMessage = sender.GetOutputItem().CurrentLog.LastOrDefault()?.Message ?? string.Empty;
            viewModel.ErrorLevel = sender.GetOutputItem().ResultSteps.Any() ? sender.GetOutputItem().ResultSteps.Max(x => x.Result) : LibraryWorkerResultLevel.None;
            viewModel.State = sender.GetLoadState();
            viewModel.Progress = !sender.GetOutputItem().ResultSteps.Any(x => x.Completed) ? 0
                                    : (sender.GetOutputItem().ResultSteps.Count(x => x.Completed) / (double)sender.GetOutputItem().ResultSteps.Count());
        }
    }
}
