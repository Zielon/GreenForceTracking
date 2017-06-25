using System.Collections.ObjectModel;

namespace Server.Messages
{
    public class MessagesContainer
    {
        private readonly ObservableCollection<Message> recivedMessages;

        public MessagesContainer()
        {
            recivedMessages = new ObservableCollection<Message>();
        }

        public ObservableCollection<Message> RecivedMessages
        {
            get { return recivedMessages; }
        }
    }
}
