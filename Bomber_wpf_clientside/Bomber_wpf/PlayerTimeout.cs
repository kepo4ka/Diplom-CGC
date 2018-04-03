using ClassLibrary_CGC;

namespace Bomber_wpf
{
    class PlayerTimeout
    {
        public Player player;
        public int timeout;

        public PlayerTimeout(Player pplayer, int ptimeout = 0)
        {
            player = pplayer;
            timeout = ptimeout;
        }


        public Player Player
        {
            get
            {
                return player;
            }

            set
            {
                player = value;
            }
        }

        public int Timeout
        {
            get
            {
                return timeout;
            }
            set
            {
                if (value>=0)
                {
                    timeout = value;
                }
            }
        }        
    }
}
