﻿#region license

// Copyright (C) 2020 ClassicUO Development Community on Github
// 
// This project is an alternative client for the game Ultima Online.
// The goal of this is to develop a lightweight client considering
// new technologies.
// 
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
// 
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
// 
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System.Collections.Generic;
using ClassicUO.Configuration;
using ClassicUO.Data;
using ClassicUO.Game.Data;
using ClassicUO.Game.GameObjects;
using ClassicUO.Game.Managers;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Input;
using ClassicUO.IO.Resources;
using ClassicUO.Network;
using ClassicUO.Renderer;
using ClassicUO.Resources;
using ClassicUO.Utility.Logging;
using Microsoft.Xna.Framework;

namespace ClassicUO.Game.UI.Gumps
{
    internal class TopBarGump : Gump
    {
        private TopBarGump() : base(0, 0)
        {
            CanMove = true;
            AcceptMouseInput = true;
            CanCloseWithRightClick = false;

            // little
            Add
            (
                new ResizePic(0x13BE)
                {
                    Width = 30, Height = 27
                }, 2
            );

            Add
            (
                new Button(0, 0x15A1, 0x15A1, 0x15A1)
                {
                    X = 5, Y = 3, ToPage = 1
                }, 2
            );


            // big
            UOTexture th1 = GumpsLoader.Instance.GetTexture(0x098B);
            UOTexture th2 = GumpsLoader.Instance.GetTexture(0x098D);

            int smallWidth = 50;

            if (th1 != null)
            {
                smallWidth = th1.Width;
            }

            int largeWidth = 100;

            if (th2 != null)
            {
                largeWidth = th2.Width;
            }

            int[][] textTable =
            {
                new[] { 0, (int) Buttons.Map },
                new[] { 1, (int) Buttons.Paperdoll },
                new[] { 1, (int) Buttons.Inventory },
                new[] { 1, (int) Buttons.Journal },
                new[] { 0, (int) Buttons.Chat },
                new[] { 0, (int) Buttons.Help },
                new[] { 1, (int) Buttons.WorldMap },
                new[] { 0, (int) Buttons.Info },
                new[] { 0, (int) Buttons.Debug },
                new[] { 1, (int) Buttons.NetStats },

                new[] { 1, (int) Buttons.UOStore },
                new[] { 1, (int) Buttons.GlobalChat }
            };

            string[] texts =
            {
                ResGumps.Map, ResGumps.Paperdoll, ResGumps.Inventory, ResGumps.Journal, ResGumps.Chat, ResGumps.Help,
                ResGumps.WorldMap, ResGumps.Info, ResGumps.Debug, ResGumps.NetStats, ResGumps.UOStore,
                ResGumps.GlobalChat
            };

            bool hasUOStore = Client.Version >= ClientVersion.CV_706400;

            ResizePic background;

            Add
            (
                background = new ResizePic(0x13BE)
                {
                    Height = 27
                }, 1
            );

            Add
            (
                new Button(0, 0x15A4, 0x15A4, 0x15A4)
                {
                    X = 5, Y = 3, ToPage = 2
                }, 1
            );

            int startX = 30;

            for (int i = 0; i < textTable.Length; i++)
            {
                if (!hasUOStore && i >= (int) Buttons.UOStore)
                {
                    break;
                }

                ushort graphic = (ushort) (textTable[i][0] != 0 ? 0x098D : 0x098B);

                Add
                (
                    new RighClickableButton(textTable[i][1], graphic, graphic, graphic, texts[i], 1, true, 0, 0x0036)
                    {
                        ButtonAction = ButtonAction.Activate,
                        X = startX,
                        Y = 1,
                        FontCenter = true
                    }, 1
                );

                startX += (textTable[i][0] != 0 ? largeWidth : smallWidth) + 1;
                background.Width = startX;
            }

            background.Width = startX + 1;

            //layer
            LayerOrder = UILayer.Over;
        }

        public bool IsMinimized { get; private set; }

        public static void Create()
        {
            TopBarGump gump = UIManager.GetGump<TopBarGump>();

            if (gump == null)
            {
                if (ProfileManager.CurrentProfile.TopbarGumpPosition.X < 0 || ProfileManager.CurrentProfile.TopbarGumpPosition.Y < 0)
                {
                    ProfileManager.CurrentProfile.TopbarGumpPosition = Point.Zero;
                }

                UIManager.Add
                (
                    gump = new TopBarGump
                    {
                        X = ProfileManager.CurrentProfile.TopbarGumpPosition.X,
                        Y = ProfileManager.CurrentProfile.TopbarGumpPosition.Y
                    }
                );

                if (ProfileManager.CurrentProfile.TopbarGumpIsMinimized)
                {
                    gump.ChangePage(2);
                }
            }
            else
            {
                Log.Error(ResGumps.TopBarGumpAlreadyExists);
            }
        }

        protected override void OnMouseUp(int x, int y, MouseButtonType button)
        {
            if (button == MouseButtonType.Right && (X != 0 || Y != 0))
            {
                X = 0;
                Y = 0;

                ProfileManager.CurrentProfile.TopbarGumpPosition = Location;
            }
        }

        public override void OnPageChanged()
        {
            ProfileManager.CurrentProfile.TopbarGumpIsMinimized = IsMinimized = ActivePage == 2;
            WantUpdateSize = true;
        }

