using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace Backgammon.ViewModels
{
    public class Chips : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); //генерация события
        }

        public ICommand ChipMouseMoveCommand
        {
            get
            {
                return new RelayCommand(param => ChipMouseMove(param));
            }
        }
        public ICommand ChipMouseLeftButtonDownCommand
        {
            get
            {
                return new RelayCommand(param => ChipMouseLeftButtonDown());
            }
        }
        public ICommand ChipMouseLeftButtonUpCommand
        {
            get
            {
                return new RelayCommand(param => ChipMouseLeftButtonUp(param));
            }
        }


        double _left; //левое положение
        double _top; //верхнее положение
        string _color; //цвет
        Point _initialPosition; //начальная позиция фишки
        Point _startPoint; //начальная точка при перемещении
        Point _prevPosition; //начальное значение фишки за ход
        int _zIndex; 
        Point _deviation; //отклонение границ фишки от положения мыши
        ViewModel _viewModel; //текущая модель представления

        public Chips(ViewModel viewModel) //конструктор
        {
            _viewModel = viewModel;
        }

        public void ChipMouseLeftButtonDown()
        {
            if (Color == "white" && _viewModel.IsDiceRolling && !_viewModel.IsComputer && !_viewModel.IsDragging) //не берем фишку, пока крутятся кубики или играет компьютер
            {
                    _viewModel.DraggedChip = this;
                    //фиксируется нажатие на фишку
                    _prevPosition = new Point(Left, Top);
                    _deviation = new Point(Mouse.GetPosition(Application.Current.MainWindow).X-Left, Mouse.GetPosition(Application.Current.MainWindow).Y - Top); 
                    int ind = _viewModel.Positions.FindPosition(_prevPosition.X, _prevPosition.Y);
                    if (_viewModel.IsStarted && _viewModel.Game.Player == 0 && ind != -1 && ((_viewModel.Game.Gamefield[ind, 1] - 1) * 10 + _viewModel.Positions.Y[ind] == _prevPosition.Y || _viewModel.Positions.Y[ind] - (_viewModel.Game.Gamefield[ind, 1] - 1) * 10 == _prevPosition.Y))
                    {
                        _startPoint = new Point(Left, Top);
                        _viewModel.Pos = _viewModel.Game.CanMove(ind);
                        if(_viewModel.Pos.Count != 0) _viewModel.IsDragging = true;
                        _viewModel.FuturePos = _viewModel.PosToString(_viewModel.Pos);
                    }
            }
        }
        public void ChipMouseLeftButtonUp(object sender)
        {
            if (_viewModel.DraggedChip == sender && _viewModel.IsDragging)
            {
                _viewModel.IsDragging = false;
                _viewModel.ChangePosition(this, Left, Top); //изменение позиции
            }
        }
        //фиксируется перемещение
        public void ChipMouseMove(object sender)
        {
                if (_viewModel.DraggedChip == sender && _viewModel.IsDragging)
                {
                    Point currentPosition = new Point(Mouse.GetPosition(Application.Current.MainWindow).X - _deviation.X, Mouse.GetPosition(Application.Current.MainWindow).Y - _deviation.Y);
                    Left += currentPosition.X - _startPoint.X;
                    Top += currentPosition.Y - _startPoint.Y;
                    _startPoint = currentPosition;
                }
        }

        public int ZIndex
        {
            get => _zIndex;
            set
            {
                if (_zIndex != value)
                {
                    _zIndex = value;
                    OnPropertyChanged();
                }
            }
        }

        public double Left
        {
            get => _left;
            set
            {
                if (_left != value)
                {
                    _left = value;
                    OnPropertyChanged();
                }
            }
        }

        public double Top
        {
            get => _top;
            set
            {
                if (_top != value)
                {
                    _top = value;
                    OnPropertyChanged();
                }
            }
        }

        public Point InitialPosition
        {
            get => _initialPosition;
            set
            {
                if (_initialPosition != value)
                {
                    _initialPosition = value;
                    OnPropertyChanged();
                }
            }
        }
        public Point PrevPosition
        {
            get => _prevPosition;
            set
            {
                if (_prevPosition != value)
                {
                    _prevPosition = value;
                    OnPropertyChanged();
                }
            }
        }
        public string Color
        {
            get => _color;
            set
            {
                if (_color != value)
                {
                    _color = value;
                    OnPropertyChanged();
                }
            }
        }
    }
}
