//-----------------------------------------------------------------------------
// TouchThumbsticksGame.cs
//
// Microsoft Advanced Technology Group
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace TouchThumbsticks
{
    /// <summary>
    /// This sample demonstrates using the touchscreen as a virtual thumbstick control. Each half
    /// of the screen behaves as a virtual thumbstick. When the player touches the screen, their
    /// first point of contact defines the center position; then they can drag away from there
    /// to move the stick.
    /// </summary>
	public class TouchThumbsticksGame : Game
	{
		#region Game Constants

		// desired width of backbuffer
		private const int graphicsWidth = 800;

		// desired height of backbuffer
		private const int graphicsHeight = 480;

		// half of the backbuffer width used for spawning enemies and creating stars
		private const int graphicsWidthHalf = graphicsWidth / 2;

		// half of the backbuffer height used for spawning enemies and creating stars
		private const int graphicsHeightHalf = graphicsHeight / 2;

		// the width of the in-game world
		private const int worldWidth = 1000;

		// the height of the in-game world
		private const int worldHeight = 1000;

		// the number of stars in the background
		private const int numStars = 1000;

		// the thickness of our graphical world border
		private const int worldBorderThickness = 4;

		// double the world border thickness used for positioning the border
		private const int worldBorderThicknessDouble = worldBorderThickness * 2;

		#endregion

        #region Fields

        // the length of time between spawning enemies
		private TimeSpan spawnRate = TimeSpan.FromSeconds(2);

		// color used to draw the world border
		private Color worldBorderColor = Color.Red;

		// create a single Random instance for the game
		private static readonly Random rand = new Random();

		// a SpriteBatch for drawing everything
		private SpriteBatch spriteBatch;

		// a blank 1x1 texture we use for drawing stars and the world border
		private Texture2D blank;

		// a texture used to draw the virtual thumbsticks on the screen
		private Texture2D thumbstick;

		// a timer used for spawning enemies
		private TimeSpan spawnTimer;

		// a list of all the points for our stars. we use the Z component of the
		// Vector3 to hold the size of the star.
		private List<Vector3> stars = new List<Vector3>();

		// the player's ship
		private PlayerShip player;

		// a list of the active enemies
		private List<EnemyShip> enemies = new List<EnemyShip>();

        #endregion

        #region Initialization
        public TouchThumbsticksGame()
		{
			new GraphicsDeviceManager(this)
			{
                PreferredBackBufferHeight = graphicsHeight,
                PreferredBackBufferWidth = graphicsWidth,

				// we only want to be fullscreen on the phone to get rid
				// of the status bar at the top.
#if WINDOWS_PHONE
				IsFullScreen = true
#endif
			};
			Content.RootDirectory = "Content";
            TargetElapsedTime = TimeSpan.FromTicks(333333);
		}

		protected override void LoadContent()
		{
			// create the SpriteBatch for our game
			spriteBatch = new SpriteBatch(GraphicsDevice);

			// load in our textures
			Bullet.Texture = Content.Load<Texture2D>("bullet");
			thumbstick = Content.Load<Texture2D>("thumbstick");

			// create the player's ship
			player = new PlayerShip(Content.Load<Texture2D>("player1"));

			// set the player's world bounds
			player.WorldWidth = worldWidth;
			player.WorldHeight = worldHeight;

			// create our 1x1 blank texture
			blank = new Texture2D(GraphicsDevice, 1, 1);
			blank.SetData(new[] { Color.White });

			// randomly create all of our stars
			for (int i = 0; i < numStars; i++)
			{
				stars.Add(new Vector3(
					(float)rand.NextDouble() * (worldWidth + graphicsWidth) - (worldWidth / 2f + graphicsWidthHalf),
					(float)rand.NextDouble() * (worldWidth + graphicsHeight) - (worldWidth / 2f + graphicsHeightHalf),
					rand.Next(1, 3)));
			}
		}
        #endregion

        #region Update
        protected override void Update(GameTime gameTime)
		{
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

			// update our virtual thumbsticks
			VirtualThumbsticks.Update();

			// countdown until we spawn more enemies
			spawnTimer -= gameTime.ElapsedGameTime;
			if (spawnTimer <= TimeSpan.Zero)
			{
				// we spawn 1-3 enemies per spawn
				int numToSpawn = rand.Next(1, 3);

				for (int i = 0; i < numToSpawn; i++)
				{
					// create the enemy
					EnemyShip enemy = new EnemyShip(Content.Load<Texture2D>("alien"));

					// target the player's ship
					enemy.Player = player;

					// we randomly pick either the left or right side of the screen
					// to place the enemy
					if (rand.Next() % 2 == 0)
					{
						enemy.Position.X = -worldWidth / 2f - (graphicsWidthHalf + 10);
					}
					else
					{
						enemy.Position.X = worldWidth / 2f + (graphicsWidthHalf + 10);
					}

					// we randomly pick either the top or bottom side of the screen
					// to place the enemy
					if (rand.Next() % 2 == 0)
					{
						enemy.Position.Y = -worldHeight / 2f - (graphicsHeightHalf + 10);
					}
					else
					{
						enemy.Position.Y = worldHeight / 2f + (graphicsHeightHalf + 10);
					}

					// add the enemy to our list
					enemies.Add(enemy);
				}

				// reset our timer
				spawnTimer = spawnRate;
			}

			// update the player
			player.Update(gameTime);

			// update all the enemies
			foreach (var enemy in enemies)
				enemy.Update(gameTime);

			// create a couple lists to hold the bullets and enemies
			// we need to remove due to collisions
			List<Bullet> bulletsToRemove = new List<Bullet>();
			List<EnemyShip> enemiesToRemove = new List<EnemyShip>();

			// figure out what bullets and enemies are colliding
			foreach (var b in player.Bullets)
			{
				foreach (var e in enemies)
				{
					// check if the enemy contains the bullet's position
					if (e.ContainsPoint(b.Position))
					{
						// add the bullet and enemy to the lists to be removed
						bulletsToRemove.Add(b);
						enemiesToRemove.Add(e);

						// break the inner loop so bullets only collide
						// with one enemy
						break;
					}
				}
			}

			// remove all marked bullets and enemies
			foreach (var b in bulletsToRemove)
				player.Bullets.Remove(b);
			foreach (var e in enemiesToRemove)
				enemies.Remove(e);

			base.Update(gameTime);
		}
        #endregion

        #region Draw
        protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.Black);

			// figure out a camera transformation based on the player's position
			Matrix cameraTransform = Matrix.CreateTranslation(
				-player.Position.X + graphicsWidthHalf, 
				-player.Position.Y + graphicsHeightHalf,
				0f);

			// begin drawing with SpriteBatch using our camera transformation
			spriteBatch.Begin(
				SpriteSortMode.Deferred,
				BlendState.AlphaBlend,
				SamplerState.LinearClamp,
				DepthStencilState.Default,
				RasterizerState.CullNone,
				null,
				cameraTransform);

			// draw all the stars
			foreach (var star in stars)
			{
				spriteBatch.Draw(blank, new Rectangle((int)star.X, (int)star.Y, (int)star.Z, (int)star.Z), Color.White);
			}

			// draw the world borders
			DrawWorldBorder();

			// draw all of the enemies
			foreach (var enemy in enemies)
				enemy.Draw(spriteBatch);

			// draw the player (which draws the bullets)
			player.Draw(spriteBatch);

			spriteBatch.End();

			// begin a new batch without the camera transformation for our UI
			spriteBatch.Begin();

			// if the user is touching the screen and the thumbsticks have positions,
			// draw our thumbstick sprite so the user knows where the centers are
			if (VirtualThumbsticks.LeftThumbstickCenter.HasValue)
			{
				spriteBatch.Draw(
					thumbstick, 
					VirtualThumbsticks.LeftThumbstickCenter.Value - new Vector2(thumbstick.Width / 2f, thumbstick.Height / 2f), 
					Color.Green);
			}

			if (VirtualThumbsticks.RightThumbstickCenter.HasValue)
			{
				spriteBatch.Draw(
					thumbstick, 
					VirtualThumbsticks.RightThumbstickCenter.Value - new Vector2(thumbstick.Width / 2f, thumbstick.Height / 2f), 
					Color.Blue);
			}

			spriteBatch.End();

			base.Draw(gameTime);
		}

		/// <summary>
		/// Draws four rectangles using the blank texture to represent our world bounds.
		/// </summary>
		private void DrawWorldBorder()
		{
			Rectangle r = new Rectangle(
				-worldWidth / 2 - worldBorderThickness,
				-worldHeight / 2 - worldBorderThickness,
				worldBorderThickness,
				worldHeight + worldBorderThicknessDouble);
			spriteBatch.Draw(blank, r, worldBorderColor);

			r = new Rectangle(
				-worldWidth / 2 - worldBorderThickness, 
				-worldHeight / 2 - worldBorderThickness, 
				worldWidth + worldBorderThicknessDouble, 
				worldBorderThickness);
			spriteBatch.Draw(blank, r, worldBorderColor);

			r = new Rectangle(
				worldWidth / 2, 
				-worldHeight / 2 - worldBorderThickness, 
				worldBorderThickness, 
				worldHeight + worldBorderThicknessDouble);
			spriteBatch.Draw(blank, r, worldBorderColor);

			r = new Rectangle(
				-worldWidth / 2 - worldBorderThickness,
				worldHeight / 2, 
				worldWidth + worldBorderThicknessDouble,
				worldBorderThickness);
			spriteBatch.Draw(blank, r, worldBorderColor);
        }
        #endregion
    }
}
