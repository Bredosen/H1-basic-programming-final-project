using H1_basic_programming_final_project.Core.DataModels;
using H1_basic_programming_final_project.Core.Handler;
using H1_basic_programming_final_project.Core.Types;
using System.Diagnostics;

namespace H1_basic_programming_final_project.Core.Pages;

public sealed class PingPongPage : Page
{
    #region Members

    #region Singleton
    private static readonly Lazy<PingPongPage> _instance = new(() => new PingPongPage(), true);
    public static readonly PingPongPage Instance = _instance.Value;
    #endregion

    #region Properties
    public int Width;
    public int Height;

    public int Player1Height = 0;
    public int Player2Height = 0;

    private const int PaddleHeight = 6;
    private int Paddle1X => 2;
    private int Paddle2X => Width - 3;
    private static readonly Random Rng = new();

    private double BallSpeed = 90.0D;
    private double PlayerSpeed = 18.0D;
    public int lastDrawX = -1, lastDrawY = -1;
    public int trail1X = -1, trail1Y = -1;
    public int trail2X = -1, trail2Y = -1;
    private double BallPosX, BallPosY;
    private double BallVelX, BallVelY;
    private const double PhysicsDT = 1.0 / 30.0;
    private const double MaxFrameClamp = 0.1;
    public double BallX = 0;
    public double BallY = 0;
    public double BallVelocityX = 0;
    public double BallVelocityY = 0;


    public int Player1Score = 0;
    public int Player2Score = 0;
    #endregion

    #endregion

    #region Constructor
    private PingPongPage() : base("PingPongPage")
    {
        Activated += PingPongPage_Activated;
    }
    #endregion

    #region Activated
    private void PingPongPage_Activated()
    {
        StartGame();
    }
    #endregion

    /*
    #region Set Player Speed
    public void SetPlayerSpeed()
    {
        COut.Space();
        COut.WriteLine("Please enter the new player speed (default is 5.5):");
        string input = COut.GetUserInput("[Speed] >> ");
        if (double.TryParse(input, out double newSpeed) && newSpeed > 0)
        {
            PlayerSpeed = InverseMap(newSpeed);
            COut.WriteLine($"Player speed set to {newSpeed}.");
        }
        else
        {
            COut.WriteLine("Invalid input. Player speed remains unchanged.");
        }
        COut.WaitForContinue();
      
    }
    #endregion

    #region Inverse Map
    public static double InverseMap(double input, double scale = 100.0)
    {
        if (input <= 0)
            return scale;
        return scale / input;
    }
    #endregion

    #region Set Ball Speed
    public void SetBallSpeed()
    {
        
        COut.Space();
        COut.WriteLine("Please enter the new ball speed (default is 50.0):");
        string input = COut.GetUserInput("[Speed] >> ");
        if (double.TryParse(input, out double newSpeed) && newSpeed > 0)
        {
            BallSpeed = newSpeed;
            COut.WriteLine($"Ball speed set to {BallSpeed}.");
        }
        else
        {
            COut.WriteLine("Invalid input. Ball speed remains unchanged.");
        }
        COut.WaitForContinue();
        
    }
    #endregion
    */



    public override void Render()
    {

    }

