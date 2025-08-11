using H1_basic_programming_final_project.Core.DataModels;
using System.Diagnostics;

namespace H1_basic_programming_final_project.Core.Pages;

public sealed class PingPongPage : Page
{
    #region Members

    #region Singleton
    private static readonly Lazy<PingPongPage> _instance = new(() => new PingPongPage(), true);
    public static readonly PingPongPage Instance = _instance.Value;
    #endregion

    #region Propertie
    public int Width;
    public int Height;

    public int Player1Height = 0;
    public int Player2Height = 0;

    private const int PaddleHeight = 6;
    private int Paddle1X => 2;
    private int Paddle2X => Width - 3; // inside right wall
    private static readonly Random Rng = new();

    private const double BallSpeed = 50.0;

    public int lastDrawX = -1, lastDrawY = -1;
    public int trail1X = -1, trail1Y = -1; // most recent trail
    public int trail2X = -1, trail2Y = -1; // older trail

    #endregion

    #endregion

    #region Constructor
    private PingPongPage()
    {

    }
    #endregion

    #region Build
    public override void Build(PageBuilder pageBuilder)
    {
        pageBuilder.PrintTitle = true;
        pageBuilder.Title = "Ping Pong";

        pageBuilder.PrintDescription = true;
        pageBuilder.Description = "Welcome to the most amazing PingPo";

        pageBuilder.RepeatPageCycle = true;

        pageBuilder.AddPageArgument("Start the game!", StartGame, "Start");
        pageBuilder.AddExitPageArgument();
    }
    #endregion

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

    #region Draw Player
    public void DrawPlayer(int x, int y, int height, char character)
    {
        for (int i = -height / 2; i < height / 2; i++)
        {
            int yy = y + i;
            if (yy <= 0 || yy >= Height - 1) continue;
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
            Player1Height = Math.Clamp(Player1Height, 1 + PaddleHeight / 2, Height - 2 - PaddleHeight / 2);
            DrawPlayer(Paddle1X, Player1Height, PaddleHeight, '|');
            return;
        }

        if (player == 2)
        {
            DrawPlayer(Paddle2X, Player2Height, PaddleHeight, ' ');
            Player2Height += up ? -1 : 1;
            Player2Height = Math.Clamp(Player2Height, 1 + PaddleHeight / 2, Height - 2 - PaddleHeight / 2);
            DrawPlayer(Paddle2X, Player2Height, PaddleHeight, '|');
            return;
        }
    }

    #endregion

    #region Start Game
    // Fields: replace your int BallX/Y and keep doubles for pos.
    private double BallPosX, BallPosY;      // continuous position
    private double BallVelX, BallVelY;      // unit vector
    private const double PhysicsDT = 1.0 / 240.0; // stable physics rate
    private const double MaxFrameClamp = 0.1;     // clamp big spikes
    public void StartGame()
    {
        Width = Console.WindowWidth;
        Height = Console.WindowHeight;

        Player1Height = Height / 2;
        Player2Height = Height / 2;

        Console.Clear();
        Console.CursorVisible = false;
        DrawBorder();

        DrawPlayer(Paddle1X, Player1Height, PaddleHeight, '|');
        DrawPlayer(Paddle2X, Player2Height, PaddleHeight, '|');

        System.Threading.Tasks.Task.Run(RawInput.Poll);

        var sw = Stopwatch.StartNew();

        BallPosX = Width / 2.0;
        BallPosY = Height / 2.0;
        BallVelX = Rng.NextDouble() * 2 - 1;
        BallVelY = 0;
        Normalize(ref BallVelX, ref BallVelY);

        const int moveIntervalMs = 35;
        long p1Last = sw.ElapsedMilliseconds, p2Last = sw.ElapsedMilliseconds;
        long lastTicks = sw.ElapsedTicks;
        double accumulator = 0.0;

        int lastDrawX = -1, lastDrawY = -1;
        trail1X = trail1Y = trail2X = trail2Y = -1;

        while (true)
        {
            long nowMs = sw.ElapsedMilliseconds;
            Player1Height = Math.Clamp(Player1Height, 1 + PaddleHeight / 2, Height - 2 - PaddleHeight / 2);
            Player2Height = Math.Clamp(Player2Height, 1 + PaddleHeight / 2, Height - 2 - PaddleHeight / 2);

            if (nowMs - p1Last >= moveIntervalMs) { p1Last = nowMs; if (RawInput.WDown) ReDrawPlayer(1, true); if (RawInput.SDown) ReDrawPlayer(1, false); }
            if (nowMs - p2Last >= moveIntervalMs) { p2Last = nowMs; if (RawInput.UpDown) ReDrawPlayer(2, true); if (RawInput.DownDown) ReDrawPlayer(2, false); }

            long nowTicks = sw.ElapsedTicks;
            double dt = (nowTicks - lastTicks) / (double)Stopwatch.Frequency;
            lastTicks = nowTicks;
            if (dt > MaxFrameClamp) dt = MaxFrameClamp;

            accumulator += dt;
            while (accumulator >= PhysicsDT)
            {
                PhysicsStep(PhysicsDT);
                accumulator -= PhysicsDT;
            }

            int minX = 1, maxX = Width - 2;
            int minY = 1, maxY = Height - 2;

            int drawX = (int)Math.Round(BallPosX);
            int drawY = (int)Math.Round(BallPosY);

            bool trail1IsCurrent = (trail1X == drawX && trail1Y == drawY);
            bool trail2IsCurrent = (trail2X == drawX && trail2Y == drawY);
            bool trail2EqualsTrail1 = (trail2X == trail1X && trail2Y == trail1Y);

            // erase oldest trail only if distinct from newer and current
            if (!trail2IsCurrent && !trail2EqualsTrail1 &&
                trail2X >= minX && trail2X <= maxX && trail2Y >= minY && trail2Y <= maxY)
            {
                Console.SetCursorPosition(trail2X, trail2Y);
                Console.Write(' ');
            }

            // draw newer trail only if not the current cell
            if (!trail1IsCurrent &&
                trail1X >= minX && trail1X <= maxX && trail1Y >= minY && trail1Y <= maxY)
            {
                Console.SetCursorPosition(trail1X, trail1Y);
                Console.Write('o');
            }

            // draw current ball
            if (drawX >= minX && drawX <= maxX && drawY >= minY && drawY <= maxY)
            {
                Console.SetCursorPosition(drawX, drawY);
                Console.Write('O');
            }

            // shift only when the ball advanced to a new cell
            if (!trail1IsCurrent)
            {
                trail2X = trail1X; trail2Y = trail1Y;
                trail1X = drawX; trail1Y = drawY;
            }

            lastDrawX = drawX;
            lastDrawY = drawY;
        }
    }



