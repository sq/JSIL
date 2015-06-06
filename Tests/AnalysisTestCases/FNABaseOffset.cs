using System;
using System.Collections.Generic;

public static class Program
{
    public class SpriteFont {
        public float Spacing = 2;
        public float LineSpacing = 8;
        public char? DefaultCharacter = '?';

        public List<char> characterMap = new List<char>();
        public List<Vector3> kerning = new List<Vector3>();
        public List<Vector2> croppingData = new List<Vector2>();

        public SpriteFont () {
            for (var i = 32; i <= 127; i++) {
                characterMap.Add((char)i);
                kerning.Add(new Vector3(
                    i * 0.75f,
                    i * 1f,
                    i * 1.25f
                ));
                croppingData.Add(new Vector2(
                    i * 16,
                    i * 12
                ));
            }
        }
    }

    public struct Vector2 {
        public static Vector2 Zero = new Vector2(0, 0);

        public float X, Y;

        public Vector2 (float x, float y) {
            X = x;
            Y = y;
        }

        public static Vector2 operator - (Vector2 lhs, Vector2 rhs) {
            return new Vector2(lhs.X - rhs.X, lhs.Y - rhs.Y);
        }

        public override string ToString () {
            return String.Format("{0}, {1}", X, Y);
        }
    }

    public struct Vector3 {
        public float X, Y, Z;

        public Vector3 (float x, float y, float z) {
            X = x;
            Y = y;
            Z = z;
        }
    }

    public struct Color {
    }

    public enum SpriteEffects {
        None
    }

    public static void Main(string[] args) {
        DrawString(
            new SpriteFont(),
            "Te st\nab cd",
            new Vector2(4, 4),
            new Color(),
            0.0f,
            new Vector2(8, 8),
            new Vector2(1, 1),
            SpriteEffects.None,
            0.0f
        );

        for (var i = 0; i < numSprites; i++)
            Console.WriteLine(spriteData[i].origin);
    }

    public static void DrawString (
		SpriteFont spriteFont,
		string text,
		Vector2 position,
		Color color,
		float rotation,
		Vector2 origin,
		Vector2 scale,
		SpriteEffects effects,
		float layerDepth
	) {
		// FIXME: This needs an accuracy check! -flibit
        // dead

		// Calculate offset, using the string size for flipped text
		Vector2 baseOffset = origin;
		if (effects != SpriteEffects.None)
		{
            baseOffset -= new Vector2(32, 64);
		}

		Vector2 curOffset = Vector2.Zero;
		bool firstInLine = true;
		foreach (char c in text)
		{
			// Special characters
			if (c == '\r')
			{
				continue;
			}
			if (c == '\n')
			{
				curOffset.X = 0.0f;
				curOffset.Y += spriteFont.LineSpacing;
				firstInLine = true;
				continue;
			}

			/* Get the List index from the character map, defaulting to the
				* DefaultCharacter if it's set.
				*/
			int index = spriteFont.characterMap.IndexOf(c);
			if (index == -1)
			{
				if (!spriteFont.DefaultCharacter.HasValue)
				{
					throw new ArgumentException(
						"Text contains characters that cannot be" +
						" resolved by this SpriteFont.",
						"text"
					);
				}
				index = spriteFont.characterMap.IndexOf(
					spriteFont.DefaultCharacter.Value
				);
			}

			/* For the first character in a line, always push the width
				* rightward, even if the kerning pushes the character to the
				* left.
				*/
			if (firstInLine)
			{
				curOffset.X += Math.Abs(spriteFont.kerning[index].X);
				firstInLine = false;
			}
			else
			{
				curOffset.X += spriteFont.Spacing + spriteFont.kerning[index].X;
			}

			// Calculate the character origin
			Vector2 offset = baseOffset;
                                                                           // dead
			offset.X += (curOffset.X + spriteFont.croppingData[index].X) * 1;
			offset.Y += (curOffset.Y + spriteFont.croppingData[index].Y) * 1;
            // dead

			// Draw!
            PushSprite(offset, layerDepth, (byte)effects);

			/* Add the character width and right-side bearing to the line
				* width.
				*/
			curOffset.X += spriteFont.kerning[index].Y + spriteFont.kerning[index].Z;
		}
	}

    struct SpriteData {
        public Vector2 origin;
        public float depth;
        public byte effects;
    }

    static SpriteData[] spriteData = new SpriteData[16];
    static int numSprites = 0;

	private static void PushSprite(
		Vector2 origin,
		float depth,
		byte effects
	) {
        // dead

		// Everything else passed is just copied
		spriteData[numSprites].origin = origin;
		spriteData[numSprites].depth = depth;
		spriteData[numSprites].effects = effects;

        // dead

        numSprites += 1;
	}
}
