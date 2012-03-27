using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Xap
{
	[Serializable]
	public class InstanceLimit : Entity
	{
		public override bool SetProperty( string propertyName, string value, string[] source, ref int line )
		{
			switch ( propertyName )
			{
				case "Max Instances":
					m_maxInstances = Parser.ParseInt( value, line );
					break;

				case "Transition Type":
					m_transitionType = Parser.ParseInt( value, line );
					break;

				case "Behavior":
					m_behaviour = Parser.ParseInt( value, line );
					break;

				case "Crossfade":
					m_crossfade = new Crossfade();
					m_crossfade.Parse( source, ref line, OwnerProject );
					break;

				default:
					return false;
			}

			return true;
		}

		#region Member variables

		public int m_maxInstances;
		public int m_transitionType;	// See docs for enum
		public int m_behaviour;			// See docs for enum

		Crossfade m_crossfade;

		#endregion
	}

	[Serializable]
	public class Crossfade : Entity
	{
		public override bool SetProperty( string propertyName, string value, string[] source, ref int line )
		{
			switch ( propertyName )
			{
				case "Fade In":
					m_fadeIn = Parser.ParseInt( value, line );
					break;

				case "Fade Out":
					m_fadeOut = Parser.ParseInt( value, line );
					break;

				case "Crossfade Type":
					m_type = Parser.ParseInt( value, line );
					break;

				default:
					return false;
			}

			return true;
		}

		#region Member variables

		public int m_fadeIn;
		public int m_fadeOut;
		public int m_type;				// See docs for enum

		#endregion
	}
}
