using System;
using System.Collections.ObjectModel;
using System.Net;

namespace ServerApplication
{
    public class MessagesContainer
    {
        private ObservableCollection<Message> recivedMessages;

        public MessagesContainer() {
            recivedMessages = new ObservableCollection<Message>();
        }

        public ObservableCollection<Message> RecivedMessages { get { return recivedMessages; } }
    }
}
