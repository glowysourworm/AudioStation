using AudioStation.Core.Model.Interface;
using AudioStation.Core.Utility.FileUtility;
using AudioStation.ViewModels.LibraryViewModels;

namespace AudioStation.Event.DialogEvents.DialogEditorEvents
{
    public class DialogNamingGroupingFormatViewModel : DialogEditorViewModelBase
    {
        string _labelText;
        string _result;
        ILibraryFormat _format;          // Name, String Template, Description
        string _description;
        string _usage;
        string _userOutput;
        string _userOutputDescription;

        char[] _invalidPathCharacters;
        char _escapeCharacter;

        TagSmallViewModel _exampleTrack;

        public string LabelText
        {
            get { return _labelText; }
            set { this.RaiseAndSetIfChanged(ref _labelText, value); }
        }
        public string Result
        {
            get { return _result; }
            set { this.RaiseAndSetIfChanged(ref _result, value); }
        }
        public char[] InvalidPathCharacters
        {
            get { return _invalidPathCharacters; }
            private set { this.RaiseAndSetIfChanged(ref _invalidPathCharacters, value); }
        }
        public char EscapeCharacter
        {
            get { return _escapeCharacter; }
            private set { this.RaiseAndSetIfChanged(ref _escapeCharacter, value); }
        }
        public ILibraryFormat Format
        {
            get { return _format; }
            set { this.RaiseAndSetIfChanged(ref _format, value); }
        }
        public string Description
        {
            get { return _description; }
            set { this.RaiseAndSetIfChanged(ref _description, value); }
        }
        public string Usage
        {
            get { return _usage; }
            set { this.RaiseAndSetIfChanged(ref _usage, value); }
        }
        public TagSmallViewModel ExampleTrack
        {
            get { return _exampleTrack; }
            set { this.RaiseAndSetIfChanged(ref _exampleTrack, value); }
        }
        public string UserOutput
        {
            get { return _userOutput; }
            set { this.RaiseAndSetIfChanged(ref _userOutput, value); }
        }
        public string UserOutputDescription
        {
            get { return _userOutputDescription; }
            set { this.RaiseAndSetIfChanged(ref _userOutputDescription, value); }
        }

        public DialogNamingGroupingFormatViewModel()
        {
            this.LabelText = string.Empty;
            this.Result = string.Empty;
            this.Format = new LibraryFormatViewModel();
            this.EscapeCharacter = '^';
            this.InvalidPathCharacters = MigrationHelpers.GetInvalidFileCharacters();
            this.Description = string.Empty;
            this.UserOutput = string.Empty;
            this.UserOutputDescription = string.Empty;
        }

        protected override void OnPropertyChanged(string name)
        {
            base.OnPropertyChanged(name);

            if (name == nameof(Result))
            {
                // Hard-coding extraneous field here: this would be handled by user code
                if (!string.IsNullOrWhiteSpace(this.Result) &&
                    this.Format != null &&
                    this.ExampleTrack != null)
                    this.UserOutput = MigrationHelpers.Format(this.Format, this.Result, this.ExampleTrack, extraneous => "mp3");
            }
        }

        protected override bool Validate(out bool isComplete, out string validationMessage)
        {
            isComplete = false;
            validationMessage = "Must complete path format";

            // Empty
            if (string.IsNullOrWhiteSpace(this.Result))
                return false;

            var pathPieces = this.Result.Split('\\', StringSplitOptions.RemoveEmptyEntries);

            foreach (var path in pathPieces)
            {
                // Empty
                if (string.IsNullOrWhiteSpace(path))
                    return false;

                // Tokens
                if (!path.Contains(this.Format.LeftToken) ||
                    !path.Contains(this.Format.RightToken) ||
                     path.Length < 3)
                {
                    validationMessage = string.Format("Please use token format:  name -> {0}name{1}", this.Format.LeftToken, this.Format.RightToken);
                    return false;
                }

                // Illegal Characters
                if (!MigrationHelpers.AreFriendlyPath(false, path))
                {
                    validationMessage = "Please remove illegal characters from your format string";
                    return false;
                }

                // Proper Token Placement
                var pathName = path.Replace(this.Format.LeftToken.ToString(), "")
                                   .Replace(this.Format.RightToken.ToString(), "");

                // Is Format Field (invalid)
                if (pathName != path &&
                    pathName.Length != path.Length - 2)
                {
                    validationMessage = string.Format("Please use token format:  name -> {0}name{1}", this.Format.LeftToken, this.Format.RightToken);
                    return false;
                }

                // Is Format Field (true)
                else if (pathName != path)
                {
                    var keyword = this.Format.Fields.FirstOrDefault(x => x.TokenName == pathName);

                    if (keyword == null)
                    {
                        validationMessage = string.Format("Invalid format field:  " + pathName);
                        return false;
                    }
                }

                // Is Format Field (false)
                else
                {
                    // Nothing to do
                }
            }

            isComplete = true;
            validationMessage = string.Empty;

            return true;
        }
    }
}
