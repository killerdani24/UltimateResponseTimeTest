#region Libraries
using System;
using System.IO;
using System.Diagnostics;
using IniParser;
using IniParser.Model;
using Raylib_cs;
using rl = Raylib_cs.Raylib;
using System.Threading;
#endregion Libraries

namespace ResponseTest
{
    class Program
    {
        static int windowWidth { get; set; } = 1366;
        static int windowHeight { get; set; } = 768;
        static int targetFPS { get; set; } = 60;
        static int maxBoxSpawns { get; set; } = 20;
        static bool setFullScreen { get; set; } = false;
        enum Level
        {
            MainMenu = 0,
            Start = 1
        }
        enum GameState
        {
            Starting = 0,
            Playing = 1,
            Finish = 2
        }

        static void UpdateSettings()
        {
            if (!File.Exists(".\\settings.ini"))
            {
                using (var sw = new StreamWriter(".\\settings.ini", false))
                    sw.Write("[Settings]\n"                       +
                             "windowWidth=1366\n"                 +
                             "windowHeight=768\n"                 +
                             "targetFPS=144\n"                    +
                             "maxBoxSpawns=20\n" +
                             "setFullScreen=false"                );
            }
            else
            {
                var parser = new FileIniDataParser();
                IniData data = parser.ReadFile(".\\settings.ini");
                data.ClearAllComments();

                windowWidth   = Convert.ToInt32  (data["Settings"]["windowWidth"]  );
                windowHeight  = Convert.ToInt32  (data["Settings"]["windowHeight"] );
                targetFPS     = Convert.ToInt32  (data["Settings"]["targetFPS"]    );
                maxBoxSpawns  = Convert.ToInt32  (data["Settings"]["maxBoxSpawns"] );
                setFullScreen = Convert.ToBoolean(data["Settings"]["setFullScreen"]);
            }
        }
        static void Main(string[] args)
        {
            #region Init
            UpdateSettings();

            rl.InitWindow(windowWidth, windowHeight, "URTT");
            rl.SetTargetFPS(targetFPS);
            rl.SetConfigFlags(ConfigFlag.FLAG_MSAA_4X_HINT);
            if (setFullScreen)
                rl.SetConfigFlags(ConfigFlag.FLAG_FULLSCREEN_MODE);

            Font verdana = rl.LoadFont("C:\\Windows\\Fonts\\verdana.ttf");
            rl.GenTextureMipmaps(ref verdana.texture);
            rl.SetTextureFilter(verdana.texture, TextureFilterMode.FILTER_ANISOTROPIC_16X);

            float mainTitleRecW = 600;
            float mainTitleRecH = 400;
            var mainTitleRec = new Rectangle((windowWidth / 2) - mainTitleRecW / 2, (windowHeight / 2) - mainTitleRecH / 2, mainTitleRecW, mainTitleRecH);

            var startBtn = new Rectangle((windowWidth / 2) - (150 / 2), (windowHeight / 2) - (50 / 2), 150, 50);

            Level levelState = Level.MainMenu;
            GameState gameState = GameState.Starting;

            var rnd = new Random();

            var sw = new Stopwatch();

            var boxWidth  = (float)rnd.Next(30, 100);
            var boxHeight = (float)rnd.Next(30, 100);
            var box = new Rectangle((float)rnd.Next(0, windowWidth - (int)boxWidth), (float)rnd.Next(0, windowHeight - (int)boxHeight), boxWidth, boxHeight);

            long score = 0;
            int boxSpawns = 0;
            #endregion Init

            while (!rl.WindowShouldClose())
            {
                #region Update
                if (levelState == Level.MainMenu)
                {
                    // Click start btton will load level
                    if (((rl.GetMouseX() > (int)startBtn.x && rl.GetMouseX() < (int)(startBtn.x + startBtn.width)) // X boundaries
                         &&
                         (rl.GetMouseY() < (int)(startBtn.y + startBtn.height) && rl.GetMouseY() > startBtn.y) // Y boundaries
                        )
                        &&
                        rl.IsMouseButtonPressed(MouseButton.MOUSE_LEFT_BUTTON) // Click
                       )
                        levelState = Level.Start;
                }

                if (levelState == Level.Start)
                {
                    switch (gameState)
                    {
                        case GameState.Starting:
                            score = 0;
                            boxSpawns = 0;
                            gameState = GameState.Playing;
                            
                            break;
                        case GameState.Playing:
                            sw.Start();

                            if (boxSpawns > maxBoxSpawns - 1)
                                gameState = GameState.Finish;

                            // Box mouse collision check
                            if (((rl.GetMouseX() > (int)box.x && rl.GetMouseX() < (int)(box.x + box.width)) // X boundaries
                                 &&
                                 (rl.GetMouseY() < (int)(box.y + box.height) && rl.GetMouseY() > box.y) // Y boundaries
                                )
                                &&
                                rl.IsMouseButtonPressed(MouseButton.MOUSE_LEFT_BUTTON) // Click
                               )
                            {   
                                box.width = (float)rnd.Next(30, 100);
                                box.height = (float)rnd.Next(30, 100);
                                box.x = (float)rnd.Next(0, windowWidth - (int)box.width);
                                box.y = (float)rnd.Next(0, windowHeight - (int)box.height);

                                boxSpawns++;
                            }

                            score = sw.ElapsedMilliseconds;

                            break;
                        case GameState.Finish:
                            sw.Stop();
                            score = sw.ElapsedMilliseconds;
                            
                            break;
                    }
                }
                #endregion Update

                #region Draw
                rl.BeginDrawing();
                
                rl.ClearBackground(Color.DARKGRAY);
                
                if (levelState == Level.MainMenu)
                {
                    rl.DrawTextRec(verdana, "ULTIMATE RESPONSE TIME TEST", mainTitleRec, 64, 2f, true, Color.WHITE);

                    rl.DrawRectangleRec(startBtn, Color.MAROON);
                    rl.DrawTextRec(verdana, "START", startBtn, 50, 0.5f, false, Color.BLACK);
                    
                    rl.DrawText("Change The Settings In 'settings.ini' In The Game Directory", 100, windowHeight - 100, 26, Color.YELLOW); ;
                }
                if (levelState == Level.Start)
                {
                    switch (gameState)
                    {
                        case GameState.Starting:
                            break;
                        case GameState.Playing:
                            rl.DrawRectangleRec(box, Color.SKYBLUE);

                            rl.DrawText($"{score}", windowWidth / 2 - 50, 35, 28, Color.ORANGE);
                            break;
                        case GameState.Finish:
                            rl.DrawText("Your Final Score: ", windowWidth / 2 - 222, windowHeight / 2 - 80, 52, Color.ORANGE);
                            rl.DrawText($"{score}", windowWidth / 2 - 68, windowHeight / 2 - 24, 48, Color.ORANGE);
                            break;
                    }
                }

                rl.DrawFPS(10, 10);

                rl.EndDrawing();
                #endregion Draw
            }
            rl.CloseWindow();
        }
    }
}