using System.Windows;
using System.Windows.Controls;

using AudioStation.Controller.Interface;
using AudioStation.Event;
using AudioStation.Event.DialogEvents;
using AudioStation.Service.Interface;
using AudioStation.ViewModels.ComponentViewModels;
using AudioStation.ViewModels.ComponentViewModels.LibraryImporterViewModels;

using SimpleWpf.IocFramework.Application.Attribute;

namespace AudioStation.Views.LibraryImportViews
{
    [IocExportDefault]
    public partial class LibraryImportWorkflowSelectionView : UserControl
    {
        private readonly ILibraryLoaderService _libraryLoaderService;
        private readonly IDialogController _dialogController;

        [IocImportingConstructor]
        public LibraryImportWorkflowSelectionView(ILibraryLoaderService libraryLoaderService, IDialogController dialogController)
        {
            InitializeComponent();

            _libraryLoaderService = libraryLoaderService;
            _dialogController = dialogController;
        }

        private void NewWorkflowButton_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = this.DataContext as LibraryImporterViewModel;

            if (viewModel != null)
            {
                var dialogViewModel = new DialogSingleTextFieldViewModel()
                {
                    Name = "Name",
                    Value = "New Workflow"
                };

                if (_dialogController.ShowDialogWindowSync(DialogEventData.ShowDialogEditor("Import Workflow", DialogEditorView.SingleTextField, dialogViewModel)) == true)
                {
                    viewModel.Workflow = new LibraryImporterWorkflowViewModel()
                    {
                        Configuration = new LibraryImporterConfigurationViewModel(),
                        CreatedDate = DateTime.Now,
                        ModifiedDate = DateTime.Now,
                        Name = dialogViewModel.Value
                    };

                    _libraryLoaderService.AddOrUpdateImportWorkflow(viewModel.Workflow);
                }
            }
        }
    }
}
