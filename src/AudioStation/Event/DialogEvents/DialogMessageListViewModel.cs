﻿using System.Collections.ObjectModel;

using SimpleWpf.Extensions;
using SimpleWpf.ViewModel;

namespace AudioStation.Event.DialogEvents
{
    public class DialogMessageListViewModel : ViewModelBase
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
    }
}
