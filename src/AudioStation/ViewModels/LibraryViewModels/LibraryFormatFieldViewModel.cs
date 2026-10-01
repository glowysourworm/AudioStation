using AudioStation.Core.Model;
using AudioStation.Core.Model.Interface;

using SimpleWpf.UI.ViewModel;

namespace AudioStation.ViewModels.LibraryViewModels
{
    public class LibraryFormatFieldViewModel : ViewModelBase, ILibraryFormatField
    {
        string _name;
        string _description;
        string _propertyName;
        string _tokenName;
        bool _isExtraneous;
        LibraryExtraneousFields _extraneousType;

        public string Name
        {
            get { return _name; }
            set { this.RaiseAndSetIfChanged(ref _name, value); }
        }
        public string Description
        {
            get { return _description; }
            set { this.RaiseAndSetIfChanged(ref _description, value); }
        }
        public string PropertyName
        {
            get { return _propertyName; }
            set { this.RaiseAndSetIfChanged(ref _propertyName, value); }
        }
        public string TokenName
        {
            get { return _tokenName; }
            set { this.RaiseAndSetIfChanged(ref _tokenName, value); }
        }
        public bool IsExtraneous
        {
            get { return _isExtraneous; }
            set { this.RaiseAndSetIfChanged(ref _isExtraneous, value); }
        }
        public LibraryExtraneousFields ExtraneousType
        {
            get { return _extraneousType; }
            set { this.RaiseAndSetIfChanged(ref _extraneousType, value); }
        }

        public LibraryFormatFieldViewModel()
        {
            this.Name = string.Empty;
            this.Description = string.Empty;
            this.PropertyName = string.Empty;
            this.TokenName = string.Empty;
            this.IsExtraneous = true;
            this.ExtraneousType = LibraryExtraneousFields.FileExtension;
        }
    }
}
