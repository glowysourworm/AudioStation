namespace AudioStation.Service
{
    //public static class LibraryLoaderHelpers
    //{
    //    public static void ApplyLibraryLoaderBulkWorkItem(LibraryLoaderBulkWorkItem sender, LibraryBulkWorkItemViewModel viewModel)
    //    {
    //        viewModel.CompletedCount = sender.GetCompletedCount();
    //        viewModel.DataErrorCount = sender.GetCount(LibraryWorkerResultLevel.DataError);
    //        viewModel.DataWarningCount = sender.GetCount(LibraryWorkerResultLevel.DataWarning);
    //        viewModel.FailureCount = sender.GetCount(LibraryWorkerResultLevel.Failure);
    //        viewModel.PendingCount = sender.GetCount(LibraryWorkItemState.Pending);
    //        viewModel.ProcessingCount = sender.GetCount(LibraryWorkItemState.Processing);
    //        viewModel.Progress = sender.GetProgress();
    //        viewModel.ServiceFailureCount = sender.GetCount(LibraryWorkerResultLevel.ServiceFailure);
    //        viewModel.ServiceNoResultCount = sender.GetCount(LibraryWorkerResultLevel.ServiceNoResult);
    //        viewModel.State = sender.GetLoadState();
    //        viewModel.SuccessCount = sender.GetCount(LibraryWorkerResultLevel.Success);
    //        viewModel.TotalCount = sender.GetCount();

    //        // Work Items:  These will have contention on the worker thread
    //        //
    //        sender.IterateWorkItems(workItem =>
    //        {
    //            var existingItem = viewModel.WorkItems.ContainsKey(workItem.GetId());

    //            LibraryWorkItemViewModel itemVM = null;

    //            if (existingItem)
    //                itemVM = viewModel.WorkItems[workItem.GetId()];

    //            else
    //            {
    //                itemVM = new LibraryWorkItemViewModel(workItem.GetId(), workItem.GetLoadType());
    //            }


    //            // Apply Update
    //            ApplyLibraryLoaderWorkItem(workItem, itemVM);

    //            // -> Bulk Work Item (add)
    //            if (!existingItem)
    //                viewModel.WorkItems.Add(itemVM.Id, itemVM);
    //        });
    //    }
    //    public static void ApplyLibraryLoaderBulkWorkItemUpdate(LibraryLoaderBulkWorkItemUpdate sender, LibraryBulkWorkItemViewModel bulkWorkItem)
    //    {
    //        bulkWorkItem.CompletedCount = sender.CompletedCount;
    //        bulkWorkItem.DataErrorCount = sender.DataErrorCount;
    //        bulkWorkItem.DataWarningCount = sender.DataWarningCount;
    //        bulkWorkItem.FailureCount = sender.FailureCount;
    //        bulkWorkItem.PendingCount = sender.PendingCount;
    //        bulkWorkItem.ProcessingCount = sender.ProcessingCount;
    //        bulkWorkItem.Progress = sender.Progress;
    //        bulkWorkItem.ServiceFailureCount = sender.ServiceFailureCount;
    //        bulkWorkItem.ServiceNoResultCount = sender.ServiceNoResultCount;
    //        bulkWorkItem.State = sender.State;
    //        bulkWorkItem.SuccessCount = sender.SuccessCount;
    //        bulkWorkItem.TotalCount = sender.TotalCount;

    //        // Completed Work Item: This happens at the end of each work item pass
    //        //
    //        if (sender.CompletedWorkItem != null)
    //        {
    //            // This work item is already in the bulk collection
    //            var workItem = bulkWorkItem.WorkItems[sender.CompletedWorkItem.GetId()];

    //            LibraryLoaderHelpers.ApplyLibraryLoaderWorkItem(sender.CompletedWorkItem, workItem);
    //        }
    //    }
    //    public static void ApplyLibraryLoaderWorkItem(LibraryLoaderWorkItemUpdate sender, LibraryWorkItemViewModel viewModel)
    //    {
    //        // Log Messages
    //        foreach (var message in sender.Log)
    //        {
    //            if (!viewModel.LogMessages.Any(x => x.Timestamp.Equals(message.Timestamp)))
    //            {
    //                viewModel.LogMessages.Add(new LogMessageViewModel()
    //                {
    //                    Level = message.Level,
    //                    Message = message.Message,
    //                    Timestamp = message.Timestamp,
    //                    Type = message.Type
    //                });
    //            }
    //        }

    //        // Work Steps
    //        foreach (var workStep in sender.ResultStepsCompleted)
    //        {
    //            if (!viewModel.WorkSteps.Any(x => x.StepNumber == workStep.StepNumber))
    //            {
    //                viewModel.WorkSteps.Add(new LibraryLoaderWorkStepViewModel()
    //                {
    //                    Complete = workStep.Completed,
    //                    Message = workStep.Message,
    //                    StepNumber = workStep.StepNumber,
    //                    Result = workStep.Result
    //                });
    //            }
    //        }

    //        viewModel.LastMessage = sender.Log.LastOrDefault()?.Message ?? string.Empty;
    //        viewModel.ErrorLevel = sender.ResultStepsCompleted.Any() ? sender.ResultStepsCompleted.Max(x => x.Result) : LibraryWorkerResultLevel.None;
    //        viewModel.State = sender.State;
    //        viewModel.Progress = !sender.ResultStepsCompleted.Any() ? 0 : (sender.ResultStepsCompleted.Count() / (double)sender.ResultStepCount);
    //    }

    //    public static void ApplyLibraryLoaderWorkItem(LibraryLoaderWorkItem sender, LibraryWorkItemViewModel viewModel)
    //    {
    //        // Log Messages
    //        foreach (var message in sender.GetOutputItem().CurrentLog)
    //        {
    //            if (!viewModel.LogMessages.Any(x => x.Timestamp.Equals(message.Timestamp)))
    //            {
    //                viewModel.LogMessages.Add(new LogMessageViewModel()
    //                {
    //                    Level = message.Level,
    //                    Message = message.Message,
    //                    Timestamp = message.Timestamp,
    //                    Type = message.Type
    //                });
    //            }
    //        }

    //        // Work Steps
    //        foreach (var workStep in sender.GetOutputItem().ResultSteps)
    //        {
    //            if (!viewModel.WorkSteps.Any(x => x.StepNumber == workStep.StepNumber))
    //            {
    //                viewModel.WorkSteps.Add(new LibraryLoaderWorkStepViewModel()
    //                {
    //                    Complete = workStep.Completed,
    //                    Message = workStep.Message,
    //                    StepNumber = workStep.StepNumber,
    //                    Result = workStep.Result
    //                });
    //            }
    //        }

    //        viewModel.LastMessage = sender.GetOutputItem().CurrentLog.LastOrDefault()?.Message ?? string.Empty;
    //        viewModel.ErrorLevel = sender.GetOutputItem().ResultSteps.Any() ? sender.GetOutputItem().ResultSteps.Max(x => x.Result) : LibraryWorkerResultLevel.None;
    //        viewModel.State = sender.GetLoadState();
    //        viewModel.Progress = !sender.GetOutputItem().ResultSteps.Any(x => x.Completed) ? 0
    //                                : (sender.GetOutputItem().ResultSteps.Count(x => x.Completed) / (double)sender.GetOutputItem().ResultSteps.Count());
    //    }
    //}
}
