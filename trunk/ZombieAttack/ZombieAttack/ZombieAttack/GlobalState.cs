using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZombieAttack
{
    class GlobalState
    {
        //Camera constants
        public const float NEAR_CLIP = 1.0f;
        public const float FAR_CLIP = 10000.0f;
        public const float VIEW_ANGLE = 45.0f;

        //player constants
        public const int MAX_PLAYER_HEALTH = 100;
        public const int MAX_RANGE = 400;
        public const float PLAYER_TURN_SPEED = 1.0f;
        public const float PLAYER_VELOCITY = 0.75f;

        //zombie constants
        public const int MAX_ZOMBIE_HEALTH = 3;

        //sound constants
        public const float BACKGROUND_MUSIC_VOLUME = 0.3f;
        public const float GUN_SOUND_VOLUME = 0.2f;


        //Generic
        public const int MIN_DISTANCE = 20;
        public const int MAX_DISTANCE = 300;
        public const int MAX_TERRAIN_RANGE = 400;
        public const float WALL_LENGTH = 210.0f;
        public const float WALL_SHIFT = 300.0f; //how much to shift walls so they connect
        public const float WALL_SCALEX = 15.0f;
        public const int NUM_PARTICLES = 50;
        public const int NUM_TREES = 30;
        //public const int NUM_ZOMBIES = 10;
        public const int WIN_NUM_ZOMBIES = 30;
        public const int COLLISION_DISTANCE = 8;
        public static readonly TimeSpan ROUND_TIME = TimeSpan.FromSeconds(800.25);
        public const string STR_TIME_LEFT = "Time Left: ";
        public const string STR_KILL_COUNT = "Zombies Killed: ";
        public const string STR_PAUSE =
            "Press Enter to resume play!";
        public const int KILL_COUNT = 0;
        public const string STR_WIN = "Game Won !";
        public const string STR_LOSE = "Game Lost !";
        public const string STR_CONTINUE =
           "Press Enter/Start to continue or Esc/Back to quit";
        public const string STR_RESTART =
            "Press Enter/Start to play again or Esc/Back to quit";

        public const float PLAYER_BOUND_SPHERE_FACTOR = .7f;
        public const float ZOMBIE_BOUND_SPHERE_FACTOR = .5f;
        public const float TREE_BOUND_SPHERE_FACTOR = .7f;


    }
}
