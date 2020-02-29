using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Map_Generator
{
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        KeyboardState lastKeyboardState;

        GridGenerator generator;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            generator = new GridGenerator();
        }

        protected override void Initialize()
        {
            base.Initialize();
            generator.GenerateGrid();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            generator.LoadContent(this);
        }       

        protected override void Update(GameTime gameTime)
        {
            KeyboardState keyboard = Keyboard.GetState();

            //&& lastKeyboardState.IsKeyUp(Keys.G)
            if (keyboard.IsKeyDown(Keys.G) )
                generator.GenerateGrid();

            lastKeyboardState = keyboard;

            base.Update(gameTime);            
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();
            generator.DrawGrid(spriteBatch);
            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
