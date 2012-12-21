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
        private const int MARGIN = 60, BUTTON_HEIGHT = 20, BUTTON_WIDTH = 100, PADDING = 15;

        
        /// Called from LoadContent, to load whatever is required for the menu
        /// </summary>
        /// <param name="Content">The content manager.</param>
        /// <param name="device">Graphics device.</param>
        static public void Load(ContentManager Content, GraphicsDevice device)
        {
            font = Content.Load<SpriteFont>("Arial14");

            menuItems.Add(new MenuItem("Games"));
            menuItems.Add(new MenuItem("Music"));
            menuItems.Add(new MenuItem("Video"));
            menuItems.Add(new MenuItem("Portal 2", FindIndex("Games")));
            menuItems.Add(new MenuItem("Bastion", FindIndex("Games")));
            menuItems.Add(new MenuItem("Play", FindIndex("Portal 2")));
            menuItems.OrderBy(x => x.parent);
            AssignPositions(device);

            SelRec.offScreen = new Vector2(0, device.PresentationParameters.BackBufferHeight);
        }

        /// <summary>
        /// Assign initial positions to the menu items
        /// </summary>
        /// <param name="device">Graphics device.</param>
        static private void AssignPositions(GraphicsDevice device)
        {
            var groupedMenu = menuItems.GroupBy(menuItem => menuItem.parent);
            Vector2 currentPos;

            foreach (var group in groupedMenu)
            {
                //the starting position
                currentPos = new Vector2();
                currentPos.X = MARGIN + ((BUTTON_WIDTH + PADDING) * FindLevel(group.ToArray()[0].parent));
                currentPos.Y = device.PresentationParameters.BackBufferHeight - MARGIN - ((BUTTON_HEIGHT + PADDING) * group.Count());
                //now iterate through each member of the group
                foreach (MenuItem menuItem in group)
                {
                    menuItem.pos = currentPos;
                    currentPos.Y += BUTTON_HEIGHT + PADDING;
                }
            }
        }

        /// <summary>
        /// Update the menu
        /// </summary>
        /// <param name="mouseState">Current mouse state</param>
        /// <param name="prevMouseState">Mouse state of previous frame</param>
        static public void Update(MouseState mouseState, MouseState prevMouseState)
        {
            CheckMouse(mouseState, prevMouseState);
        }

        
        /// <summary>
        /// Check if any menu items have been clicked on or moused over.
        /// </summary>
        /// <param name="mouseState">The current mouse state.</param>
        /// <param name="prevMouseState">The mouse state in the last frame.</param>
        static private void CheckMouse(MouseState mouseState, MouseState prevMouseState)
        {
            Rectangle buttonArea;

            //if mouse was just clicked
            if (mouseState.LeftButton == ButtonState.Pressed && prevMouseState.LeftButton == ButtonState.Released)
            {
                //check each shown menu item to see if the mouse was over it
                foreach (MenuItem menuItem in menuItems.Where(x => x.IsShown()))
                {
                    buttonArea = new Rectangle((int)menuItem.pos.X - PADDING/2, (int)menuItem.pos.Y - PADDING/2, BUTTON_WIDTH + PADDING/2, BUTTON_HEIGHT + PADDING/2);
                    if (buttonArea.Contains(new Point(mouseState.X, mouseState.Y)))
                    {
                        //if the clicked item was already selected, then unselect all items in the current level and all levels below
                        if (menuItem.selected)
                        {
                            for (int i = menuItems.FindIndex(x => x.parent == menuItem.parent); i < menuItems.Count(); i++)
                            {
                                menuItems[i].selected = false;
                            }
                        }
                        else    //if not, do the same, and then select only the clicked item
                        {
                            for (int i = menuItems.FindIndex(x => x.parent == menuItem.parent); i < menuItems.Count(); i++)
                            {
                                menuItems[i].selected = false;
                            }
                            menuItem.selected = true;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Draw the menu
        /// </summary>
        /// <param name="spriteBatch">Sprite batch</param>
        /// <param name="recScreen">A rectangle the exact size of the screen</param>
        /// <param name="device">Graphics device</param>
        static public void Draw(SpriteBatch spriteBatch, Rectangle recScreen, GraphicsDevice device)
        {
            //first, draw a rectangle under any menu items that are selected
            SelRec.DrawSelected(spriteBatch, device);

            //first, draw the first level of the menu
            DrawLevel(spriteBatch, recScreen, -1);
            //then find each item that is selected, and draw their children
            for (int i = 0; i < menuItems.Count(); i++)
            {
                if (menuItems[i].selected == true)
                {
                    DrawLevel(spriteBatch, recScreen, i);
                }
            }
        }

        /// <summary>
        /// Draw a level of the menu, depending on what parent is passed
        /// </summary>
        /// <param name="spriteBatch">Sprite batch</param>
        /// <param name="recScreen">A rectangle the exact size of the screen</param>
        /// <param name="parentToDraw">The parent of the level that needs to be drawn</param>
        static private void DrawLevel(SpriteBatch spriteBatch, Rectangle recScreen, int parentToDraw)
        {
            foreach (MenuItem menuItem in menuItems)
            {
                if (menuItem.parent == parentToDraw)
                {
                    spriteBatch.DrawString(font, menuItem.text, menuItem.pos, Color.White);
                }
            }
        }

        /// <summary>
        /// Find the index of an item in menuItems by the text property
        /// </summary>
        /// <param name="text">The search paramater</param>
        /// <returns>The index of the first item in the menu that matches the text paramater</returns>
        static private int FindIndex(string text)
        {
            return menuItems.FindIndex(menuItem => menuItem.text == text);
        }

        /// <summary>
        /// Find the level of an item, based on its parent
        /// </summary>
        /// <param name="parent">The parent</param>
        /// <returns>The level of the item</returns>
        static private int FindLevel(int parent)
        {
            int x = parent, i = 0;
            while (x != -1)
            {
                i++;
                x = menuItems[x].parent;
            }
            return i;
        }

        /// <summary>
        /// How many levels are open
        /// </summary>
        /// <returns>Open levels</returns>
        static private int CountOpenLevels()
        {
            return menuItems.FindAll(menuItem => menuItem.selected == true).Count();
        }

        /// <summary>
        /// Data for each menu item
        /// </summary>
        private class MenuItem
        {
            public string text { get; set; }
            public string desc { get; set; }
            public int parent { get; set; }
            public bool selected { get; set; }
            public Vector2 pos;

            public MenuItem(string newText, int newParent = -1)
            {
                text = newText;
                parent = newParent;
                desc = "";
                selected = false;
            }

            //derived bool showing whether this menu item should currently be shown
            public bool IsShown()
            {
                if (parent == -1)
                {
                    return true;
                }
                else if (menuItems[parent].selected == true)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Data and functions to handle the selection rectangle
        /// </summary>
        static class SelRec
        {
            public const int STATE_STOPPED = 0, STATE_MOVING = 1;
            public const int SPEED = 100;
            static public Vector2 offScreen;
            static public int state = STATE_STOPPED;

            /// <summary>
            /// Draw a rectangle on any selected menu items
            /// </summary>
            /// <param name="spriteBatch">Sprite batch</param>
            /// <param name="device">Graphics device</param>
            static public void DrawSelected(SpriteBatch spriteBatch, GraphicsDevice device)
            {
                int left, top, width, height;
                Texture2D whiteTex = new Texture2D(device, 1, 1);
                whiteTex.SetData(new[] { Color.White });

                foreach (MenuItem item in menuItems)
                {
                    if (item.selected == true)
                    {
                        if (item.parent == -1)
                        {
                            left = 0;
                            width = MARGIN + BUTTON_WIDTH + PADDING / 2 + 1;
                        }
                        else
                        {
                            left = (int)item.pos.X - PADDING / 2;
                            width = BUTTON_WIDTH + PADDING;
                        }

                        top = (int)item.pos.Y - PADDING / 2;
                        height = BUTTON_HEIGHT + PADDING;

                        spriteBatch.Draw(whiteTex, new Rectangle(left, top, width, height), new Color(204, 85, 0) * 0.8f);
                    }
                }
            }
        }
    }
}
