//-----------------------------------------------------------------------------
// PlayerShip.cs
//
// Microsoft Advanced Technology Group
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace TouchThumbsticks
{
	/// <summary>
	/// The ship controlled by the player.
	/// </summary>
	public class PlayerShip : Ship
	{
		// the acceleration of the ship
		private const float acceleration = .75f;

		// the speed of the bullets
		private const float bulletSpeed = 20f;

		/// <summary>
		/// Cooldown in between bullets firing.
		/// </summary>
		private static readonly TimeSpan cooldown = TimeSpan.FromSeconds(.15f);

		/// <summary>
		/// The timer we use to count down until we can fire
		/// another bullet.
		/// </summary>
		private TimeSpan fireTimer;

		/// <summary>
		/// A list of all the bullets the player has fired that are active.
		/// </summary>
		public List<Bullet> Bullets = new List<Bullet>();

		/// <summary>
		/// The width of the world; used to keep the player in-bounds.
		/// </summary>
		public int WorldWidth;

		/// <summary>
		/// The height of the world; used to keep the player in-bounds.
		/// </summary>
		public int WorldHeight;

		public PlayerShip(Texture2D texture)
			: base(texture)
		{
		}

		public override void Update(GameTime gameTime)
		{
			// adjust our velocity base on our virtual thumbstick
			Velocity += VirtualThumbsticks.LeftThumbstick * acceleration;

			// add our velocity to our position to move the ship
			Position += Velocity;

			// decrease the velocity a little bit for some drag
			Velocity *= .98f;

			// decrease our fire timer
			fireTimer -= gameTime.ElapsedGameTime;

			// if the user is moving the right thumbstick a bit
			if (VirtualThumbsticks.RightThumbstick.Length() > .3f)
			{
				// update our ship's rotation based on the right thumbstick
				Rotation = -(float)Math.Atan2(
					-VirtualThumbsticks.RightThumbstick.Y, 
					VirtualThumbsticks.RightThumbstick.X);

				// if our fire timer has reached zero
				if (fireTimer <= TimeSpan.Zero)
				{
					// add a new bullet to our list
					Vector2 bulletVelocity = Vector2.Normalize(VirtualThumbsticks.RightThumbstick) * bulletSpeed;
					Bullets.Add(new Bullet(Position, bulletVelocity, Color.Red));

					// reset our timer
					fireTimer = cooldown;
				}
			}

			// if the user isn't moving the right thumbstick enough, update our
			// rotation based on the left thumstick
			else if (VirtualThumbsticks.LeftThumbstick.Length() > .2f)
			{
				Rotation = -(float)Math.Atan2(
					-VirtualThumbsticks.LeftThumbstick.Y, 
					VirtualThumbsticks.LeftThumbstick.X);
			}

			// update all the bullets
			foreach (var b in Bullets)
				b.Update();

			// remove any bullets that are outside of the world
			for (int i = Bullets.Count - 1; i >= 0; i--)
			{
				Bullet b = Bullets[i];
				if (b.Position.X < -WorldWidth / 2f || 
					b.Position.X > WorldWidth / 2f || 
					b.Position.Y < -WorldHeight / 2f || 
					b.Position.Y > WorldHeight / 2f)
					Bullets.RemoveAt(i);
			}

			// clamp the player ship inside the world bounds
			ClampPlayerShip();
		}

		/// <summary>
		/// Clamps the player ship inside of the world bounds.
		/// </summary>
		private void ClampPlayerShip()
		{
			if (Position.X < -WorldWidth / 2f)
			{
				Position.X = -WorldWidth / 2f;
				if (Velocity.X < 0f)
					Velocity.X = 0f;
			}

			if (Position.X > WorldWidth / 2f)
			{
				Position.X = WorldWidth / 2f;
				if (Velocity.X > 0f)
					Velocity.X = 0f;
			}

			if (Position.Y < -WorldHeight / 2f)
			{
				Position.Y = -WorldHeight / 2f;
				if (Velocity.Y < 0f)
					Velocity.Y = 0f;
			}

			if (Position.Y > WorldHeight / 2f)
			{
				Position.Y = WorldHeight / 2f;
				if (Velocity.Y > 0f)
					Velocity.Y = 0f;
			}
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			// draw all the bullets first
			foreach (var b in Bullets)
				b.Draw(spriteBatch);

			// call base.Draw to draw the ship
			base.Draw(spriteBatch);
		}
	}
}