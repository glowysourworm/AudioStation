using System.Collections.ObjectModel;

using SimpleWpf.Extensions;

namespace AudioStation.Event.DialogEvents
{
    public class DialogMessageListViewModel : DialogViewModelBase
    {
        public ObservableCollection<string> _messageList;

        /// <summary>
        /// List of messages for the Message List View option
        /// </summary>
        public ObservableCollection<string> MessageList
        {
            get { return _messageList; }
            set { this.RaiseAndSetIfChanged(ref _messageList, value); }
        }

        public DialogMessageListViewModel()
        {
            this.MessageList = new ObservableCollection<string>();
        }

        protected override bool Validate()
        {
            return true;
        }
    }
}