        protected override void OnDragEnd(int x, int y)
        {
            base.OnDragEnd(x, y);
            ProfileManager.CurrentProfile.TopbarGumpPosition = Location;
        }

        public override void OnButtonClick(int buttonID)
        {
            switch ((Buttons) buttonID)
            {
                case Buttons.Map:
                    MiniMapGump miniMapGump = UIManager.GetGump<MiniMapGump>();

                    if (miniMapGump == null)
                    {
                        UIManager.Add(new MiniMapGump());
                    }
                    else
                    {
                        miniMapGump.SetInScreen();
                        miniMapGump.BringOnTop();
                    }

                    break;

                case Buttons.Paperdoll:
                    PaperDollGump paperdollGump = UIManager.GetGump<PaperDollGump>(World.Player);

                    if (paperdollGump == null)
                    {
                        GameActions.OpenPaperdoll(World.Player);
                    }
                    else
                    {
                        paperdollGump.SetInScreen();
                        paperdollGump.BringOnTop();
                    }

                    break;

                case Buttons.Inventory:
                    Item backpack = World.Player.FindItemByLayer(Layer.Backpack);

                    if (backpack == null)
                    {
                        return;
                    }

                    ContainerGump backpackGump = UIManager.GetGump<ContainerGump>(backpack);

                    if (backpackGump == null)
                    {
                        GameActions.DoubleClick(backpack);
                    }
                    else
                    {
                        backpackGump.SetInScreen();
                        backpackGump.BringOnTop();
                    }

                    break;

                case Buttons.Journal:
                    JournalGump journalGump = UIManager.GetGump<JournalGump>();

                    if (journalGump == null)
                    {
                        UIManager.Add(new JournalGump { X = 64, Y = 64 });
                    }
                    else
                    {
                        journalGump.SetInScreen();
                        journalGump.BringOnTop();
                    }

                    break;

                case Buttons.Chat:
                    if (ChatManager.ChatIsEnabled == ChatStatus.Enabled)
                    {
                        ChatGump chatGump = UIManager.GetGump<ChatGump>();

                        if (chatGump == null)
                        {
                            UIManager.Add(new ChatGump());
                        }
                        else
                        {
                            chatGump.SetInScreen();
                            chatGump.BringOnTop();
                        }
                    }
                    else if (ChatManager.ChatIsEnabled == ChatStatus.EnabledUserRequest)
                    {
                        ChatGumpChooseName chatGump = UIManager.GetGump<ChatGumpChooseName>();

                        if (chatGump == null)
                        {
                            UIManager.Add(new ChatGumpChooseName());
                        }
                        else
                        {
                            chatGump.SetInScreen();
                            chatGump.BringOnTop();
                        }
                    }

                    break;

                case Buttons.GlobalChat:
                    Log.Warn(ResGumps.ChatButtonPushedNotImplementedYet);
                    GameActions.Print(ResGumps.GlobalChatNotImplementedYet, 0x23, MessageType.System);

                    break;

                case Buttons.UOStore:
                    if (Client.Version >= ClientVersion.CV_706400)
                    {
                        NetClient.Socket.Send(new POpenUOStore());
                    }

                    break;

                case Buttons.Help:
                    GameActions.RequestHelp();

                    break;

                case Buttons.Debug:

                    DebugGump debugGump = UIManager.GetGump<DebugGump>();

                    if (debugGump == null)
                    {
                        debugGump = new DebugGump(100, 100);
                        UIManager.Add(debugGump);
                    }
                    else
                    {
                        debugGump.IsVisible = !debugGump.IsVisible;
                        debugGump.SetInScreen();
                    }

                    break;

                case Buttons.NetStats:
                    NetworkStatsGump netstatsgump = UIManager.GetGump<NetworkStatsGump>();

                    if (netstatsgump == null)
                    {
                        netstatsgump = new NetworkStatsGump(100, 100);
                        UIManager.Add(netstatsgump);
                    }
                    else
                    {
                        netstatsgump.IsVisible = !netstatsgump.IsVisible;
                        netstatsgump.SetInScreen();
                    }

                    break;

                case Buttons.WorldMap:

                    WorldMapGump worldMap = UIManager.GetGump<WorldMapGump>();

                    if (worldMap == null || worldMap.IsDisposed)
                    {
                        worldMap = new WorldMapGump();
                        UIManager.Add(worldMap);
                    }
                    else
                    {
                        worldMap.BringOnTop();
                        worldMap.SetInScreen();
                    }

                    break;
            }
        }

        private enum Buttons
        {
            Map,
            Paperdoll,
            Inventory,
            Journal,
            Chat,
            Help,
            WorldMap,
            Info,
            Debug,
            NetStats,
            UOStore,
            GlobalChat
        }

        private class RighClickableButton : Button
        {
            public RighClickableButton
            (
                int buttonID,
                ushort normal,
                ushort pressed,
                ushort over = 0,
                string caption = "",
                byte font = 0,
                bool isunicode = true,
                ushort normalHue = ushort.MaxValue,
                ushort hoverHue = ushort.MaxValue
            ) : base(buttonID, normal, pressed, over, caption, font, isunicode, normalHue, hoverHue)
            {
            }

            public RighClickableButton(List<string> parts) : base(parts)
            {
            }

            protected override void OnMouseUp(int x, int y, MouseButtonType button)
            {
                base.OnMouseUp(x, y, button);
                Parent?.InvokeMouseUp(new Point(x, y), button);
            }
        }
    }
}