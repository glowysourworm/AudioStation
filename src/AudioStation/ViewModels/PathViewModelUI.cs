using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SimpleWpf.ViewModel;

namespace AudioStation.ViewModels
{
    public class PathViewModelUI : PathViewModel
    {
        bool _isExpanded;
        bool _isSelected;

        public bool IsExpanded
        {
            get { return _isExpanded; }
            set { this.RaiseAndSetIfChanged(ref _isExpanded, value); }
        }
        public bool IsSelected
        {
            get { return _isSelected; }
            set { this.RaiseAndSetIfChanged(ref _isSelected, value); }
        }

        public PathViewModelUI(string baseDirectory, string path) : base(baseDirectory, path)
        {
        }

        public override string ToString()
        {
            return this.Path;
        }
    }
}
