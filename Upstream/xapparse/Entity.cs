using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Xap
{
	/// <summary>
	/// The base class for all entities stored within an XACT project file.
	/// </summary>
	[Serializable]
	public abstract class Entity
	{
		#region Input

		/// <summary>
		/// Interpret the property and value pair as given, including parsing sub-structures. Throw an exception
		/// if an error is encountered, or return false to generate an exception with default formatting.
		/// </summary>
		/// <param name="property">The property to interpret.</param>
		/// <param name="value">The value to assign.</param>
		/// <param name="source">The source file (required for parsing sub-structures).</param>
		/// <param name="line">The current line, which will change if a sub-structure is parsed.</param>
		/// <returns>true for success, or false to fail and throw.</returns>
		public abstract bool SetProperty( string propertyName, string value, string[] source, ref int line );

		/// <summary>
		/// Parse this structure from the XAP file source.
		/// </summary>
		/// <param name="source">The source data.</param>
		/// <param name="line">The current line, which will change during the Parser.</param>
		public void Parse( string[] source, ref int line, Project project )
		{
			m_project = project;
			string propertyName;
			string value;

			// Will currently be looking at the structure header, increment to get into structure delimiting brackets
			++line;

			if ( !Parser.TokeniseLine( source[line], out propertyName, out value ) || propertyName != "{" )
			{
				throw new InvalidContentException( "Structure on line " + line.ToString() + " does not open with a curly brace." );
			}

			// Increment again to get into the structure proper
			++line;

			while ( line < source.Length )
			{
				if ( Parser.TokeniseLine( source[line], out propertyName, out value ) )
				{
					switch ( propertyName )
					{
						case "}":
							return;

						default:
							if ( !SetProperty( propertyName, value, source, ref line ) )
							{
								throw new InvalidContentException( "Type " + this.GetType().ToString() + " did not expect property '"
									+ propertyName + "', value '" + value + "', on line " + line.ToString() );
							}
							break;
					}
				}

				++line;
			}
		}

		#endregion

		#region Member variables

		Project m_project;

		public Project OwnerProject
		{
			get { return m_project; }
			set { m_project = value; }
		}

		#endregion
	}
}
