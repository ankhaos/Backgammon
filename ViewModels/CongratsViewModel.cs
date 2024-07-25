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
    public class CongratsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); //генерация события
        }

        public ICommand StartCommand { get; }
        public ICommand ExitCommand { get; }

        string _winner = "";
        bool _dialogResult = false;

        public string Winner
        {
            get => _winner;
            set
            {
                _winner = value;
                OnPropertyChanged();
            }
        }

        public CongratsViewModel(string win)
        {
            _winner = win;
            StartCommand = new RelayCommand(Start);
            ExitCommand = new RelayCommand(Exit);
        }

        private void Start(object parameter) //нажатие на начало новой игры
        {
            _dialogResult = true;
            CloseWindow();
        }

        private void Exit(object parameter) //нажатие на выход
        {
            Application.Current.Shutdown(); //закрытие всех окон
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
