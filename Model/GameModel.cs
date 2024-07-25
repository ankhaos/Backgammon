using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

namespace Backgammon
{
    public class GameModel
    {
        int[,] _gamefield = new int[24, 2]; //игровое поле
        int[] _dices = new int[2] {0,0}; //кубики
        int _player; //кто игрок
        int _sum; //количество ходов
        List<int> _movings; //величины перемещений
        Random random = new Random();
        int _count24; //проверка головы
        int[] _mirrored = new int[24] { 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }; //зеркальные индексы, чтобы алгоритмы работали для двух противоположных игроков

        public GameModel() //конструктор
        {
            for (int i = 0; i < 23; i++)
            {
                _gamefield[i, 0] = -1;
            }
            // 0 белые; -1 пустые; 1 черные
            _gamefield[23, 0] = 0;
            _gamefield[23, 1] = 15; //все белые фишки на 24 месте
            _gamefield[11, 0] = 1;
            _gamefield[11, 1] = 15; //все черные фишки на 12 месте
            while (_dices[0] == _dices[1])
            {
                _dices[0] = random.Next(1, 7); //генерация кубика для компьютера
                _dices[1] = random.Next(1, 7); //генерация кубика для игрока
            }
            _player = _dices[0] < _dices[1] ? 0 : 1; // 0 - белые, 1 - черные
        }

        public int[,] Gamefield { get { return _gamefield; } }
        public int[] Dices { get { return _dices; } }
        public int Player { get { return _player; } set { _player = value; } }
        public int Sum{ get { return _sum; } }

        public void Turn() //ход 
        {
            _sum = 0;
            _count24 = 0;
            _movings = new List<int>();

            //генерация кубиков
            _dices[0] = random.Next(1, 7);
            _dices[1] = random.Next(1, 7);
            if (_dices[0] == _dices[1]) {_dices[0] = random.Next(1, 7); _dices[1] = random.Next(1, 7); } //против частых дублей
            _sum = _dices[0] + _dices[1];

            _movings.Add(_dices[0]); //добавление всех возможных перемещений в список 
            _movings.Add(_dices[1]);
            _movings.Add(_sum);

            if (_dices[0] == _dices[1]) //если дубль, то ход удваивается
            {
                if (new List<int> () { 3,4,6}.Contains(_dices[0]) && _player == 0 && _gamefield[23, 1] == 15 || _player == 1 && _gamefield[11, 1] == 15) _count24 = -1;
                _sum *= 2;
                _movings.Add(_dices[0] * 3);
                _movings.Add(_sum);
            }
            if (!CheckCourse()) _sum = 0; //если в связи с расстановкой фишек на поле ход с такими данными не получается сделать, игрок пропускает ход
        }

        public List<int> CanMove(int index) //поиск позиций, на которые можно двигаться
        {
            List<int> positions = new List<int>();
            
            index = Mirrored(index, _player); //для компьютерного игрока поле зеркально отображается

            if (index == 23 && _count24 == 1) return positions; //за ход с головы доступна только одна фишка, поэтому для второй нет возможности двигаться

            HashSet<int> ind = new HashSet<int>();

            foreach(int move in _movings)
            {
                ind.Add(index - move); //возможные ходы
                if (index - move < 0 && move > _dices[1] && move > _dices[0]) ind.Remove(index - move); //при сбросе сумма не учитывавется
            }

            ind.Remove(index); //изначальное местоположение нам не нужно

            foreach (int i in ind)
            {
                int pos = MirroredPos(i, _player); //отзеркаленные значения конвертируются в реальное местоположение
                if (pos >= 0 && (_gamefield[pos, 0] == -1 || (_gamefield[pos, 1] < 6 && _gamefield[pos, 0] == _player)) && CheckRow(pos, index)) //соответствие игроку и правилам
                {
                    positions.Add(pos); //добавление возможных позиций в список
                }
                if (pos < 0 && CheckHome() && !positions.Contains(24)) positions.Add(24); //сброс
            }
            return positions;
        }

        public void Move(int index, int prevind) //фиксирование фишки
        {
           
            _gamefield[prevind, 1] -= 1; //удаление фишки с предыдущей позиции
            _movings.Sort();
           
            if (Mirrored(prevind, _player) == 23) _count24 += 1; //блокировка головы

            if (_gamefield[prevind, 1] == 0) _gamefield[prevind, 0] = -1; //поле без фишки

            prevind = Mirrored(prevind, _player);
            if (index < 24) //не в сброс
            {
                _gamefield[index, 1] += 1; //добавление фишки на новую позицию
                if (_player == 0) { _gamefield[index, 0] = 0; } else { _gamefield[index, 0] = 1; } //фиксация игрока на позиции
                index = Mirrored(index, _player);
            }
            else //в сброс 
            {
                if (_movings[0] >= prevind + 1) index = prevind - _movings[0]; 
                else if(_movings[1] >= prevind + 1) index = prevind - _movings[1];
            }


            if (_dices[0] != _dices[1]) _movings.Remove(prevind - index); //удаление сделанного хода
            _sum -= prevind - index; //остаток ходов в сумме
            _movings.RemoveAll(m => m > _sum); //удаление всех невозможных передвижений
            _movings.Add(_sum);

            if (!CheckCourse()) _sum = 0; //проверка, что ходы остались

            //переключение игрока
            if (_sum == 0) _player = (_player + 1) % 2; //сумма стала 0 - переключение игрока
        }

