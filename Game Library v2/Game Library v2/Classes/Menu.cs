using System;
using System.Linq;
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
    static class Menu
    {
        static private List<MenuItem> menuItems = new List<MenuItem>();
        static private SpriteFont font;
        private const int MARGIN_LEFT = 60, MARGIN_BOTTOM = 80, BUTTON_HEIGHT = 35, BUTTON_WIDTH = 75;

        //called from LoadContent, to load whatever is required for the menu
        static public void Load(ContentManager Content)
        {
            font = Content.Load<SpriteFont>("Arial14");

            menuItems.Add(new MenuItem("Games", menuItems.Count));
            menuItems.Add(new MenuItem("Music", menuItems.Count));
            menuItems.Add(new MenuItem("Video", menuItems.Count));
            menuItems.Add(new MenuItem("Portal 2", menuItems.Count, 0));
            menuItems[0].selected = true;
        }

        //draw the menu
        static public void Draw(SpriteBatch spriteBatch, Rectangle recScreen)
        {
            int posY = recScreen.Height - MARGIN_BOTTOM;

            //first, draw the first level of the menu
            DrawLevel(spriteBatch, recScreen, -1);
            //then find each item that is selected, and draw their children
            foreach (MenuItem menuItem in menuItems)
            {
                if (menuItem.selected == true)
                {
                    DrawLevel(spriteBatch, recScreen, menuItem.id);
                }
            }
        }

        //draw a level of the menu, depending on what parent is passed
        static private void DrawLevel(SpriteBatch spriteBatch, Rectangle recScreen, int parentToDraw)
        {
            int posY = recScreen.Height - MARGIN_BOTTOM;

            //find how many levels are above this one, to know horizontal position

            for (int i = menuItems.Count() - 1; i >= 0; i--)
            {
                if (menuItems[i].parent == parentToDraw)
                {
                    spriteBatch.DrawString(font, menuItems[i].text, new Vector2(MARGIN_LEFT, posY), Color.White);
                    posY -= BUTTON_HEIGHT;
                }
            }
        }

        //data for each menu item
        private class MenuItem
        {
            public string text { get; set; }
            public string desc { get; set; }
            public int id { get; set; }
            public int parent { get; set; }
            public bool selected { get; set; }

            public MenuItem(string newText, int newID, int newParent = -1)
            {
                id = newID;
                text = newText;
                parent = newParent;
                desc = "";
                selected = false;
            }
        }
    }
}
