using System.Windows.Controls;

using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.IocFramework.EventAggregation;

namespace AudioStation.Views
{
    [IocExportDefault]
    public partial class NowPlayingView : UserControl
    {
        IIocEventAggregator _eventAggregator;

        [IocImportingConstructor]
        public NowPlayingView(IIocEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;

            InitializeComponent();
        }
    }
}
