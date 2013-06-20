//-----------------------------------------------------------------------------
// Ship.cs
//
// Microsoft Advanced Technology Group
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace TouchThumbsticks
{
	/// <summary>
	/// A base class for all ships in our game.
	/// </summary>
	public abstract class Ship
	{
		/// <summary>
		/// The texture used to draw the ship.
		/// </summary>
		private Texture2D texture;

		/// <summary>
		/// The position of the ship in the game world.
		/// </summary>
		public Vector2 Position;

		/// <summary>
		/// The ship's velocity.
		/// </summary>
		public Vector2 Velocity;

		/// <summary>
		/// The ship's rotation.
		/// </summary>
		public float Rotation;

		public Ship(Texture2D texture)
		{
			this.texture = texture;
		}

		/// <summary>
		/// Does the ship contain the current point? Used for collision detection
		/// with the bullets.
		/// </summary>
		/// <param name="point">The point to test.</param>
		/// <returns>True if the ship contains the point, false otherwise.</returns>
		public virtual bool ContainsPoint(Vector2 point) { return false; }

		/// <summary>
		/// Allows the ship to update.
		/// </summary>
		/// <param name="gameTime">The current game timestamp.</param>
		public virtual void Update(GameTime gameTime) { }

		/// <summary>
		/// Allows the ship to draw.
		/// </summary>
		/// <param name="spriteBatch">The SpriteBatch to draw with.</param>
		public virtual void Draw(SpriteBatch spriteBatch)
		{
			spriteBatch.Draw(
				texture,
				Position,
				null,
				Color.White,
				Rotation,
				new Vector2(texture.Width / 2f, texture.Height / 2f),
				1f,
				SpriteEffects.None,
				0f);
		}
	}
}