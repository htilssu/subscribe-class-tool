using System.Collections.Generic;
using ClassRegisterApp.Models;

namespace ClassRegisterApp.ViewModel;

/// <inheritdoc />
public class MainViewModel : ViewModelBase
{
    private List<User> _user = [];
    public List<User> User
    {
        get => _user;
        set
        {
            if (Equals(value, _user)) return;
            _user = value;
            OnPropertyChanged();
        }
    }
}
