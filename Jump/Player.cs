using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jump
{
    public class Player
    {
        public int health;

        public Player(int health)
        {
            this.health = health;
        }

        public int TakeDmg(int damage)
        {
            health -= damage;
            if (health < 0) health = 0;
            return health;
        }
        public int GetHealth()
        {
            health++;
            return health;
        }
        

    }
}