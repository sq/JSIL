using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Xap
{
	internal static class Parser
	{
		/// <summary>
		/// Converts for example "Name = it_begin;" into "Name" and "ir_begin".
		/// In the case of a line with no value, value is returned blank.
		/// In the case of an empty line, property and value are both blank and false is returned.
		/// </summary>
		/// <param name="line">This line.</param>
		/// <param name="property">The property of this line.</param>
		/// <param name="value">The value of the property on this line.</param>
		/// <returns>true if there was anything on the line.</returns>
		internal static bool TokeniseLine( string line, out string propertyName, out string value )
		{
			if ( !string.IsNullOrEmpty( line ) )
			{
#if false		// I tried regex and failed. Am I a bad person?
				Match m = Regex.Match( line, "^[:blank:]*<prop>[:print:]*( = <val>[:print:]);\n$", RegexOptions.CultureInvariant );

				if ( m.Groups["prop"].Success )
				{
					property = m.Groups["prop"].Value.Trim();

					if ( m.Groups["val"].Success )
					{
						value = m.Groups["val"].Value.Trim();
					}
					else
					{
						value = "";
					}

					return true;
				}
#else
				int semiColon = line.LastIndexOf( ';' );

				if ( semiColon >= 0 )
				{
					line = line.Remove( semiColon );
				}

				int equals = line.IndexOf( '=' );

				if ( equals > 0 )
				{
					propertyName = line.Substring( 0, equals ).Trim();
					value = line.Substring( equals + 1 ).Trim();
				}
				else
				{
					propertyName = line.Trim();
					value = "";
				}

				return true;
#endif
			}

			propertyName = "";
			value = "";
			return false;
		}

		internal static int ParseInt( string value, int line )
		{
			int outValue;

			if ( !int.TryParse( value, out outValue ) )
			{
				throw new InvalidContentException( "Could not parse '" + value + "' as int on line " + line.ToString() );
			}

			return outValue;
		}

		internal static uint ParseUint( string value, int line )
		{
			uint outValue;

			if ( !uint.TryParse( value, out outValue ) )
			{
				throw new InvalidContentException( "Could not parse '" + value + "' as uint on line " + line.ToString() );
			}

			return outValue;
		}

		internal static float ParseFloat( string value, int line )
		{
			float outValue;

			if ( !float.TryParse( value, out outValue ) )
			{
				throw new InvalidContentException( "Could not parse '" + value + "' as float on line " + line.ToString() );
			}

			return outValue;
		}

		internal static float ParseIntOrFloat( string value, int line )
		{
			float outValue;

			if ( !float.TryParse( value, out outValue ) )
			{
				int outInt;

				if ( !int.TryParse( value, out outInt ) )
				{
					throw new InvalidContentException( "Could not parse '" + value + "' as either an int or a float on line " + line.ToString() );
				}

				outValue = (float)outInt;
			}

			return outValue;
		}

		// Comments/Notes can be multiline
		internal static string ParseComment( string firstValue, string[] source, ref int line )
		{
			StringBuilder comment = new StringBuilder( firstValue );
			int semiColon = source[line].LastIndexOf( ';' );

			if ( semiColon >= 0 )
			{
				// Simple case, only one line in this comment
				return comment.ToString();
			}
			else
			{
				comment.Append( "\n" );
				++line;

				while ( line < source.Length )
				{
					semiColon = source[line].LastIndexOf( ';' );

					if ( semiColon >= 0 )
					{
						comment.Append( source[line].Remove( semiColon ) );
						return comment.ToString();
					}

					comment.Append( source[line] );
					comment.Append( "\n" );
					++line;
				}
			}

			return comment.ToString();
		}

	}
}
