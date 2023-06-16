using System;
using System.Collections.Generic;
using System.Threading;

class SnakeGame
{
    private static int width = 20; // Lățimea zonei de joc
    private static int height = 10; // Înălțimea zonei de joc
    private static int score = 0; // Scorul jucătorului
    private static int delay = 200; // Durata între mișcări (în milisecunde)

    private static int foodX;
    private static int foodY;
    private static bool gameOver;
    private static Direction direction;
    private static List<int> snakeX;
    private static List<int> snakeY;
    private static List<int> wallX;
    private static List<int> wallY;
    private static bool enableWalls;
    private static int level;
    private static bool levelUp;
    private static int lives;
    private static bool enablePowerUps;
    private static int powerUpX;
    private static int powerUpY;
    private static bool powerUpActive;
    private static int powerUpDuration;
    private static int powerUpTimer;

    private enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }

    static void Main()
    {
        Console.Title = "Snake Game";
        Console.CursorVisible = false;

        InitializeGame();

        while (!gameOver)
        {
            if (Console.KeyAvailable)
                ProcessInput(Console.ReadKey(true).Key);

            MoveSnake();
            CheckCollision();
            UpdatePowerUp();
            DrawGame();

            Thread.Sleep(delay);
        }

        Console.SetCursorPosition(width / 2 - 5, height / 2);
        Console.WriteLine("Game Over! Score: " + score);
        Console.ReadLine();
    }

    private static void InitializeGame()
    {
        snakeX = new List<int>() { width / 2 };
        snakeY = new List<int>() { height / 2 };
        direction = Direction.Right;
        gameOver = false;
        score = 0;
        level = 1;
        lives = 3;
        enableWalls = false;
        enablePowerUps = true;
        levelUp = false;
        powerUpActive = false;
        powerUpDuration = 10;
        powerUpTimer = 0;

        GenerateFood();
        GenerateWalls();
    }

    private static void GenerateFood()
    {
        Random random = new Random();
        foodX = random.Next(0, width);
        foodY = random.Next(0, height);
    }

    private static void GenerateWalls()
    {
        wallX = new List<int>();
        wallY = new List<int>();

        if (enableWalls)
        {
            Random random = new Random();
            int wallCount = random.Next(5, 10); // Generare între 5 și 10 ziduri

            for (int i = 0; i < wallCount; i++)
            {
                int wallPosX = random.Next(1, width - 1);
                int wallPosY = random.Next(1, height - 1);

                // Asigură că zidul nu se suprapune cu șarpele sau cu mâncarea
                if (!snakeX.Contains(wallPosX) && !snakeY.Contains(wallPosY) && (foodX != wallPosX || foodY != wallPosY))
                {
                    wallX.Add(wallPosX);
                    wallY.Add(wallPosY);
                }
            }
        }
    }

    private static void GeneratePowerUp()
    {
        Random random = new Random();
        powerUpX = random.Next(0, width);
        powerUpY = random.Next(0, height);
    }

    private static void ProcessInput(ConsoleKey key)
    {
        switch (key)
        {
            case ConsoleKey.UpArrow:
                if (direction != Direction.Down)
                    direction = Direction.Up;
                break;
            case ConsoleKey.DownArrow:
                if (direction != Direction.Up)
                    direction = Direction.Down;
                break;
            case ConsoleKey.LeftArrow:
                if (direction != Direction.Right)
                    direction = Direction.Left;
                break;
            case ConsoleKey.RightArrow:
                if (direction != Direction.Left)
                    direction = Direction.Right;
                break;
            case ConsoleKey.Escape:
                gameOver = true;
                break;
        }
    }

    private static void MoveSnake()
    {
        int headX = snakeX[0];
        int headY = snakeY[0];

        switch (direction)
        {
            case Direction.Up:
                headY--;
                break;
            case Direction.Down:
                headY++;
                break;
            case Direction.Left:
                headX--;
                break;
            case Direction.Right:
                headX++;
                break;
        }

        // Verifică dacă șarpele a ajuns la marginea zonei de joc și dacă zidurile sunt activate
        if (enableWalls && (headX < 0 || headX >= width || headY < 0 || headY >= height))
        {
            lives--;
            if (lives <= 0)
                gameOver = true;
            else
                ResetSnake();
        }
        else
        {
            // Verifică dacă șarpele a ajuns la marginea zonei de joc și dacă zidurile sunt dezactivate, mută-l pe partea opusă
            if (!enableWalls)
            {
                if (headX < 0)
                    headX = width - 1;
                else if (headX >= width)
                    headX = 0;
                if (headY < 0)
                    headY = height - 1;
                else if (headY >= height)
                    headY = 0;
            }

            snakeX.Insert(0, headX);
            snakeY.Insert(0, headY);

            // Verifică dacă șarpele a mâncat hrana
            if (headX == foodX && headY == foodY)
            {
                score++;
                if (score % 5 == 0)
                {
                    level++;
                    levelUp = true;
                }
                GenerateFood();
            }
            else
            {
                snakeX.RemoveAt(snakeX.Count - 1);
                snakeY.RemoveAt(snakeY.Count - 1);
            }
        }
    }

    private static void CheckCollision()
    {
        int headX = snakeX[0];
        int headY = snakeY[0];

        // Verifică coliziunea cu propriul corp
        if (snakeX.IndexOf(headX) != snakeY.IndexOf(headY))
        {
            lives--;
            if (lives <= 0)
                gameOver = true;
            else
                ResetSnake();
        }

        // Verifică coliziunea cu zidurile
        if (enableWalls && wallX.Contains(headX) && wallY.Contains(headY))
        {
            lives--;
            if (lives <= 0)
                gameOver = true;
            else
                ResetSnake();
        }

        // Verifică coliziunea cu un power-up
        if (enablePowerUps && powerUpActive && headX == powerUpX && headY == powerUpY)
        {
            powerUpActive = false;
            powerUpTimer = 0;
            delay -= 50; // Scade intervalul de mișcare pentru o perioadă de timp

            if (delay < 50)
                delay = 50; // Asigură că intervalul minim de mișcare este 50ms
        }
    }

    private static void UpdatePowerUp()
    {
        if (enablePowerUps)
        {
            powerUpTimer++;

            // Verifică dacă este activ un power-up și dacă a expirat
            if (powerUpActive && powerUpTimer >= powerUpDuration)
            {
                powerUpActive = false;
                powerUpTimer = 0;
                delay += 50; // Restabilește intervalul de mișcare la valoarea inițială
            }

            // Verifică dacă trebuie generat un nou power-up
            if (!powerUpActive && powerUpTimer == 0 && score > 0 && score % 5 == 0)
            {
                GeneratePowerUp();
                powerUpActive = true;
            }
        }
    }

    private static void ResetSnake()
    {
        snakeX = new List<int>() { width / 2 };
        snakeY = new List<int>() { height / 2 };
        direction = Direction.Right;

        GenerateFood();
    }

    private static void DrawGame()
    {
        Console.Clear();

        // Desenează zidurile
        if (enableWalls)
        {
            for (int i = 0; i < wallX.Count; i++)
            {
                Console.SetCursorPosition(wallX[i], wallY[i]);
                Console.Write("#");
            }
        }

        // Desenează hrana
        Console.SetCursorPosition(foodX, foodY);
        Console.Write("@");

        // Desenează șarpele
        for (int i = 0; i < snakeX.Count; i++)
        {
            Console.SetCursorPosition(snakeX[i], snakeY[i]);
            if (i == 0)
                Console.Write("O"); // Capul șarpelui
            else
                Console.Write("o"); // Corpul șarpelui
        }

        // Desenează power-up-ul
        if (enablePowerUps && powerUpActive)
        {
            Console.SetCursorPosition(powerUpX, powerUpY);
            Console.Write("P");
        }

        // Desenează scorul și informații suplimentare
        Console.SetCursorPosition(0, height);
        Console.WriteLine("Score: " + score);
        Console.WriteLine("Level: " + level);
        Console.WriteLine("Lives: " + lives);
        Console.WriteLine("Enable Walls: " + (enableWalls ? "Yes" : "No"));
        Console.WriteLine("Enable Power-Ups: " + (enablePowerUps ? "Yes" : "No"));

        // Afiseaza mesajul de nivel nou atins
        if (levelUp)
        {
            Console.SetCursorPosition(width / 2 - 5, height / 2);
            Console.WriteLine("Level Up! Level: " + level);
            Thread.Sleep(2000);
            levelUp = false;
        }
    }
}
