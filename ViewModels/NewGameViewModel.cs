using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static Backgammon.ViewModel;

namespace Backgammon.ViewModels
{
    public class NewGameViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); //генерация события
        }

        public ICommand YesCommand { get;}
        public ICommand NoCommand { get;}

        private bool _dialogResult;

        public NewGameViewModel()
        {
            YesCommand = new RelayCommand(Yes);
            NoCommand = new RelayCommand(No);
        }

        private void Yes(object parameter) //нажатие на да
        {
            _dialogResult = true;
            CloseWindow();
        }

        private void No(object parameter) //нажатие на нет
        {
            _dialogResult = false;
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
