//-----------------------------------------------------------------------------
// Bullet.cs
//
// Microsoft Advanced Technology Group
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TouchThumbsticks
{
	/// <summary>
	/// A single bullet in our game.
	/// </summary>
	public class Bullet
	{
		/// <summary>
		/// We use one texture for all bullets that is set from our game.
		/// </summary>
		public static Texture2D Texture;

		/// <summary>
		/// The rotation of the bullet
		/// </summary> 
		private readonly float rotation;

		/// <summary>
		/// The velocity of the bullet.
		/// </summary>
		private Vector2 velocity;

		/// <summary>
		/// The color of the bullet
		/// </summary>
		private Color color;

		/// <summary>
		/// The position of the bullet.
		/// </summary>
		public Vector2 Position;

		/// <summary>
		/// Creates a new Bullet at the given position with a given velocity.
		/// </summary>
		/// <param name="pos">The starting position of the bullet.</param>
		/// <param name="vel">The velocity of the bullet.</param>
		/// <param name="color">The color of the bullet.</param>
		public Bullet(Vector2 pos, Vector2 vel, Color col)
		{
			Position = pos;
			velocity = vel;
			color = col;

			// we use the Atan2 method to compute the rotation of the bullet
			// base on its velocity.
			rotation = (float)Math.Atan2(vel.Y, vel.X);
		}

		/// <summary>
		/// Moves the bullet along its velocity.
		/// </summary>
		public void Update()
		{
			Position += velocity;
		}

		/// <summary>
		/// Draws the bullet.
		/// </summary>
		/// <param name="spriteBatch">The SpriteBatch to use to draw the bullet.</param>
		public void Draw(SpriteBatch spriteBatch)
		{
			spriteBatch.Draw(
				Texture,
				Position,
				null, 
				color, 
				rotation,
				new Vector2(Texture.Width / 2f, Texture.Height / 2f), 
				1f, 
				SpriteEffects.None, 
				0f);
		}
	}
}