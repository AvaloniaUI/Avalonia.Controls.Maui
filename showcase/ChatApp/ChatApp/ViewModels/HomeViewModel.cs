using ChatApp.Models;
using ChatApp.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ChatApp.ViewModels
{
    public class HomeViewModel : ViewModelBase
    {
        ObservableCollection<User> _users = new ObservableCollection<User>();
        ObservableCollection<Message> _recentChat = new ObservableCollection<Message>();

        public HomeViewModel()
        {
            LoadData();
        }

        public ObservableCollection<User> Users
        {
            get { return _users; }
            set
            {
                _users = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Message> RecentChat
        {
            get { return _recentChat; }
            set
            {
                _recentChat = value;
                OnPropertyChanged();
            }
        }

        Message? _selectedMessage;

        public Message? SelectedMessage
        {
            get => _selectedMessage;
            set
            {
                if (_selectedMessage != value)
                {
                    _selectedMessage = value;
                    OnPropertyChanged();
                    if (_selectedMessage != null)
                    {
                        OnNavigate(_selectedMessage);
                        SelectedMessage = null;
                    }
                }
            }
        }

        public ICommand DetailCommand => new Command<object>(OnNavigate);

        void LoadData()
        {
            Users = new ObservableCollection<User>(MessageService.Instance.GetUsers());
            RecentChat = new ObservableCollection<Message>(MessageService.Instance.GetChats());
        }

        void OnNavigate(object parameter)
        {
            NavigationService.Instance.NavigateToAsync<DetailViewModel>(parameter);
        }
    }
}