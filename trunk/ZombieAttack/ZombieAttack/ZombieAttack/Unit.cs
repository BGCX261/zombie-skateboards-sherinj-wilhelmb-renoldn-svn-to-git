using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace ZombieAttack
{
    class Unit
    {
        public Model model;
        public Vector3 position;
        public int health;

        public Unit(Model m, Vector3 pos, int h)
        {
            model = m;
            position = pos;
            health = h;
        }

    }
}
