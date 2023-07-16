using System;
using Core.World;
using Hexxy.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Hexxy;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private Camera _camera;
    private World _world;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        IsFixedTimeStep = true;
        TargetElapsedTime = TimeSpan.FromMilliseconds(12);
    }

    protected override void Initialize()
    {
        _graphics.PreferredBackBufferWidth = Global.BackBufferWidth;
        _graphics.PreferredBackBufferHeight = Global.BackBufferHeight;
        _graphics.ApplyChanges();

        _camera = new();
        _world = new();

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _world.GenerateWorld(Content);
        Global.Font = Content.Load<SpriteFont>("font/Font");
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        if (Keyboard.GetState().IsKeyDown(Keys.A))
            _camera.Update(new Vector2(-6f, 0));
        if (Keyboard.GetState().IsKeyDown(Keys.D))
            _camera.Update(new Vector2(6f, 0));
        if (Keyboard.GetState().IsKeyDown(Keys.W))
            _camera.Update(new Vector2(0, -6f));
        if (Keyboard.GetState().IsKeyDown(Keys.S))
            _camera.Update(new Vector2(0, 6f));
        if (Keyboard.GetState().IsKeyDown(Keys.Q))
            _camera.Zoom -= _camera.Zoom <= 0.3f ? 0 : 0.1f;
        if (Keyboard.GetState().IsKeyDown(Keys.E))
            _camera.Zoom += _camera.Zoom >= 5f ? 0 : 0.1f;

        if (Keyboard.GetState().IsKeyDown(Keys.U))
            LoadContent();
        if (Keyboard.GetState().IsKeyDown(Keys.B))
            Global.WaterLevel += 0.01f;
        if (Keyboard.GetState().IsKeyDown(Keys.N))
            Global.WaterLevel -= 0.01f;

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _camera.Draw(_spriteBatch);

        foreach (var hex in _world.Hexs)
        {
            hex.Draw(_spriteBatch);
        }

        _spriteBatch.DrawString(Global.Font, $"Water: {Global.WaterLevel}", _camera.Position - new Vector2(3000, 500), Color.Black, 0, Vector2.Zero, 5f, SpriteEffects.None, 0);
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}
