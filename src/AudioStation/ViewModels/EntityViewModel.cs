using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AudioStation.Core.Model;

using SimpleWpf.Extensions;
using SimpleWpf.ViewModel;

namespace AudioStation.ViewModels
{
    public class EntityViewModel : ViewModelBase
    {
        int _id;
        LibraryEntityType _type;

        public int Id
        {
            get { return _id; }
            private set { this.RaiseAndSetIfChanged(ref _id, value); }
        }
        public LibraryEntityType Type
        {
            get { return _type; }
            private set { this.RaiseAndSetIfChanged(ref _type, value); }
        }

        public EntityViewModel(int id,  LibraryEntityType type)
        {
            this.Id = id;
            this.Type = type;
        }
    }
}
