using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;
using System.Windows;
using System.Windows.Controls;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;
using Backgammon.ViewModels;
using System.Windows.Input;

namespace Backgammon
{
    public class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); //генерация события
        }

        public ICommand StartCommand
        { 
            get
            {
                return new RelayCommand(param => Start());
            }
        }


        Dictionary<int, List<Chips>> chipDictionary = new Dictionary<int, List<Chips>> { }; //словарь, где ключом будет индекс, а значением - фишка
        bool isDragging = false; //проверка движения
        bool isComputer = false; //проверка хода компьютера
        bool isDiceRolling = true; //генерит кубики
        Positions positions = new Positions(); //отвечает за позиции на поле
        GameModel game; //игра
        List<int> pos = new List<int>(); //позиции
        private TaskCompletionSource<bool> chipPos; //ожидает расположение фишки
        bool isStarted = false; //проверяет начата ли игра
        string _turn; //текущий игрок
        string _textondice1; //текст на левом кубике
        string _textondice2; //текст на правом кубике
        string _futurePos; //текст возможных ходов выбранной фишки
        ImageSource _playerImage; //картинка текущего игрока
        Chips _draggedChip; //фишка на перемещении

        public ObservableCollection<Chips> _chips { get; set; } //все фишки на поле

        public ViewModel() //конструктор
        {
            _textondice1 = "1";
            _textondice2 = "1";
            _playerImage = new BitmapImage(new Uri("photo/person.png", UriKind.Relative));
            _chips = new ObservableCollection<Chips>();
            SaveInitialPositions();
            ResetChipsToInitialPositions();
        }

        public bool IsDragging
        {
            get => isDragging;
            set {isDragging = value;}
        }
        public bool IsComputer
        {
            get => isComputer;
            set { isComputer = value;}
        }

        public bool IsDiceRolling
        {
            get => isDiceRolling;
            set {isDiceRolling = value;}
        }
        public Positions Positions
        {
            get => positions;
            set { positions = value; }
        }
        public bool IsStarted
        {
            get => isStarted;
            set { isStarted = value; }
        }
        public GameModel Game
        {
            get => game;
            set { game = value; }
        }
        public List<int> Pos
        {
            get => pos;
            set { pos = value; }
        }
        public Chips DraggedChip
        {
            get { return _draggedChip; }
            set
            {
                _draggedChip = value;
                OnPropertyChanged(nameof(DraggedChip));
            }
        }

        //изменяет позицию в соответствии с разметкой поля
        public async Task ChangePosition(Chips chip, double x, double y) 
        {
            int index = positions.FindPosition(x, y); //определение новой позиции взятой фишки
            if (index != -1 && pos.Contains(index))
            {
                game.Move(index, positions.FindPosition(chip.PrevPosition.X, chip.PrevPosition.Y)); //если фишка позволяет сделать ход на это позицию, делается ход
                
                //установка фишки на новую позицию визуально
                chip.Left = positions.X[index]; 
                if (index > 11) chip.Top = positions.Y[index] + 10 * (game.Gamefield[index, 1] - 1);
                else chip.Top = positions.Y[index] - 10 * (game.Gamefield[index, 1] - 1);

                chip.ZIndex = game.Gamefield[index, 1]; //устанавка порядка изображения фишки
            }
            else if (pos.Contains(24) &&
                    (game.Player == 0 && x >= 747 && x <= 50 + 747 && y >= 318 && y <= 318 + 153) //заданы границы дропов (взяты с поля)
                    || (game.Player == 1 && x <= 797 && x >= 50 - 797 && y <= 272 && y >= 272 - 153))
            {
                game.Move(24, positions.FindPosition(chip.PrevPosition.X, chip.PrevPosition.Y)); //если ход осуществляется в сброс, то делается здесь
                chip.Top = y;
                chip.Left = x;
                await Task.Delay(500);
                chip.Top = 650+30; //верхняя граница поля + 30
                chip.Left = 1000+30; //левая граница поля + 30
                chip.ZIndex = -2; //фишка за полем
            }

            else //новая позиция не удовлетворяет выпавшим на кубиках значениям
            {
                //возвращение фишки на исходную позицию
                chip.Top = chip.PrevPosition.Y;
                chip.Left = chip.PrevPosition.X;
            }
            //фишка была перемещена, завершение задачи
            chipPos?.TrySetResult(true);
        }

        private async void StartGame()
        {
            await Task.Delay(500);
            chipDictionary = new Dictionary<int, List<Chips>>
            {
                { 11, new List<Chips> { _chips[15], _chips[16], _chips[17], _chips[18], _chips[19], _chips[20], _chips[21], _chips[22], _chips[23], _chips[24], _chips[25], _chips[26], _chips[27], _chips[28], _chips[29] } }
            };
            isStarted = true; //игра началась
            game = new GameModel();
            await ChangeDiceNumbers(); //генерация кубиков
            Turn = game.PlayerToText(); //определение игрока
            ChangeImage(); //установка картинки игрока

            DefinePlayer newMenu = new DefinePlayer(game.PlayerToText()); //открытие формы с первым игроком
            Window firstFormWindow = Application.Current.Windows.OfType<Window>().FirstOrDefault(); //определяем открытую форму, чтобы новую расположить по центру
            newMenu.Owner = firstFormWindow;
            newMenu.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            bool? result = newMenu.ShowDialog();

            GameProcess(); //запуск игрового процесса
        }

        private async void GameProcess()
        {
            while (game.Gameover())
            {
                Turn = game.PlayerToText(); //определение игрока
                game.Turn(); 
                await ChangeDiceNumbers(); //изменение значений на кубиках
                await Task.Delay(50);

                if (game.Sum == 0) FuturePos = "Пропуск хода";

                while (game.Sum > 0)
                {
                    chipPos = new TaskCompletionSource<bool>();
                    if (game.Player == 1) await ComputerTurn(); //ход компьютера
                    await chipPos.Task;
                    FuturePos = "";
                }
                await Task.Delay(300);
                ChangeImage(); //смена картинки
            }

            game.Player = game.Winner();
            Congrats newWinner = new Congrats(game.PlayerToText()); 
            Window firstFormWindow = Application.Current.Windows.OfType<Window>().FirstOrDefault(); //определяем открытую форму, чтобы новую расположить по центру
            newWinner.Owner = firstFormWindow;
            newWinner.WindowStartupLocation = WindowStartupLocation.CenterOwner; //расположение по центру
            bool? result = newWinner.ShowDialog();
            if (result == true) Start();
        }
        private async Task ComputerTurn()
        {
            isComputer = true;
            int[] mc = game.MonteCarlo(); //запуск Monte Carlo
            int index = mc[0]; //индекс, который даст результат                 
            int computerMove = mc[1]; //ход
            Chips chip = chipDictionary[index][chipDictionary[index].Count() - 1]; //поиск фишки
            chipDictionary[index].RemoveAt(chipDictionary[index].Count() - 1); //удаление фишки с текущего места

            pos = game.CanMove(positions.FindPosition(chip.Left, chip.Top)); //определение возможных позиций фишки

            chip.PrevPosition = new Point(chip.Left, chip.Top); //задание нового предыдущего значения для фишки


            await ChangePosition(chip, positions.X[computerMove], positions.Y[computerMove]); //перестановка фишки

            if (!chipDictionary.ContainsKey(computerMove)) //доавление в словарь нового положения
            {
                chipDictionary[computerMove] = new List<Chips>();
            }

            chipDictionary[computerMove].Add(chip); //обновление словаря
            await Task.Delay(200);
            isComputer = false;
        }

        public string Turn
        {
            get => _turn;
            set
            {
                if(_turn != value)
                {
                    _turn = value;
                    OnPropertyChanged();
                }
            }
        }

        public string FuturePos
        {
            get => _futurePos;
            set
            {
                if (_futurePos != value)
                {
                    _futurePos = value;
                    OnPropertyChanged(nameof(FuturePos));
                }
            }
        }

        public string Textondice1
        {
            get => _textondice1;
            set
            {
                if (_textondice1 != value)
                {
                    _textondice1 = value;
                    OnPropertyChanged(nameof(Textondice1));
                }
            }
        }

        public string Textondice2
        {
            get => _textondice2;
            set
            {
                if (_textondice2 != value)
                {
                    _textondice2 = value;
                    OnPropertyChanged(nameof(Textondice2));
                }
            }
        }

        public ImageSource playerImage
        {
            get => _playerImage;
            set
            {
                if (_playerImage != value)
                {
                    _playerImage = value;
                    OnPropertyChanged(nameof(playerImage));
                }
            }
        }

        //кручение кубика
        private async Task ChangeDiceNumbers()
        {
            isDiceRolling = false;
            Random random = new Random();
            int num = random.Next(7, 11);

            for (int i = 0; i < num; i++)
            {
                //пауза в 0,1 секунды
                await Task.Delay(100);
                int number1 = random.Next(1, 7);
                int number2 = random.Next(1, 7);
                Textondice1 = number1.ToString();
                Textondice2 = number2.ToString();
            }
            Textondice1 = game.Dices[0].ToString();
            Textondice2 = game.Dices[1].ToString();
            await Task.Delay(100);
            isDiceRolling = true;
        }

        private void SaveInitialPositions()
        {
            for(int i = 0; i < 30; i++)
            {
                if(i<15) _chips.Add(new Chips(this) { Left = 682, Top = 78 + 10 * i, InitialPosition = new Point(682, 78 + 10 * i), PrevPosition = new Point(682, 78 + 10 * i), Color = "white" }); // Начальные координаты
                else _chips.Add(new Chips(this) { Left = 284, Top = 471 - 10 * (i%15), InitialPosition = new Point(284, 471 - 10 * (i % 15)), PrevPosition = new Point(284, 471 - 10 * (i % 15)),  Color = "black" }); // Начальные координаты
            }
        }
        private void ChangeImage()
        {
            if (game.Player == 1) playerImage = new BitmapImage(new Uri("photo/comp.png", UriKind.Relative)); //UriKind.Relative указывает, что путь к изображению является относительным
            else playerImage = new BitmapImage(new Uri("photo/person.png", UriKind.Relative));
        }

        private void ResetChipsToInitialPositions()
        {
            foreach (var chip in _chips)
            {
                chip.Left = chip.InitialPosition.X;
                chip.Top = chip.InitialPosition.Y;
                chip.ZIndex = 0;
            }
        }
        public string PosToString(List<int> pos)
        {
            pos.Sort();
            pos.Reverse();
            string ans = "";
            foreach (int p in pos)
            {
                if (p == 24) ans += "сброс ";
                else ans += (p + 1).ToString() + " ";
            }
            return ans;
        }

        private void Start()
        {
            if(isDiceRolling)
            {
                if (isStarted)
                {
                    NewGame newGame = new NewGame();
                    Window firstFormWindow = Application.Current.Windows.OfType<Window>().FirstOrDefault(); //определяем открытую форму, чтобы новую расположить по центру
                    newGame.Owner = firstFormWindow;
                    newGame.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    bool? result = newGame.ShowDialog();

                    if (result == true)
                    {
                        ResetChipsToInitialPositions();
                        StartGame();
                    }
                }
                else
                {
                    isStarted = true;
                    StartGame();
                }
            }  
        }
    }
}
