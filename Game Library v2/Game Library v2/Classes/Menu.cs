using System;
using System.Linq;
using System.Xml.Linq;
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

            SelRec.Load(device);

            LoadMenuItems();
        }

        static private void LoadMenuItems()
        {
            XDocument xml = XDocument.Load("menu.xml");
            List<string> text = xml.Descendants("Game").Select(x => x.Value).ToList();
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
            bool mouseOverItem = false;

            //check each shown menu item to see if the mouse was over it
            foreach (MenuItem item in menuItems.Where(x => x.IsShown()))
            {
                buttonArea = new Rectangle((int)item.pos.X - PADDING/2, (int)item.pos.Y - PADDING/2, BUTTON_WIDTH + PADDING, BUTTON_HEIGHT + PADDING);
                if (buttonArea.Contains(new Point(mouseState.X, mouseState.Y)))
                {
                    mouseOverItem = true;
                    SelRec.Update(item);
                }
            }
            if (!mouseOverItem)
                SelRec.Update();

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
            //Draw the highlight and selection rectangles
            SelRec.Draw(spriteBatch);

            //draw the first level of the menu
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
            public const float SPEED = 750;
            static public Vector2 destination;
            static public int state, offScreen;
            static public Rectangle recHighlight;
            static public Texture2D texWhite;

            /// <summary>
            /// Set initial values and stuff
            /// </summary>
            /// <param name="device">Graphics device</param>
            static public void Load(GraphicsDevice device)
            {
                state = STATE_STOPPED;
                offScreen = device.PresentationParameters.BackBufferHeight;
                destination = new Vector2(0, offScreen);
                recHighlight = new Rectangle(0, (int)offScreen, MARGIN + BUTTON_WIDTH + PADDING / 2 + 1, BUTTON_HEIGHT + PADDING);
                texWhite = new Texture2D(device, 1, 1);
                texWhite.SetData(new[] { Color.White });
            }

            /// <summary>
            /// Update the position and destination of the highlight rectangle
            /// </summary>
            /// <param name="item">The item the mouse is currently over</param>
            static public void Update(MenuItem item = null)
            {
                UpdateDestination(item);
                UpdatePosition();
            }

            /// <summary>
            /// Update the destination of the highlight rectangle
            /// </summary>
            /// <param name="item">The item the mouse is currently over</param>
            static private void UpdateDestination(MenuItem item)
            {
                //if the mouse isn't over any items, move the highlight rec off the screen
                if (item == null)
                {
                    destination = new Vector2(recHighlight.X, offScreen);
                }
                //else if the rectangle is on the same column as the item the mouse is over, just move the rectangle up or down
                else if (recHighlight.X < item.pos.X && (recHighlight.X + BUTTON_WIDTH + PADDING) > item.pos.X)
                {
                    destination = new Vector2(recHighlight.X, item.pos.Y - PADDING / 2);
                }
                //else if the rectangle is already offscreen, move it to the proper column, so the previous condition will start to move it to the right place
                else if (recHighlight.Y >= offScreen)
                {
                    //since we're switching columns here, this is the best place to change the width as well
                    if (item.parent == -1)
                    {
                        recHighlight.X = 0;
                        recHighlight.Width = MARGIN + BUTTON_WIDTH + PADDING / 2 + 1;
                    }
                    else
                    {
                        recHighlight.X = (int)item.pos.X - PADDING / 2;
                        recHighlight.Width = BUTTON_WIDTH + PADDING;
                    }
                }
                //else if the rectangle is just in the wrong column, move it offscreen so the previous condition can move it to the correct column
                else
                {
                    destination = new Vector2(recHighlight.X, offScreen);
                }
            }

            /// <summary>
            /// Update the position of the highlight rectangle, based on its destination
            /// </summary>
            static private void UpdatePosition()
            {
                //if the position is close to the destination, just move it straight there
                if (recHighlight.Y - destination.Y < SPEED / 60 && recHighlight.Y - destination.Y > -SPEED / 60)
                {
                    recHighlight.Y = (int)destination.Y;
                }
                else
                {
                    //otherwise, just move it up or down accordingly
                    if (recHighlight.Y > destination.Y)
                    {
                        recHighlight.Y -= (int)(SPEED / 60);
                    }
                    else
                    {
                        recHighlight.Y += (int)(SPEED / 60);
                    }
                }
            }

            /// <summary>
            /// Draw the selection and highlight rectangles
            /// </summary>
            /// <param name="spriteBatch">Sprite batch</param>
            static public void Draw(SpriteBatch spriteBatch)
            {
                DrawHighlightBox(spriteBatch);
                DrawSelectBox(spriteBatch);
            }

            /// <summary>
            /// Draw a rectangle under any selected menu items
            /// </summary>
            /// <param name="spriteBatch">Sprite batch</param>
            static public void DrawSelectBox(SpriteBatch spriteBatch)
            {
                int left, top, width, height;

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

                        spriteBatch.Draw(texWhite, new Rectangle(left, top, width, height), new Color(204, 85, 0) * 0.8f);
                    }
                }
            }

            /// <summary>
            /// Draw the highlight rectangle wherever it's supposed to be
            /// </summary>
            /// <param name="spriteBatch">Sprite batch</param>
            static public void DrawHighlightBox(SpriteBatch spriteBatch)
            {
                spriteBatch.Draw(texWhite, recHighlight, new Color(0, 64, 128) * 0.8f);
            }
        }
    }
}