    #region Draw Border
    public void DrawBorder()
    {
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                if (x == 0 || x == Width - 1 || y == 0 || y == Height - 1)
                {
                    Console.SetCursorPosition(x, y);
                    Console.Write('#');
                }
            }
        }
    }
    #endregion

    #region Draw Score
    public void DrawScore()
    {
        string score = $"=[ Player 1: {Player1Score} | Player 2: {Player2Score} ]=";
        Console.SetCursorPosition(Width / 2 - (score.Length / 2), 0);
        Console.Write(score);
    }
    #endregion

    #region Draw Player
    public void DrawPlayer(int x, int y, double height, char character)
    {
        for (int i = (int)(-height / 2); i < height / 2; i++)
        {
            int yy = y + i;
            if (yy <= 0 || yy >= Height - 1)
            {
                continue;
            }

            Console.SetCursorPosition(x, yy);
            Console.Write(character);
        }
    }
    #endregion

    #region ReDraw Player
    public void ReDrawPlayer(int player, bool up)
    {
        if (player == 1)
        {

            DrawPlayer(Paddle1X, Player1Height, PaddleHeight, ' ');
            Player1Height += up ? -1 : 1;
            Player1Height = Math.Clamp(Player1Height, 1 + (PaddleHeight / 2), Height - 2 - (PaddleHeight / 2));
            DrawPlayer(Paddle1X, Player1Height, PaddleHeight, '|');
            return;
        }

        if (player == 2)
        {
            DrawPlayer(Paddle2X, Player2Height, PaddleHeight, ' ');
            Player2Height += up ? -1 : 1;
            Player2Height = Math.Clamp(Player2Height, 1 + (PaddleHeight / 2), Height - 2 - (PaddleHeight / 2));
            DrawPlayer(Paddle2X, Player2Height, PaddleHeight, '|');
            return;
        }
    }
    #endregion


    long p1Last = 0L;
    long p2Last = 0L;
    long lastTicks = 0L;
    double accumulator = 0.0D;

    int minX = 0, maxX = 0;
    int minY = 0, maxY = 0;

    #region Start Game
    public void StartGame()
    {
        InitializeConsole();
        InitializePlayers();
        DrawInitialScene();
        StartRawInput();

        InitializeBallPhysics();

        DrawScore();

        p1Last = LifeCycleWatch.ElapsedMilliseconds;
        p2Last = LifeCycleWatch.ElapsedMilliseconds;
        lastTicks = LifeCycleWatch.ElapsedTicks;
        minX = 1;
        minY = 1;
        maxX = Width - 1;
        maxY = Height - 2;
        RunGameLoop();
    }
    #endregion

    #region Initialize Console
    private void InitializeConsole()
    {
        Width = Console.WindowWidth;
        Height = Console.WindowHeight;
        Console.Clear();
        Console.CursorVisible = false;
    }
    #endregion

    #region Initialize Players
    private void InitializePlayers()
    {
        Player1Height = Height / 2;
        Player2Height = Height / 2;
    }
    #endregion

    #region Draw Initial Scene
    private void DrawInitialScene()
    {
        DrawBorder();
        DrawPlayer(Paddle1X, Player1Height, PaddleHeight, '|');
        DrawPlayer(Paddle2X, Player2Height, PaddleHeight, '|');
    }
    #endregion

    #region Start RawInput
    private void StartRawInput()
    {
        _ = System.Threading.Tasks.Task.Run(RawInput.Poll);
    }
    #endregion

    #region Initialize Ball Physics
    private void InitializeBallPhysics()
    {
        BallPosX = Width / 2.0;
        BallPosY = Height / 2.0;
        BallVelX = (Rng.NextDouble() * 2) - 1;
        BallVelY = 0;
        Normalize(ref BallVelX, ref BallVelY);

        lastDrawX = -1; lastDrawY = -1;
        trail1X = trail1Y = trail2X = trail2Y = -1;
    }
    #endregion

    #region Run Game Loop
    private void RunGameLoop()
    {
        while (true)
        {
            ClampPaddles();

            long nowMs = LifeCycleWatch.ElapsedMilliseconds;
            HandleInputTick(nowMs, ref p1Last, ref p2Last, PlayerSpeed);

            double dt = ComputeDeltaTime(LifeCycleWatch, ref lastTicks);
            AccumulatePhysics(dt, ref accumulator);

            int drawX = (int)Math.Round(BallPosX);
            int drawY = (int)Math.Round(BallPosY);

            DrawFrame(drawX, drawY, minX, maxX, minY, maxY);

            lastDrawX = drawX;
            lastDrawY = drawY;
        }
    }
    #endregion

    #region Clamp Paddles
    private void ClampPaddles()
    {
        Player1Height = Math.Clamp(Player1Height, 1 + (PaddleHeight / 2), Height - 2 - (PaddleHeight / 2));
        Player2Height = Math.Clamp(Player2Height, 1 + (PaddleHeight / 2), Height - 2 - (PaddleHeight / 2));
    }
    #endregion

    #region Handle Input Tick
    private void HandleInputTick(long nowMs, ref long p1Last, ref long p2Last, double moveIntervalMs)
    {

        if (nowMs - p1Last >= moveIntervalMs)
        {
            p1Last = nowMs;
            if (RawInput.IsHeld(VirtuelKeys.W))
            {
                ReDrawPlayer(1, true);
            }

            if (RawInput.IsHeld(VirtuelKeys.S))
            {
                ReDrawPlayer(1, false);
            }
        }

        if (nowMs - p2Last >= moveIntervalMs)
        {
            p2Last = nowMs;
            if (RawInput.IsHeld(VirtuelKeys.UP))
            {
                ReDrawPlayer(2, true);
            }

            if (RawInput.IsHeld(VirtuelKeys.DOWN))
            {
                ReDrawPlayer(2, false);
            }
        }

    }
    #endregion

    #region Compute Delta Time
    private double ComputeDeltaTime(Stopwatch sw, ref long lastTicks)
    {
        long nowTicks = sw.ElapsedTicks;
        double dt = (nowTicks - lastTicks) / (double)Stopwatch.Frequency;
        lastTicks = nowTicks;
        if (dt > MaxFrameClamp)
        {
            dt = MaxFrameClamp;
        }

        return dt;
    }
    #endregion

    #region Accumulate Physics
    private void AccumulatePhysics(double dt, ref double accumulator)
    {
        accumulator += dt;
        while (accumulator >= PhysicsDT)
        {
            PhysicsStep(PhysicsDT);
            accumulator -= PhysicsDT;
        }
    }
    #endregion

    #region Draw Frame
    private void DrawFrame(int drawX, int drawY, int minX, int maxX, int minY, int maxY)
    {
        bool trail1IsCurrent = trail1X == drawX && trail1Y == drawY;
        bool trail2IsCurrent = trail2X == drawX && trail2Y == drawY;
        bool trail2EqualsTrail1 = trail2X == trail1X && trail2Y == trail1Y;

        EraseOldestTrailIfNeeded(minX, maxX, minY, maxY, trail2IsCurrent, trail2EqualsTrail1);
        DrawNewerTrailIfNeeded(minX, maxX, minY, maxY, trail1IsCurrent);
        DrawCurrentBallIfInBounds(drawX, drawY, minX, maxX, minY, maxY);
        ShiftTrailsIfMoved(drawX, drawY, trail1IsCurrent);
    }
    #endregion

    #region Erase Oldest Trail If Needed
    private void EraseOldestTrailIfNeeded(int minX, int maxX, int minY, int maxY, bool trail2IsCurrent, bool trail2EqualsTrail1)
    {
        if (!trail2IsCurrent && !trail2EqualsTrail1 &&
            trail2X >= minX && trail2X <= maxX && trail2Y >= minY && trail2Y <= maxY)
        {
            Console.SetCursorPosition(trail2X, trail2Y);
            Console.Write(' ');
        }
    }
    #endregion

    #region Draw Newer Trail If Needed
    private void DrawNewerTrailIfNeeded(int minX, int maxX, int minY, int maxY, bool trail1IsCurrent)
    {
        if (!trail1IsCurrent &&
            trail1X >= minX && trail1X <= maxX && trail1Y >= minY && trail1Y <= maxY)
        {
            Console.SetCursorPosition(trail1X, trail1Y);
            Console.Write('o');
        }
    }
    #endregion

    #region Draw Current Ball If In Bounds
    private void DrawCurrentBallIfInBounds(int drawX, int drawY, int minX, int maxX, int minY, int maxY)
    {
        if (drawX >= minX && drawX <= maxX && drawY >= minY && drawY <= maxY)
        {
            Console.SetCursorPosition(drawX, drawY);
            Console.Write('O');
        }
    }
    #endregion

    #region Shift Trails If Moved
    private void ShiftTrailsIfMoved(int drawX, int drawY, bool trail1IsCurrent)
    {
        if (!trail1IsCurrent)
        {
            trail2X = trail1X; trail2Y = trail1Y;
            trail1X = drawX; trail1Y = drawY;
        }
    }
    #endregion

    #region Physics Step
    private void PhysicsStep(double dt)
    {
        int minX = 1, maxX = Width - 2;
        int minY = 1, maxY = Height - 2;

        double dx = BallVelX * BallSpeed * dt;
        double dy = BallVelY * BallSpeed * dt;

        for (int iter = 0; iter < 4; iter++)
        {
            double nx = BallPosX + dx;
            double ny = BallPosY + dy;

            if (HandleVerticalWallReflection(minY, maxY, ref dx, ref dy, nx, ny))
            {
                continue;
            }

            int p1Line = Paddle1X + 1;
            int p2Line = Paddle2X - 1;

            if (TryLeftPaddleHit(dx, p1Line, ref dx, ref dy, dt))
            {
                continue;
            }

            if (TryRightPaddleHit(dx, p2Line, ref dx, ref dy, dt))
            {
                continue;
            }

            BallPosX = nx;
            BallPosY = ny;
            break;
        }

        HandleGoalIfAny(minX, maxX);
    }
    #endregion

    #region Handle Vertical Wall Reflection
    private bool HandleVerticalWallReflection(int minY, int maxY, ref double dx, ref double dy, double nx, double ny)
    {
        if (ny < minY)
        {
            double t = (minY - BallPosY) / (ny - BallPosY);
            BallPosX += dx * t;
            BallPosY = minY;
            dy = (1 - t) * (-dy);
            BallVelY = -BallVelY;
            return true;
        }
        if (ny > maxY)
        {
            double t = (maxY - BallPosY) / (ny - BallPosY);
            BallPosX += dx * t;
            BallPosY = maxY;
            dy = (1 - t) * (-dy);
            BallVelY = -BallVelY;
            return true;
        }
        return false;
    }
    #endregion

    #region Try Left Paddle Hit
    private bool TryLeftPaddleHit(double dx, int p1Line, ref double ndx, ref double ndy, double dt)
    {
        if (dx < 0 && BallPosX > p1Line && BallPosX + dx <= p1Line)
        {
            double t = (p1Line - BallPosX) / dx;
            double yAt = BallPosY + (BallVelY * BallSpeed * dt * t);

            int p1Top = Player1Height - (PaddleHeight / 2);
            int p1Bot = Player1Height + (PaddleHeight / 2);

            if (yAt >= p1Top && yAt <= p1Bot)
            {
                BallPosX = p1Line;
                BallPosY = yAt;

                double offset = (yAt - Player1Height) / (PaddleHeight / 2);
                BallVelX = -BallVelX;
                BallVelY += 0.6 * offset;
                Normalize(ref BallVelX, ref BallVelY);

                double remain = 1 - t;
                ndx = BallVelX * BallSpeed * dt * remain;
                ndy = BallVelY * BallSpeed * dt * remain;
                return true;
            }
        }
        return false;
    }
    #endregion

    #region Try Right Paddle Hit
    private bool TryRightPaddleHit(double dx, int p2Line, ref double ndx, ref double ndy, double dt)
    {
        if (dx > 0 && BallPosX < p2Line && BallPosX + dx >= p2Line)
        {
            double t = (p2Line - BallPosX) / dx;
            double yAt = BallPosY + (BallVelY * BallSpeed * dt * t);

            int p2Top = Player2Height - (PaddleHeight / 2);
            int p2Bot = Player2Height + (PaddleHeight / 2);

            if (yAt >= p2Top && yAt <= p2Bot)
            {
                BallPosX = p2Line;
                BallPosY = yAt;

                double offset = (yAt - Player2Height) / (PaddleHeight / 2);
                BallVelX = -BallVelX;
                BallVelY += 0.6 * offset;
                Normalize(ref BallVelX, ref BallVelY);

                double remain = 1 - t;
                ndx = BallVelX * BallSpeed * dt * remain;
                ndy = BallVelY * BallSpeed * dt * remain;
                return true;
            }
        }
        return false;
    }
    #endregion

    #region Handle Goal If Any
    private void HandleGoalIfAny(int minX, int maxX)
    {
        if (BallPosX <= minX || BallPosX >= maxX)
        {
            // --- scoring hook ---
            if (BallPosX <= minX) Player2Score++; // right player scores
            else Player1Score++; // left player scores
            DrawScore(); // update UI

            BallPosX = Width / 2.0;
            BallPosY = Height / 2.0;
            BallVelX = (BallPosX <= minX) ? 1 : -1;
            BallVelY = (Rng.NextDouble() * 2) - 1;
            Normalize(ref BallVelX, ref BallVelY);
        }
    }
    #endregion

    #region Update Ball (Discrete)

    public void UpdateBall(double deltaTime)
    {
        double minX = 1, maxX = Width - 2;
        double minY = 1, maxY = Height - 2;

        double vx = BallVelocityX, vy = BallVelocityY;
        Normalize(ref vx, ref vy);

        double stepX = vx * BallSpeed * deltaTime;
        double stepY = vy * BallSpeed * deltaTime;

        double nx = BallX + stepX;
        double ny = BallY + stepY;

        ReflectDiscreteVertical(minY, maxY, ref vy, ref ny);

        if (HitCollision(nx, ny, ref vx, ref vy))
        {
            nx = BallX + (vx * BallSpeed * deltaTime);
            ny = BallY + (vy * BallSpeed * deltaTime);
        }

        if (HandleDiscreteGoalIfAny(nx, minX, maxX, minY, maxY, ref vx, ref vy))
        {
            return;
        }

        int drawX = (int)Math.Round(nx);
        int drawY = (int)Math.Round(ny);

        bool trail1IsCurrent = trail1X == drawX && trail1Y == drawY;
        bool trail2IsCurrent = trail2X == drawX && trail2Y == drawY;
        bool trail2EqualsTrail1 = trail2X == trail1X && trail2Y == trail1Y;

        EraseOldestTrailIfNeeded((int)minX, (int)maxX, (int)minY, (int)maxY, trail2IsCurrent, trail2EqualsTrail1);
        DrawNewerTrailIfNeeded((int)minX, (int)maxX, (int)minY, (int)maxY, trail1IsCurrent);
        DrawCurrentBallIfInBounds(drawX, drawY, (int)minX, (int)maxX, (int)minY, (int)maxY);
        ShiftTrailsIfMoved(drawX, drawY, trail1IsCurrent);

        BallX = drawX;
        BallY = drawY;

        BallVelocityX = vx;
        BallVelocityY = vy;
    }
    #endregion

    #region Reflect Discrete Vertical
    private void ReflectDiscreteVertical(double minY, double maxY, ref double vy, ref double ny)
    {
        if (ny < minY) { vy = -vy; ny = minY + (minY - ny); }
        else if (ny > maxY) { vy = -vy; ny = maxY - (ny - maxY); }
    }
    #endregion

    #region Handle Discrete Goal If Any
    private bool HandleDiscreteGoalIfAny(double nx, double minX, double maxX, double minY, double maxY, ref double vx, ref double vy)
    {
        if (nx <= minX || nx >= maxX)
        {
            if (nx <= minX) Player2Score++;
            else Player1Score++;
            DrawScore();

            ClearAllBallGlyphs((int)minX, (int)maxX, (int)minY, (int)maxY);

            BallX = Width / 2;
            BallY = Height / 2;

            vx = (nx <= minX) ? 1 : -1;
            vy = 0;
            Normalize(ref vx, ref vy);

            BallVelocityX = vx;
            BallVelocityY = vy;

            trail1X = trail1Y = trail2X = trail2Y = -1;
            return true;
        }
        return false;
    }
    #endregion

    #region Clear All Ball Glyphs
    private void ClearAllBallGlyphs(int minX, int maxX, int minY, int maxY)
    {
        if (trail2X >= minX && trail2X <= maxX && trail2Y >= minY && trail2Y <= maxY)
        { Console.SetCursorPosition(trail2X, trail2Y); Console.Write(' '); }
        if (trail1X >= minX && trail1X <= maxX && trail1Y >= minY && trail1Y <= maxY)
        { Console.SetCursorPosition(trail1X, trail1Y); Console.Write(' '); }
        if (BallX >= minX && BallX <= maxX && BallY >= minY && BallY <= maxY)
        { Console.SetCursorPosition((int)BallX, (int)BallY); Console.Write(' '); }
    }
    #endregion

    #region Hit Collision
    public bool HitCollision(double nx, double ny, ref double vx, ref double vy)
    {
        int y = (int)Math.Round(ny);

        int p1Top = Player1Height - (PaddleHeight / 2);
        int p1Bot = Player1Height + (PaddleHeight / 2);
        int p2Top = Player2Height - (PaddleHeight / 2);
        int p2Bot = Player2Height + (PaddleHeight / 2);

        if (vx < 0 && (int)Math.Round(nx) <= Paddle1X + 1 && y >= p1Top && y <= p1Bot)
        {
            vx = -vx;
            double offset = (y - Player1Height) / (double)(PaddleHeight / 2);
            vy += 0.6 * offset;
            Normalize(ref vx, ref vy);
            return true;
        }

        if (vx > 0 && (int)Math.Round(nx) >= Paddle2X - 1 && y >= p2Top && y <= p2Bot)
        {
            vx = -vx;
            double offset = (y - Player2Height) / (double)(PaddleHeight / 2);
            vy += 0.6 * offset;
            Normalize(ref vx, ref vy);
            return true;
        }

        return false;
    }
    #endregion

    #region Normalize
    private static void Normalize(ref double vx, ref double vy)
    {
        double len = Math.Sqrt((vx * vx) + (vy * vy));
        if (len < 1e-6) { vx = 0.70710678; vy = 0.70710678; return; }
        vx /= len; vy /= len;
    }
    #endregion
}
