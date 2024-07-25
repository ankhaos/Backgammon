using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Backgammon
{
    internal class Node // узел дерева для MCTS
    {
        List<Node> _children; // cписок дочерних узлов, представляющих возможные ходы из текущего состояния игры
        int _visitCount; // количество раз, когда этот узел был посещен при выполнении MCTS
        int _winCount; // количество побед, которые были получены из этого узла
        Node _parent; // родительский узел
        GameModel _state; // текущее состояние игры

        public Node(GameModel state)
        {
            _children = new List<Node>();
            _visitCount = 0;
            _winCount = 0;
            _parent = null;
            _state = state;
        }

        public List<Node> Children { get { return _children; } set { _children = value; } }
        public int VisitCount { get { return _visitCount; } set { _visitCount = value; } }
        public int WinCount { get { return _winCount; } set { _winCount = value; } }
        public Node Parent { get { return _parent; } set { _parent = value; } }
        public GameModel State { get { return _state; } set { _state = value; } }

        
        public void AddChild(Node child) // Метод для добавления дочернего узла
        {
            _children.Add(child);
            child.Parent = this;
        }

        // Метод для выбора лучшего дочернего узла на основе UCB (Upper Confidence Bounds)
        public Node SelectChild() 
        {
            double totalVisits = (double)_visitCount; //количество посещений родительского узла
            double c = 1.41; // Константа для UCB

            return _children.OrderByDescending(child =>
            {
                if (child.VisitCount == 0)
                {
                    return double.PositiveInfinity;
                }
                double childValue = (double)child.WinCount / (double)child.VisitCount;
                double exploration = c * Math.Sqrt(Math.Log(totalVisits) / child.VisitCount);
                return childValue + exploration;
            }).FirstOrDefault(); // Возвращает дочерний узел с наибольшей оценкой UCT
        }

        // Метод для расширения дерева
        public void Expand()  // Добавляет новый дочерний узел
        {
            List<Move> possibleMoves = _state.GetPossibleMoves();
            foreach (Move chip in possibleMoves)
            {
                foreach (int move in chip.Moves)
                {
                    GameModel newState = _state.Clone(); // Создаем копию текущего состояния
                    newState.Move(move, chip.Chip); // Применяем ход к новому состоянию (move - новое значение, chip - значение, откуда был совершен ход)
                    Node newNode = new Node(newState); // Создаем новый узел с новым состоянием
                    AddChild(newNode);
                }
            }
            //return _children.Count > 0 ? _children[0] : null; // Возвращаем первый добавленный узел или null, если узлов нет
        }

        // Метод для симуляции игры из текущего состояния (Создание копии игры, выбор случайных ходов из доступных для каждого игрока, определение победителя)
        public int Simulate()  // Возвращает результат игры (1 для победы, 0 для поражения)
        {
            GameModel simulationState = _state.Clone(); // Создаем копию текущего состояния для симуляции

            while (simulationState.Gameover())
            {
                simulationState.Turn(); // Генерация ходов
                Random random = new Random();
                while (simulationState.Sum > 0)
                {
                    List<Move> possibleMoves = simulationState.GetPossibleMoves();  // Получаем список возможных ходов для текущего игрока
                    Move randomMove = possibleMoves[random.Next(possibleMoves.Count())]; // Выбираем случайный ход из доступных
                    simulationState.Move(randomMove.Moves[random.Next(randomMove.Moves.Count())], randomMove.Chip); // Выполняем случайный ход
                }
            }
            
            // Игра окончена, определяем победителя
            if (simulationState.Winner() == 1) return 1;
            else return 0; 
        }

        // Метод для обновления статистики узла
        public void Backpropagation(int result)
        {
            _visitCount++;
            _winCount += result;
        }
    }
}

