using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Pong
{
    public class PongGameLoop : Game
    {
        private GraphicsDeviceManager _graphics; // can be used to switch to fullscreen
        private SpriteBatch _spriteBatch;

        // View - Components
        SpriteFont font;
        Texture2D background;

        // Input
        KeyboardState keyboardState, previousKeyboardState;

        // Global Variables - Screen
        public const int
            WINDOW_WIDTH = 1120,
            WINDOW_HEIGHT = 725,
            BLOCK_WIDTH = 20,
            BLOCK_HEIGHT = 120,
            BLOCK_SPEED = 12,
            LEFT_BLOCK_X = 10,
            RIGHT_BLOCK_X = WINDOW_WIDTH - BLOCK_WIDTH - 10,
            STARTING_Y = 250,
            BALL_SIZE = 20,
            BALL_X = 275,
            BALL_Y = 200,
            BALL_VELOCITY_X = 5,
            BALL_VELOCITY_Y = 4,
            LOWER_BOUND = 630,
            LEFT_SERVE_X = 300,
            LEFT_SERVE_Y = 300,
            RIGHT_SERVE_X = 700,
            GAME_OVER = 10,
            RIGHT_SERVE_Y = 300,
            GAMEOVER_LEFT = 620,
            GAMEOVER_RIGHT = 100;

        public int bounces = 0;
        public bool spedUp = false;

        // Game Clock
        double timeLast, timeNow, elapsedTime;



        Texture2D blockTex, ballTex;
        Block Left, Right;

        Ball b = new Ball(new Vector2(BALL_X,BALL_Y), new Velocity(BALL_VELOCITY_X, BALL_VELOCITY_Y), BALL_SIZE);


        public PongGameLoop()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = false;
        }

        protected override void Initialize()
        {
            // window logic
            _graphics.PreferredBackBufferWidth = WINDOW_WIDTH;
            _graphics.PreferredBackBufferHeight = WINDOW_HEIGHT;
            _graphics.IsFullScreen = false;
            _graphics.ApplyChanges();

            // game logic
            timeLast = 0;
            timeNow = 0;

            Left = new Block(new Vector2(LEFT_BLOCK_X, STARTING_Y), true, BLOCK_HEIGHT, BLOCK_WIDTH);
            Right = new Block(new Vector2(RIGHT_BLOCK_X, STARTING_Y), false, BLOCK_HEIGHT, BLOCK_WIDTH);


            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            background = Content.Load<Texture2D>(@"Sprites\background");
            font = Content.Load<SpriteFont>(@"Fonts\font");

            blockTex = new Texture2D(GraphicsDevice, 1, 1);
            blockTex.SetData(new[] { Color.White });

            ballTex = Content.Load<Texture2D>(@"Sprites\ball");
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape) || Keyboard.GetState().IsKeyDown(Keys.Back))
                Exit();

            // below is for toggling full screen using Enter button. I would avoid at all costs. Easier to just set a good max screen
            //if (Keyboard.GetState().IsKeyDown(Keys.Enter) && previousState != null && previousState.IsKeyUp(Keys.Enter))
            //    _graphics.ToggleFullScreen();

            previousKeyboardState = keyboardState;
            keyboardState = Keyboard.GetState();

            timeNow = gameTime.TotalGameTime.TotalSeconds;
            elapsedTime = timeNow - timeLast;

            if (Left.Points >= GAME_OVER || Right.Points >= GAME_OVER)
            {
                if (previousKeyboardState.IsKeyDown(Keys.Space))
                {
                    if (Left.Points >= GAME_OVER)
                        b.Reset(new Vector2(RIGHT_SERVE_X, RIGHT_SERVE_Y), new Velocity(-BALL_VELOCITY_X, BALL_VELOCITY_Y));
                    else
                        b.Reset(new Vector2(LEFT_SERVE_X, LEFT_SERVE_Y), new Velocity(BALL_VELOCITY_X, BALL_VELOCITY_Y));
                    bounces = 0;
                    spedUp = false;
                    DoGameOver();
                } 
            }
            else
            {
                // Move Left block (W-up,S-down)
                if (Left.WithinBoundaries(LOWER_BOUND))
                {
                    if (previousKeyboardState.IsKeyDown(Keys.S))
                    {
                        Left.Move(false, BLOCK_SPEED);
                    }
                    else if (previousKeyboardState.IsKeyDown(Keys.W))
                    {
                        Left.Move(true, BLOCK_SPEED);
                    }
                    // over boundaries check
                    if (!Left.WithinBoundaries(LOWER_BOUND))
                    {
                        if ((Left.V.Y + BLOCK_HEIGHT) > LOWER_BOUND)
                            Left.V.Y = LOWER_BOUND - BLOCK_HEIGHT;
                        else
                            Left.V.Y = 0;
                    }
                }

                // Move Right block (Up_key-up,Down_key-down)
                if (Right.WithinBoundaries(LOWER_BOUND))
                {
                    if (previousKeyboardState.IsKeyDown(Keys.Down))
                    {
                        Right.Move(false, BLOCK_SPEED);
                    }
                    else if (previousKeyboardState.IsKeyDown(Keys.Up))
                    {
                        Right.Move(true, BLOCK_SPEED);
                    }
                    // over boundaries check
                    if (!Right.WithinBoundaries(LOWER_BOUND))
                    {
                        if ((Right.V.Y + BLOCK_HEIGHT) > LOWER_BOUND)
                            Right.V.Y = LOWER_BOUND - BLOCK_HEIGHT;
                        else
                            Right.V.Y = 0;
                    }
                }

                if (b.WithinBoundaries(WINDOW_WIDTH,LOWER_BOUND))
                    b.Move();
            
                if (!b.WithinVerticalBoundaries(LOWER_BOUND))
                {
                    if ((b.v2.Y + b.size) > LOWER_BOUND)
                        b.v2.Y = LOWER_BOUND - b.size;
                    else
                        b.v2.Y = 0;

                    b.V.Y = -b.V.Y;
                }

                if (b.TouchingBlock(Right) || b.TouchingBlock(Left))
                {
                    if (b.TouchingSideOfBlock(Right) || b.TouchingSideOfBlock(Left))
                        b.V.Y = -b.V.Y;
                    else 
                        b.V.X = -b.V.X;
                    bounces++;

                    if ((bounces / 4 > 0 && bounces / 8 < 1) && !spedUp)
                    {
                        b.SpeedUp();
                        spedUp = true;
                    }
                    else if ((bounces / 8 > 0 && bounces / 12 < 1) && spedUp)
                    {
                        b.SpeedUp();
                        spedUp = false;
                    } 
                    else if ((bounces / 12 > 0 && bounces / 16 < 1) && !spedUp)
                    {
                        b.SpeedUp();
                        spedUp = true;
                    }
                }

                // scored a point
                if (!b.WithinBoundaries(WINDOW_WIDTH, LOWER_BOUND)) 
                {
                    if (b.v2.X + b.size >= WINDOW_WIDTH)
                    {
                        Left.Points++;
                        b.Reset(new Vector2(RIGHT_SERVE_X, RIGHT_SERVE_Y), new Velocity(-BALL_VELOCITY_X, BALL_VELOCITY_Y));
                    }
                    else
                    {
                        Right.Points++;
                        b.Reset(new Vector2(LEFT_SERVE_X, LEFT_SERVE_Y), new Velocity(BALL_VELOCITY_X, BALL_VELOCITY_Y));
                    }
                    bounces = 0;
                    spedUp = false;
                }

            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin();

            _spriteBatch.Draw(background, new Vector2(0, 0), Color.White);

            _spriteBatch.Draw(blockTex, new Rectangle((int)Left.V.X, (int)Left.V.Y, BLOCK_WIDTH, BLOCK_HEIGHT), Color.White);
            _spriteBatch.Draw(blockTex, new Rectangle((int)Right.V.X, (int)Right.V.Y, BLOCK_WIDTH, BLOCK_HEIGHT), Color.White);

            _spriteBatch.Draw(ballTex, new Rectangle((int)b.v2.X, (int)b.v2.Y, b.size, b.size), Color.White);

            _spriteBatch.DrawString(font, Left.Points.ToString(), new Vector2(500, 630), Color.White);
            _spriteBatch.DrawString(font, Right.Points.ToString(), new Vector2(600, 630), Color.White);

            if (Left.Points >= GAME_OVER || Right.Points >= GAME_OVER)
            {
                _spriteBatch.DrawString(font, "Game Over! " + (Left.Points >= GAME_OVER ? "Left" : "Right")  + " Player Won!\nPress Space to start over", new Vector2(Left.Points >= GAME_OVER ? GAMEOVER_LEFT : GAMEOVER_RIGHT, 200), Color.White);
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        protected void DoGameOver()
        {
            Left.Points = 0;
            Right.Points = 0;
        }

    }

    public struct Orientation
    {

        public Orientation(bool left)
        {
            Left = left;
            Right = !left;
        }


        public bool Left { get; set; }
        public bool Right { get; set; }
    }

    public struct Velocity
    { 
        public Velocity(float x, float y)
        {
            X = x;
            Y = y;
        }

        public float X { get; set; }
        public float Y { get; set; }

        public Velocity ResetVelocity()
        {
            return new Velocity(0, 0);
        }
    }


    public class Ball
    {
        private System.Random gen = new System.Random();
        public Vector2 v2;
        public Velocity V;
        public int size;

        public Ball(Vector2 starting, Velocity initial, int S)
        {
            v2 = starting;
            V = initial;
            size = S;
        }

        public void Move()
        {
            v2.X += V.X;
            v2.Y += V.Y;
        }

        public void Reset(Vector2 reset_Vector, Velocity reset_Velocity)
        {
            v2 = reset_Vector;
            V = reset_Velocity;
        }

        public void SpeedUp()
        {
            float speedUp = 1 + ((float)gen.Next(10,50) / 100);
            V.X *= speedUp;
            V.Y *= speedUp;
        }

        public bool WithinVerticalBoundaries(int bounds)
        {
            return v2.Y >= 0 && (v2.Y + size) <= bounds;
        }

        public bool WithinBoundaries(int boundX, int boundY)
        {
            var y = v2.Y >= 0 && (v2.Y + size) <= boundY;
            var x = v2.X >= 0 && (v2.X + size) <= boundX;

            return x && y;
        }

        public bool TouchingBlock(Block b)
        {
            bool x_side, y_top, y_bottom;
            if (b.O.Right)
            {
                x_side = (v2.X + size) >= b.V.X && (v2.X + size) <= (b.V.X + b.Width);
                y_top = (v2.Y >= b.V.Y && v2.Y <= (b.V.Y + b.Height));
                y_bottom = (v2.Y + size) >= b.V.Y && (v2.Y + size) <= (b.V.Y + b.Height);
            }
            else
            {
                x_side = v2.X >= b.V.X && v2.X <= (b.V.X + b.Width);
                y_top = v2.Y >= b.V.Y && v2.Y <= (b.V.Y + b.Height);
                y_bottom = (v2.Y + size) >= b.V.Y && (v2.Y + size) <= (b.V.Y + b.Height);
            }
            return x_side && (y_top || y_bottom);
        }

        public bool TouchingSideOfBlock(Block b)
        {
            bool y_top, y_bottom;
            if (b.O.Right)
            {
                y_top = (v2.Y >= b.V.Y && v2.Y <= (b.V.Y + b.Height));
                y_bottom = (v2.Y + size) >= b.V.Y && (v2.Y + size) <= (b.V.Y + b.Height);
            }
            else
            {
                y_top = v2.Y >= b.V.Y && v2.Y <= (b.V.Y + b.Height);
                y_bottom = (v2.Y + size) >= b.V.Y && (v2.Y + size) <= (b.V.Y + b.Height);
            }
            return y_top ^ y_bottom;
        }
    }
}
