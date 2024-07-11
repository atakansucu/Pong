using System;
using SDL2;
using static System.Net.Mime.MediaTypeNames;
using static SDL2.SDL;
using static SDL2.SDL_ttf;
using static SDL2.SDL_image;
using System.Runtime.InteropServices;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PongGame
{
    public enum Difficulty
    {
        Easy,
        Medium,
        Hard
    }


    internal class StartAgainstComputer
    {
        public static void CpuMovePaddle(Difficulty difficulty, ref SDL_Rect paddleRect, ref SDL_Rect ballRect)
        {

            // Adjust CPU paddle position based on ball's Y-coordinate
            if (paddleRect.y + paddleRect.h / 2 < ballRect.y + ballRect.h / 2)
            {
                // Ball is below the CPU paddle, move the paddle down
                paddleRect.y += 5;
            }
            else if (paddleRect.y + paddleRect.h / 2 > ballRect.y + ballRect.h / 2)
            {
                // Ball is above the CPU paddle, move the paddle up
                paddleRect.y -= 5;
            }

            // Ensure that the paddle remains within the screen bounds
            if (paddleRect.y < 60)
            {
                paddleRect.y = 60;
            }
            else if (paddleRect.y > 600 - paddleRect.h - 60)
            {
                paddleRect.y = 600 - paddleRect.h - 60;
            }
        }


        public static void Start(IntPtr renderer)
        {
            // Top ve paddle resimlerinin dosya yolları 
            string ballImagePath = "Assets\\Images\\Ball.png";
            string paddleImagePath = "Assets\\Images\\Paddle_1.png";
            string backgroundImagePath = "Assets\\Images\\BackgroundEmpty.png";

            // Top ve paddle'ları yüklemek için yüzeyler oluştur
            IntPtr ballSurface = IMG_Load(ballImagePath);
            IntPtr paddleSurface = IMG_Load(paddleImagePath);
            IntPtr backgroundSurface = IMG_Load(backgroundImagePath);

            // Yüklenen yüzeylerin kontrolü
            if (ballSurface == IntPtr.Zero || paddleSurface == IntPtr.Zero || backgroundSurface == IntPtr.Zero)
            {
                Console.WriteLine($"Failed to load image! SDL Error: {SDL_GetError()}");
                return;
            }

            // Texture'ları oluştur
            IntPtr ballTexture = SDL_CreateTextureFromSurface(renderer, ballSurface);
            IntPtr paddleTexture = SDL_CreateTextureFromSurface(renderer, paddleSurface);
            IntPtr backgroundTexture = SDL_CreateTextureFromSurface(renderer, backgroundSurface);

            // Texture'ların kontrolü
            if (ballTexture == IntPtr.Zero || paddleTexture == IntPtr.Zero || backgroundTexture == IntPtr.Zero)
            {
                Console.WriteLine($"Failed to create texture from surface! SDL Error: {SDL_GetError()}");
                SDL_FreeSurface(ballSurface);
                SDL_FreeSurface(paddleSurface);
                SDL_FreeSurface(backgroundSurface);
                SDL_DestroyTexture(ballTexture);
                SDL_DestroyTexture(paddleTexture);
                SDL_DestroyTexture(backgroundTexture);
                return;
            }

            // Top ve paddle yüzeylerini bellekten temizle
            SDL_FreeSurface(ballSurface);
            SDL_FreeSurface(paddleSurface);
            SDL_FreeSurface(backgroundSurface);

            // Top ve paddle boyutlarını al
            int ballWidth, ballHeight;
            int paddleWidth, paddleHeight;
            SDL_QueryTexture(ballTexture, out _, out _, out ballWidth, out ballHeight);
            SDL_QueryTexture(paddleTexture, out _, out _, out paddleWidth, out paddleHeight);

            // Oyun ekranı boyutları
            const int screenWidth = 800;
            const int screenHeight = 600;

            // Top ve paddle'ların başlangıç pozisyonlarını belirle
            SDL_Rect ballRect = new SDL_Rect { x = screenWidth / 2 - ballWidth / 2, y = screenHeight / 2 - ballHeight / 2, w = ballWidth, h = ballHeight };
            SDL_Rect leftPaddleRect = new SDL_Rect { x = 50, y = screenHeight / 2 - paddleHeight / 2, w = paddleWidth, h = paddleHeight };
            SDL_Rect rightPaddleRect = new SDL_Rect { x = screenWidth - 50 - paddleWidth, y = screenHeight / 2 - paddleHeight / 2, w = paddleWidth, h = paddleHeight };


            // Top (ball) oluşturma

            float ballSpeedX = 2.0f; // Topun X eksenindeki hızı
            float ballSpeedY = 2.0f; // Topun Y eksenindeki hızı


            // Oyuncu skorları
            int player1Score = 0;
            int player2Score = 0;

            float speedIncrease = 0.1f;

            // Frame rate settings
            const uint desiredFPS = 60;
            const uint frameTime = 1000 / desiredFPS;

            SDL_Color textColor2 = new SDL_Color { r = 255, g = 255, b = 255 };
            IntPtr font = TTF_OpenFont("Assets\\Fonts\\MontereyFLF-Bold.ttf", 24); // varsayılan bir TrueType font dosyası kullanıyoruz 

            bool returnToMenu = false; // Flag to indicate whether to return to the main menu
            bool quit = false;

            // Tuşları takip etmek için bir Dictionary oluştur
            Dictionary<SDL_Keycode, bool> keyStates = new Dictionary<SDL_Keycode, bool>();

            uint dKeyDownTime = 0;
            uint leftKeyDownTime = 0;


            while (!quit)
            {
                uint frameStart = SDL_GetTicks();

                SDL_Event e;
                while (SDL_PollEvent(out e) != 0)
                {
                    if (e.type == SDL_EventType.SDL_QUIT || (e.type == SDL_EventType.SDL_KEYDOWN && e.key.keysym.sym == SDL_Keycode.SDLK_ESCAPE))
                    {
                        quit = true;
                        break;
                    }

                    // Tuş durumlarını güncelle
                    if (e.type == SDL_EventType.SDL_KEYDOWN)
                    {
                        keyStates[e.key.keysym.sym] = true;
                        if (e.key.keysym.sym == SDL_Keycode.SDLK_d)
                        {
                            dKeyDownTime = SDL_GetTicks();
                        }
                        else if (e.key.keysym.sym == SDL_Keycode.SDLK_LEFT)
                        {
                            leftKeyDownTime = SDL_GetTicks();
                        }
                    }
                    else if (e.type == SDL_EventType.SDL_KEYUP)
                    {
                        keyStates[e.key.keysym.sym] = false;
                        if (e.key.keysym.sym == SDL_Keycode.SDLK_d)
                        {

                            dKeyDownTime = 0;
                        }
                        else if (e.key.keysym.sym == SDL_Keycode.SDLK_LEFT)
                        {

                            leftKeyDownTime = 0;
                        }
                    }
                }
                SDL_Color textColor = new SDL_Color { r = 255, g = 255, b = 255 };
                // Check for winner
                if (player1Score >= 2 || player2Score >= 2)
                {
                    string winner = player1Score >= 2 ? "1" : "2";
                    var winSurface = TTF_RenderText_Solid(font, $"Player {winner} has won! Press enter to start again.", textColor);
                    var winTexture = SDL_CreateTextureFromSurface(renderer, winSurface);
                    var winRect = new SDL_Rect { x = 100, y = 150, w = 600, h = 50 };  // Adjust the position and size as necessary
                    SDL_RenderCopy(renderer, winTexture, IntPtr.Zero, ref winRect);
                    SDL_RenderPresent(renderer);

                    // Show return to menu button
                    var returnToMenuSurface = TTF_RenderText_Solid(font, "Main Menu", textColor);
                    var returnToMenuTexture = SDL_CreateTextureFromSurface(renderer, returnToMenuSurface);
                    var returnToMenuRect = new SDL_Rect { x = 350, y = 400, w = 150, h = 50 };

                    SDL_RenderCopy(renderer, returnToMenuTexture, IntPtr.Zero, ref returnToMenuRect);
                    SDL_RenderPresent(renderer);

                    // Diğer değişkenleri sıfırla veya yeniden başlat
                    player1Score = 0;
                    player2Score = 0;
                    ballRect.x = 400;
                    ballRect.y = 300;
                    leftPaddleRect.y = 250;
                    rightPaddleRect.y = 250;


                    // Wait for Enter to be pressed or return to menu button to be clicked
                    bool restart = false;
                    while (!restart)
                    {
                        SDL_Event restartEvent;
                        while (SDL_PollEvent(out restartEvent) != 0)
                        {
                            switch (restartEvent.type)
                            {
                                case SDL_EventType.SDL_QUIT:
                                    quit = true;
                                    restart = true; // Exit both loops
                                    break;
                                case SDL_EventType.SDL_KEYDOWN:
                                    if (restartEvent.key.keysym.sym == SDL_Keycode.SDLK_RETURN)
                                    {
                                        restart = true;
                                    }
                                    break;
                                case SDL_EventType.SDL_MOUSEBUTTONDOWN:
                                    if (restartEvent.button.button == SDL_BUTTON_LEFT)
                                    {
                                        int x = restartEvent.button.x;
                                        int y = restartEvent.button.y;

                                        if (x >= returnToMenuRect.x && x < returnToMenuRect.x + returnToMenuRect.w &&
                                            y >= returnToMenuRect.y && y < returnToMenuRect.y + returnToMenuRect.h)
                                        {
                                            Console.WriteLine("Return to menu button clicked");
                                            returnToMenu = true;
                                            quit = true;
                                            restart = true; // Exit both loops
                                        }
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                    }


                    if (!quit)
                    {
                        player1Score = 0;
                        player2Score = 0;
                        ballRect.x = 400;
                        ballRect.y = 300;
                        leftPaddleRect.y = 250;
                        rightPaddleRect.y = 250;
                    }
                }


                // Sol çubuğu hareket ettirme (W ve S tuşları)
                if (keyStates.ContainsKey(SDL_Keycode.SDLK_w) && keyStates[SDL_Keycode.SDLK_w])
                {
                    if (leftPaddleRect.y > 60)
                        leftPaddleRect.y -= 10;
                }
                if (keyStates.ContainsKey(SDL_Keycode.SDLK_s) && keyStates[SDL_Keycode.SDLK_s])
                {
                    if (leftPaddleRect.y < 600 - leftPaddleRect.h - 60)
                        leftPaddleRect.y += 10;
                }



                // Sol çubuğu hareket ettirme (A ve D tuşları)
                if (keyStates.ContainsKey(SDL_Keycode.SDLK_a) && keyStates[SDL_Keycode.SDLK_a])
                {
                    if (leftPaddleRect.x > 30)
                        leftPaddleRect.x -= 10;

                }
                if (keyStates.ContainsKey(SDL_Keycode.SDLK_d) && keyStates[SDL_Keycode.SDLK_d])
                {
                    if (leftPaddleRect.x < 200 - leftPaddleRect.w - 30) // Sadece ekranın yarısına kadar gidebilmesini sağlayalım
                        leftPaddleRect.x += 10;

                }


                CpuMovePaddle(Difficulty.Easy, ref rightPaddleRect, ref ballRect);



                // Topun hareketini güncelleme
                ballRect.x += (int)ballSpeedX;
                ballRect.y += (int)ballSpeedY;

                // Topun çubuklara çarpma kontrolü
                if (ballRect.y <= 60 || ballRect.y >= 600 - ballRect.h - 60)
                    ballSpeedY = -ballSpeedY; // Y ekseninde yansıma

                // Sol çubuğa çarpma kontrolü
                if (ballSpeedX < 0 && // Top sola doğru hareket ediyorsa
                    ballRect.x <= leftPaddleRect.x + leftPaddleRect.w - 10 && // Topun sağ kenarı, sol çubuğun sağ kenarından küçük veya eşitse
                    ballRect.x >= leftPaddleRect.x + 10 && // Topun sol kenarı, sol çubuğun sol kenarından büyük veya eşitse
                    ballRect.y + ballRect.h >= leftPaddleRect.y + 10 && // Topun alt kenarı, sol çubuğun üst kenarından büyük veya eşitse
                    ballRect.y <= leftPaddleRect.y + leftPaddleRect.h - 10) // Topun üst kenarı, sol çubuğun alt kenarından küçük veya eşitse
                {
                    if (leftPaddleRect.x < (200 - leftPaddleRect.w - 30))
                    {
                        uint dKeyDuraton = 0;

                        if (ballSpeedX >= -5)
                        {
                            if (keyStates.ContainsKey(SDL_Keycode.SDLK_d) && keyStates[SDL_Keycode.SDLK_d])
                            {
                                dKeyDuraton = SDL_GetTicks() - dKeyDownTime;
                                dKeyDuraton = dKeyDuraton / 10;
                                Console.WriteLine(dKeyDuraton);

                                ballSpeedX -= speedIncrease * dKeyDuraton; // Topun X eksenindeki hızını artır

                                if (ballSpeedY > 0)
                                {
                                    ballSpeedY += speedIncrease * dKeyDuraton;
                                }
                                else
                                {
                                    ballSpeedY -= speedIncrease * dKeyDuraton;
                                }
                            }
                        }
                    }

                    ballSpeedX = -ballSpeedX; // Sol çubuğa çarpma               
                    ballRect.x -= (int)ballSpeedX;

                    if (ballSpeedY > 0)
                    {
                        ballRect.y += (int)ballSpeedY;
                    }
                    else
                    {
                        ballRect.y -= (int)ballSpeedY;
                    }


                    Console.WriteLine("SOLballSpeedX: " + ballSpeedX + " SOLballSpeedY:" + +ballSpeedY);
                    Console.WriteLine("SOLspeedIncrease: " + speedIncrease);

                }

                // Sağ çubuğa çarpma kontrolü
                if (ballSpeedX > 0 && // Top sağa doğru hareket ediyorsa
                    ballRect.x + ballRect.w >= rightPaddleRect.x + 10 && // Topun sol kenarı, sağ çubuğun sol kenarından büyük veya eşitse
                    ballRect.x + ballRect.w <= rightPaddleRect.x + rightPaddleRect.w - 10 && // Topun sol kenarı, sağ çubuğun sağ kenarından küçük veya eşitse
                    ballRect.y + ballRect.h >= rightPaddleRect.y + 10 && // Topun alt kenarı, sağ çubuğun üst kenarından büyük veya eşitse
                    ballRect.y <= rightPaddleRect.y + rightPaddleRect.h - 10)
                { // Topun üst kenarı, sağ çubuğun alt kenarından küçük veya eşitse
                    ballSpeedX += speedIncrease;
                    ballSpeedX = -ballSpeedX; // Sağ çubuğa çarpma
                    ballRect.x += (int)ballSpeedX;

                    if (ballSpeedY > 0)
                    {
                        ballSpeedY += speedIncrease;
                        ballRect.y += (int)ballSpeedY;
                    }
                    else
                    {
                        ballSpeedY -= speedIncrease;
                        ballRect.y -= (int)ballSpeedY;
                    }

                    Console.WriteLine("SAGballSpeedX: " + ballSpeedX + "SAGballSpeedY:" + +ballSpeedY);
                    Console.WriteLine("SAGspeedIncrease: " + speedIncrease);
                }
                // Topun ekran sınırlarına çarpma kontrolü
                if (ballRect.x <= 0)
                {
                    player2Score++; // Sağ oyuncu skorunu artır
                    ballRect.x = 400; // Topu başlangıç noktasına geri getir
                    ballRect.y = 300;
                    ballSpeedX = 2.0f; // Topun X eksenindeki hızı
                    ballSpeedY = 2.0f; // Topun Y eksenindeki hızı
                }
                else if (ballRect.x >= 800 - ballRect.w)
                {
                    player1Score++; // Sol oyuncu skorunu artır
                    ballRect.x = 400; // Topu başlangıç noktasına geri getir
                    ballRect.y = 300;
                    ballSpeedX = 2.0f; // Topun X eksenindeki hızı
                    ballSpeedY = 2.0f; // Topun Y eksenindeki hızı
                }

                SDL_SetRenderDrawColor(renderer, 0, 0, 50, 255);
                SDL_RenderClear(renderer);
                SDL_SetRenderDrawColor(renderer, 255, 255, 255, 255);

                // Arkaplanı çiz
                SDL_RenderCopy(renderer, backgroundTexture, IntPtr.Zero, IntPtr.Zero);

                SDL_RenderCopy(renderer, ballTexture, IntPtr.Zero, ref ballRect);
                SDL_RenderCopy(renderer, paddleTexture, IntPtr.Zero, ref leftPaddleRect);
                SDL_RenderCopy(renderer, paddleTexture, IntPtr.Zero, ref rightPaddleRect);




                // Skorları ekrana yazdırma
                var surface = TTF_RenderText_Solid(font, $"{player1Score} - {player2Score}", textColor); // Skorları birleştirip yazdır
                var scoreTexture = SDL_CreateTextureFromSurface(renderer, surface);
                var scoreRect = new SDL_Rect { x = 325, y = 250, w = 150, h = 100 }; // Ekranın ortasına yerleştir
                SDL_RenderCopy(renderer, scoreTexture, IntPtr.Zero, ref scoreRect);

                SDL_RenderPresent(renderer);
                SDL_Delay(10); // Topun hızını düşürmek için bir gecikme ekleyelim

            }

            if (returnToMenu)
            {
                var menu = new MainMenu(renderer);
                menu.ShowMenu();
            }

            SDL_Quit();
            return;
        }
    }
}
