using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using static Backgammon.ViewModel;

namespace Backgammon.ViewModels
{
    public class DefinePlayerViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); //генерация события
        }

        public ICommand OkCommand { get; }

        bool _dialogResult;
        string _player = "";

        public string Player
        {
            get => _player;
            set
            {
                _player = value;
                OnPropertyChanged();
            }
        }

        public DefinePlayerViewModel(string player)
        {
            OkCommand = new RelayCommand(Ok);
            _player = $"Первый ход за игроком: {player}";
        }

        private void Ok(object parameter) //нажатие на ok
        {
            _dialogResult = true;
            CloseWindow();
        }

        private void CloseWindow() //закрытие этого окна
        {
            Window window = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.DataContext == this); //метод возвращает коллекцию всех открытых окон в текущем приложении и ищет в этой коллекции окно, у которого DataContext является текущим экземпляром
            if (window != null)
            {
                window.DialogResult = _dialogResult;
            }
        }
    }
}
