//-----------------------------------------------------------------------------
// EnemyShip.cs
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
	/// An enemy ship.
	/// </summary>
	public class EnemyShip : Ship
	{
		/// <summary>
		/// A reference to the player so the enemy can fly towards the player
		/// </summary>
		public Ship Player;

		/// <summary>
		/// The radius of the ship; used for collision detection.
		/// </summary>
		private float radius;

		public EnemyShip(Texture2D texture)
			: base(texture)
		{
			// calculate the radius of the ship from the texture
			radius = (float)Math.Sqrt(texture.Width * texture.Width + texture.Height * texture.Height) * .75f;
		}

		public override void Update(GameTime gameTime)
		{
			// figure out the direction from the enemy to the player
			Vector2 d = Vector2.Normalize(Player.Position - Position);

			// set our rotation based on that vector
			Rotation = (float)Math.Atan2(d.Y, d.X);

			// move towards the player at constant speed
			Position += d * 4f;
		}

		public override bool ContainsPoint(Vector2 point)
		{
			// determine if the distance from the enemy to the point
			// is within the enemy's radius
			return Vector2.Distance(Position, point) < radius;
		}
	}
}