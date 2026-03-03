using SurfingApp.Models;
using SurfingApp.Services;
using System.Collections.ObjectModel;

namespace SurfingApp.ViewModels
{
    public class HomeViewModel : ViewModelBase
    {
        ObservableCollection<User> _users;
        ObservableCollection<Post> _posts;

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

        public ObservableCollection<Post> Posts
        {
            get { return _posts; }
            set
            {
                _posts = value;
                OnPropertyChanged();
            }
        }

        void LoadData()
        {
            Users = new ObservableCollection<User>(UserService.Instance.GetUsers());
            Posts = new ObservableCollection<Post>(PostService.Instance.GetPosts());
        }
    }
}