        private bool CheckHome() //проверка возможности заведения в сброс
        {
            int s = 0;
            for(int i = 23; i >= 6; i--)
            {
                int pos = Mirrored(i, _player);
                if(_gamefield[pos, 0] == _player) s += _gamefield[pos, 1];
            }
            if (s > 0) return false;
            else return true;
        }

        private bool CheckRow(int ind, int prevind) //Вы имеете право выстроить заграждение из шести шашек только в том случае, если хотя бы одна шашка противника находится в его доме. 
        {
            int i = 0;
            ind = Mirrored(ind, _player);
            bool flag = true; //фишка отсутствует в доме

            while (i <= 5 && flag) //проверка наличия фишки соперника в доме
            {
                if (_gamefield[Mirrored(i, (_player + 1) % 2), 0] == (_player + 1) % 2) flag = false; //в доме соперника фишка есть
                i += 1;
            }

            int summ = 1;
            for (int direction = -1; direction <= 1; direction += 2) //-1 - влево, +1 - вправо
            {
                i = ind + direction; //начало с соседней позиции
                while (i >= 0 && i < 24)
                {
                    int pos = MirroredPos(i, _player);
                    if (_gamefield[pos, 0] == _player && (_gamefield[pos, 1] > 1 && i == prevind || i != prevind)) summ++;
                    else break; //остановка подсчета
                    i += direction; //переход к следующей позиции в том же направлении
                }
            }

            if (summ == 6 && flag || summ > 6) return false; //больше 6 фишек в ряд нельзя в любом случае
            else return true;
        }

        public string PlayerToText() //конвертирует числовое значение игрока в строковое
        {
            if (_player == 1) return "Компьютер";
            else return "Вы";
        }

        public bool Gameover() //проверка окончания игры
        {
            int sumw = 0;
            int sumb = 0;
            for(int i = 0; i<24;i++)
            {
                sumw += _gamefield[i,0] == 0 ? _gamefield[i, 1] : 0;
                sumb += _gamefield[i, 0] == 1 ? _gamefield[i, 1] : 0;
            }
            if (sumw != 0 && sumb != 0) return true; //игра продолжается
            else return false;  //игра закончена
        }

        public int Winner()
        {
            int sumw = 0;
            int sumb = 0;
            for (int i = 0; i < 24; i++)
            {
                sumw += _gamefield[i, 0] == 0 ? _gamefield[i, 1] : 0;
                sumb += _gamefield[i, 0] == 1 ? _gamefield[i, 1] : 0;
            }
            if (sumw == 0) return 0;
            else return 1;
        }

        private bool CheckCourse() //проверка возможности сделать ход за текущего игрока
        {
            if (_sum <= 0) return false; 
            
            List<int> pos = new List<int>();
            for (int i = 0; i < 24; i++)
            { 
                if (_gamefield[i, 0] == -1 || _gamefield[i, 0] != _player) continue; //пустое или чужое поле не рассматривается
                pos.AddRange(CanMove(i)); //добавление ходов фишки
            }
            if (pos.Count() > 0) return true;
            else return false;
        }

        private int Mirrored(int pos, int player) //для второго игрока меняются индексы в соответсвии с логикой игры (будто черный меняется местами с белым)
        {
            if(player == 1)
            {
                pos = _mirrored[pos];
            }
            return pos;
        }
        private int MirroredPos(int pos, int player) //находится фактический индекс второго игрока (зеркально отражаем)
        {
            if(player == 1)
            {
                pos = Array.IndexOf(_mirrored, pos);
            }
            return pos;
        }

        public List<Move> GetPossibleMoves()  //возможные ходы
        {
            List<Move> possibleMoves = new List<Move>();

            for(int i = 0; i < 24; i++)
            {
                if (_gamefield[i,0] == _player)
                {
                    List<int> pos;
                    pos = CanMove(i);
                    if (pos.Count() > 0) possibleMoves.Add(new Move(pos,i)); // Вычисляет возможные ходы
                }
            }

            return possibleMoves;
        }
        public GameModel Clone() // Клонирование текущей игры
        {
            return new GameModel
            {
                _gamefield = (int[,])_gamefield.Clone(),
                _dices = (int[])_dices.Clone(),
                _player = _player,
                _sum = _sum,
                _movings = new List<int>(_movings),
                _count24 = _count24,
                _mirrored = (int[])_mirrored.Clone()
            };
        }

        public int[] MonteCarlo()
        {
            GameModel initialState = Clone();  //создание текущего состояния игры
            MonteCarloTS mcts = new MonteCarloTS(initialState);

            mcts.PerformSearch(700); //запуск поиска с заданным количеством итераций

            Node bestMoveNode = mcts.GetBestMove(); //получение лучшего хода

            GameModel bestMoveState = bestMoveNode.State; //получения состояния игры, соответствующего лучшему ходу

            bestMoveState.Player = 1; //ход компьютера


            int index = -1;
            int prevind = -1; 
            for(int i = 0; i<24; i++) //определение индексов лучшего хода (сравнение текущего состояния игры с полученным состоянием, в котором сделан лучший ход)
            {
                if (initialState.Gamefield[i, 0] == 1 && initialState.Gamefield[i, 1] > bestMoveState.Gamefield[i, 1])
                {
                    prevind = i; //поиск индекса, из которого делается ход
                }
                else if (bestMoveState.Gamefield[i, 0] == 1 && initialState.Gamefield[i, 1] < bestMoveState.Gamefield[i, 1])
                {
                    index = i; //поиск индекса, в который был сделан ход
                }
                if (i == 23 && index == -1 && bestMoveState.CheckHome()) index = 24; //если ход в сброс
            }

            return new int[2] {prevind, index };
        }
    }
}

