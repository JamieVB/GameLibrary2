using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Game_Library_v2
{
    class Menu
    {
        private List<MenuItem> menuItems = new List<MenuItem>();
        private SpriteFont font;

        public Menu(ContentManager Content)
        {
            font = Content.Load<SpriteFont>("Arial14");

            menuItems.Add(new MenuItem("Games", (menuItems.Count)));
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (MenuItem menuItem in menuItems)
            {
                spriteBatch.DrawString(font, (menuItem.text + " - " + menuItem.id), new Vector2(20, 20), Color.White);
            }
        }

        private class MenuItem
        {
            public string text { get; set; }
            public string desc { get; set; }
            public int id { get; set; }
            public int parent { get; set; }

            public MenuItem(string newText, int newID, int newParent = -1)
            {
                id = newID;
                text = newText;
                parent = newParent;
            }
        }
    }
}
