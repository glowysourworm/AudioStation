using System.Windows;
using System.Windows.Controls;

using AudioStation.Controller.Interface;
using AudioStation.Core.Component.Interface;
using AudioStation.Model;
using AudioStation.ViewModels.Controls;

using Microsoft.Extensions.Logging;

using SimpleWpf.IocFramework.Application.Attribute;

namespace AudioStation.Controls
{
    [IocExportDefault]
    public partial class FileTreeView : UserControl
    {
        private readonly IOutputController _outputController;

        public static readonly DependencyProperty FileTreeProperty =
            DependencyProperty.Register("FileTree", typeof(FileItemViewModel), typeof(FileTreeView));

        public static readonly DependencyProperty SearchPatternProperty = 
            DependencyProperty.RegisterAttached("SearchPattern", typeof(string), typeof(FileTreeView));

        public static readonly DependencyProperty BaseDirectoryProperty =
            DependencyProperty.Register("BaseDirectory", typeof(string), typeof(FileTreeView));

        public FileItemViewModel FileTree
        {
            get { return (FileItemViewModel)GetValue(FileTreeProperty); }
            set { SetValue(FileTreeProperty, value); }
        }

        public string SearchPattern
        {
            get { return (string)GetValue(SearchPatternProperty); }
            set { SetValue(SearchPatternProperty, value); }
        }

        public string BaseDirectory
        {
            get { return (string)GetValue(BaseDirectoryProperty); }
            set { SetValue(BaseDirectoryProperty, value); }
        }

        public FileTreeView()
        {
            InitializeComponent();
        }

        [IocImportingConstructor]
        public FileTreeView(IDialogController dialogController, IOutputController outputController)
        {
            _outputController = outputController;

            InitializeComponent();

            this.DataContext = this;
            this.Loaded += OnLoaded;

            this.FolderBrowserButton.Click += (sender, e) =>
            {
                var folder = dialogController.ShowSelectFolder();

                if (!string.IsNullOrEmpty(folder))
                {
                    this.BaseDirectory = folder;
                    Load();
                }
            };
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Load();
        }

        private void Load()
        {
            if (!string.IsNullOrEmpty(this.BaseDirectory) && 
                !string.IsNullOrEmpty(this.SearchPattern))
            {
                try
                {
                    this.FileTree = new FileItemViewModel(this.BaseDirectory, true, this.SearchPattern);
                }
                catch (Exception ex)
                {
                    _outputController.Log("Error loading directory:  {0}", LogMessageType.General, LogLevel.Error, ex, ex.Message);
                }
            }
        }
    }
}
