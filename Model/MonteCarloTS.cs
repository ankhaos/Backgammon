using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backgammon
{
    internal class MonteCarloTS
    {
        Node _root; //корень дерева узлов
        public MonteCarloTS(GameModel initialState)
        {
            _root = new Node(initialState);
        }

        public void PerformSearch(int iterations)
        {
            _root.Expand();
            for (int i = 0; i < iterations; i++)
            {
                Node node = _root;

                //выбор
                if (node.Children.Count > 0) //есть дочерние узлы
                {
                    node = node.SelectChild();
                }

                node.Expand(); //расширение

                //if (node.Children.Count == 0) // Если у текущего узла нет дочерних узлов
                //{
                //    Node expandedNode = node.Expand(); // Расширяем текущий узел
                //    if (expandedNode != null) // Если удалось расширить узел
                //    {
                //        node = expandedNode; // Переходим к расширенному узлу
                //    }
                //}

                //моделирование
                int result = node.Simulate();

                //обратное распространение
                while (node != null)
                {
                    node.Backpropagation(result);
                    node = node.Parent;
                }
            }
        }

        public Node GetBestMove()
        {
            //возвращает дочерний узел с наибольшим количеством побед
            return _root.Children.OrderByDescending(c => c.WinCount).FirstOrDefault();
        }
    }
}