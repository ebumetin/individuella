using System.Collections.Specialized;
using System.ComponentModel;

namespace WpfGrejs.ViewModel;

public class LoginViewModel : INotifyCollectionChanged, INotifyPropertyChanged
{
    public event NotifyCollectionChangedEventHandler? CollectionChanged;
    public event PropertyChangedEventHandler? PropertyChanged;

    public void LoginPressed()
    {
    }
}