using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backgammon
{
    public class Positions //Клетки для фишек на поле
    {
        int[] _x = new int[25]; //положения x
        int[] _y = new int[25]; //положения y
        List<int[]> _divisions = new List<int[]>(); //деления поля [0] - левая граница, [1] - правая граница, [3] - верхняя граница, [4] - нижняя граница

        public List<int[]> Divisions { get { return _divisions; } }
        public int[] X { get { return _x; } set { _x = value; } }
        public int[] Y { get { return _y; } set { _y = value; } }

        public Positions() //конструктор
        {
            double k = 0;
            for (int i = 0; i < 24; i++)
            {
                _divisions.Add(new int[4]); 
            }

            for ( int i = 0; i < 12; i++) //сопоставление клеток с конкретным полем
            {
                _x[23-i] = 682 - (int)Math.Round(34 *k);
                _y[23 - i] = 78;
                _x[i] = 682 - (int)Math.Round(34 * k);
                _y[i] = 471;

                _divisions[23 - i] = new int[4] {_x[23 - i]- 20, _x[23 - i] + 20, 248, 58};
                _divisions[i] = new int[4] { _x[i] - 20, _x[i] + 20, 491, 301 };
                k++;
                if (k == 6) k += 24.0 / 34.0;
            }
            _x[24] = 755;
            _y[24] = 175;
        }

        public int FindPosition(double x, double y) //поиск индекса согласно переданным координатам
        {
            for (int i = 0; i < _divisions.Count; i++)
            {
                if (x >= _divisions[i][0] && x <= _divisions[i][1] && y <= _divisions[i][2] && y >= _divisions[i][3])  return i;
            }
            return -1; //фишка за пределами поля
        }
    }
}
