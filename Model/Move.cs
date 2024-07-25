using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backgammon
{
    public class Move //ходы для фишки
    {
        List<int> _moves; // возможные ходы
        int _chip; // фишка, из которой возможен ход

        public Move(List<int> moves, int chip)
        {
            _moves = moves;
            _chip = chip;
        }

        public List<int> Moves { get { return _moves; } set { _moves = value; } }
        public int Chip { get { return _chip; } set { _chip = value; } }
    }
}