    // Continuous physics with segment tests and mirroring
    private void PhysicsStep(double dt)
    {
        int minX = 1, maxX = Width - 2;
        int minY = 1, maxY = Height - 2;

        double dx = BallVelX * BallSpeed * dt;
        double dy = BallVelY * BallSpeed * dt;

        // Process up to 4 reflections per step for stability
        for (int iter = 0; iter < 4; iter++)
        {
            double nx = BallPosX + dx;
            double ny = BallPosY + dy;

            // Top/bottom continuous reflection
            if (ny < minY)
            {
                double t = (minY - BallPosY) / (ny - BallPosY); // 0..1
                BallPosX += dx * t;
                BallPosY = minY;
                dy = (1 - t) * (-dy); // reflect Y, consume time
                BallVelY = -BallVelY;
                continue;
            }
            if (ny > maxY)
            {
                double t = (maxY - BallPosY) / (ny - BallPosY);
                BallPosX += dx * t;
                BallPosY = maxY;
                dy = (1 - t) * (-dy);
                BallVelY = -BallVelY;
                continue;
            }

            // Paddle continuous collision: vertical line intersection if moving toward it
            int p1Line = Paddle1X + 1;
            int p2Line = Paddle2X - 1;

            // left paddle
            if (dx < 0 && BallPosX > p1Line && nx <= p1Line)
            {
                double t = (p1Line - BallPosX) / dx;           // 0..1
                double yAt = BallPosY + dy * t;
                int p1Top = Player1Height - PaddleHeight / 2;
                int p1Bot = Player1Height + PaddleHeight / 2;

                if (yAt >= p1Top && yAt <= p1Bot)
                {
                    // move to impact
                    BallPosX = p1Line;
                    BallPosY = yAt;

                    // tweak angle by hit offset
                    double offset = (yAt - Player1Height) / (double)(PaddleHeight / 2); // [-1..1]
                    BallVelX = -BallVelX;
                    BallVelY += 0.6 * offset;
                    Normalize(ref BallVelX, ref BallVelY);

                    // remaining time after impact
                    double remain = 1 - t;
                    dx = BallVelX * BallSpeed * dt * remain;
                    dy = BallVelY * BallSpeed * dt * remain;
                    continue;
                }
            }

            // right paddle
            if (dx > 0 && BallPosX < p2Line && nx >= p2Line)
            {
                double t = (p2Line - BallPosX) / dx;
                double yAt = BallPosY + dy * t;
                int p2Top = Player2Height - PaddleHeight / 2;
                int p2Bot = Player2Height + PaddleHeight / 2;

                if (yAt >= p2Top && yAt <= p2Bot)
                {
                    BallPosX = p2Line;
                    BallPosY = yAt;

                    double offset = (yAt - Player2Height) / (double)(PaddleHeight / 2);
                    BallVelX = -BallVelX;
                    BallVelY += 0.6 * offset;
                    Normalize(ref BallVelX, ref BallVelY);

                    double remain = 1 - t;
                    dx = BallVelX * BallSpeed * dt * remain;
                    dy = BallVelY * BallSpeed * dt * remain;
                    continue;
                }
            }

            // No reflections this substep: advance and finish
            BallPosX = nx;
            BallPosY = ny;
            break;
        }

        // Goal check after movement
        if (BallPosX <= minX || BallPosX >= maxX)
        {
            // serve from center toward scorer
            BallPosX = Width / 2.0;
            BallPosY = Height / 2.0;
            BallVelX = (BallPosX <= minX) ? 1 : -1;
            BallVelY = Rng.NextDouble() * 2 - 1;
            Normalize(ref BallVelX, ref BallVelY);
        }
    }

    #endregion

    #region Update Ball
    public double BallX = 0; // Get sets to center of screen upon startup.
    public double BallY = 0; // Get sets to center of screen upon startup.
    public double BallVelocityX = 0; // Horizontal velocity of the ball. // Gets a random value between -1 and 1. upon startup.
    public double BallVelocityY = 0; // Horizontal velocity of the ball. // Gets a random value between -1 and 1. upon startup.
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

        if (ny < minY) { vy = -vy; ny = minY + (minY - ny); }
        else if (ny > maxY) { vy = -vy; ny = maxY - (ny - maxY); }

        if (HitCollision(nx, ny, ref vx, ref vy))
        {
            nx = BallX + vx * BallSpeed * deltaTime;
            ny = BallY + vy * BallSpeed * deltaTime;
        }

        if (nx <= minX || nx >= maxX)
        {
            // clear any visible trail and ball
            if (trail2X >= minX && trail2X <= maxX && trail2Y >= minY && trail2Y <= maxY)
            { Console.SetCursorPosition(trail2X, trail2Y); Console.Write(' '); }
            if (trail1X >= minX && trail1X <= maxX && trail1Y >= minY && trail1Y <= maxY)
            { Console.SetCursorPosition(trail1X, trail1Y); Console.Write(' '); }
            if (BallX >= minX && BallX <= maxX && BallY >= minY && BallY <= maxY)
            { Console.SetCursorPosition((int)BallX, (int)BallY); Console.Write(' '); }

            BallX = Width / 2;
            BallY = Height / 2;

            vx = (nx <= minX) ? 1 : -1;
            vy = 0;
            Normalize(ref vx, ref vy);

            BallVelocityX = vx;
            BallVelocityY = vy;

            trail1X = trail1Y = trail2X = trail2Y = -1;
            return;
        }

        int drawX = (int)Math.Round(nx);
        int drawY = (int)Math.Round(ny);

        bool trail1IsCurrent = (trail1X == drawX && trail1Y == drawY);
        bool trail2IsCurrent = (trail2X == drawX && trail2Y == drawY);
        bool trail2EqualsTrail1 = (trail2X == trail1X && trail2Y == trail1Y);

        // erase oldest trail only if distinct
        if (!trail2IsCurrent && !trail2EqualsTrail1 &&
            trail2X >= minX && trail2X <= maxX && trail2Y >= minY && trail2Y <= maxY)
        {
            Console.SetCursorPosition(trail2X, trail2Y);
            Console.Write(' ');
        }

        // draw newer trail only if not current
        if (!trail1IsCurrent &&
            trail1X >= minX && trail1X <= maxX && trail1Y >= minY && trail1Y <= maxY)
        {
            Console.SetCursorPosition(trail1X, trail1Y);
            Console.Write('o');
        }

        // draw current ball
        if (drawX >= minX && drawX <= maxX && drawY >= minY && drawY <= maxY)
        {
            Console.SetCursorPosition(drawX, drawY);
            Console.Write('O');
        }

        // shift only when moved to a new cell
        if (!trail1IsCurrent)
        {
            trail2X = trail1X; trail2Y = trail1Y;
            trail1X = drawX; trail1Y = drawY;
        }

        BallX = drawX;
        BallY = drawY;

        BallVelocityX = vx;
        BallVelocityY = vy;
    }

    #endregion

    public bool HitCollision(double nx, double ny, ref double vx, ref double vy)
    {
        int y = (int)Math.Round(ny);

        // paddle extents
        int p1Top = Player1Height - PaddleHeight / 2;
        int p1Bot = Player1Height + PaddleHeight / 2;
        int p2Top = Player2Height - PaddleHeight / 2;
        int p2Bot = Player2Height + PaddleHeight / 2;

        // left paddle: ball moving left and crossing Paddle1X+1
        if (vx < 0 && (int)Math.Round(nx) <= Paddle1X + 1 && y >= p1Top && y <= p1Bot)
        {
            // reflect X
            vx = -vx;

            // add angle based on where it hit the paddle
            double offset = (y - Player1Height) / (double)(PaddleHeight / 2); // [-1..1]
            vy += 0.6 * offset;
            Normalize(ref vx, ref vy);
            return true;
        }

        // right paddle: ball moving right and crossing Paddle2X-1
        if (vx > 0 && (int)Math.Round(nx) >= Paddle2X - 1 && y >= p2Top && y <= p2Bot)
        {
            vx = -vx;

            double offset = (y - Player2Height) / (double)(PaddleHeight / 2); // [-1..1]
            vy += 0.6 * offset;
            Normalize(ref vx, ref vy);
            return true;
        }

        return false;
    }

    private static void Normalize(ref double vx, ref double vy)
    {
        double len = Math.Sqrt(vx * vx + vy * vy);
        if (len < 1e-6) { vx = 0.70710678; vy = 0.70710678; return; }
        vx /= len; vy /= len;
    }
}
