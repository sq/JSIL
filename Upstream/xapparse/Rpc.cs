using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Xap
{
	[Serializable]
	[DebuggerDisplay( "{m_name}" )]
	public class Rpc : Entity
	{
		public override bool SetProperty( string propertyName, string value, string[] source, ref int line )
		{
			switch ( propertyName )
			{
				case "Name":
					m_name = value;
					break;

				case "Comment":
					m_comment = Parser.ParseComment( value, source, ref line );
					break;

				case "Variable Entry":
					VariableEntry entry = new VariableEntry();
					entry.Parse( source, ref line, OwnerProject );
					m_variables.Add( entry.FindVariable() );
					break;

				case "RPC Curve":
					RpcCurve curve = new RpcCurve();
					curve.Parse( source, ref line, OwnerProject );
					m_rpcCurves.Add( curve );
					m_variables.Add( curve.m_variable.FindVariable() );
					break;

				case "Effect Entry":
					m_effectEntry = new EffectEntry();
					m_effectEntry.Parse( source, ref line, OwnerProject );
					// XXX what if you can have more than one? We'd need a List
					break;

				default:
					return false;
			}

			return true;
		}

		#region Member Variables

		public string m_name;
		public string m_comment;

		public List<Variable> m_variables = new List<Variable>();
		public List<RpcCurve> m_rpcCurves = new List<RpcCurve>();
		public EffectEntry m_effectEntry;

		#endregion
	}

	[Serializable]
	[DebuggerDisplay( "{m_name}" )]
	public class RpcEntry : Entity
	{
		public override bool SetProperty( string propertyName, string value, string[] source, ref int line )
		{
			switch ( propertyName )
			{
				case "RPC Name":
					m_name = value;
					break;

				default:
					return false;
			}

			return true;
		}

		public Rpc FindRpc()
		{
			foreach ( Rpc rpc in OwnerProject.m_globalSettings.m_rpcs )
			{
				if ( rpc.m_name == m_name )
				{
					return rpc;
				}
			}

			return null;
		}

		public string m_name;
	}

	[Serializable]
	[DebuggerDisplay( "{m_name}" )]
	public class RpcCurve : Entity
	{
		public override bool SetProperty( string propertyName, string value, string[] source, ref int line )
		{
			switch ( propertyName )
			{
				case "Name":
					m_name = value;
					break;

				case "Property":
					m_property = Parser.ParseInt( value, line );
					break;

				case "Sound":
					m_sound = Parser.ParseInt( value, line );
					break;

				case "Line Color":
					m_lineColour = Parser.ParseUint( value, line );
					break;

				case "Viewable":
					m_viewable = Parser.ParseInt( value, line );
					break;

				case "Effect Parameter Entry":
					EffectParameterEntry entry = new EffectParameterEntry();
					entry.Parse( source, ref line, OwnerProject );
					m_effectParameterEntries.Add( entry );
					break;

				case "RPC Point":
					RpcPoint point = new RpcPoint();
					point.Parse( source, ref line, OwnerProject );
					m_rpcPoints.Add( point );
					break;

				case "Variable Entry":	// Not in the docs!
					m_variable = new VariableEntry();
					m_variable.Parse( source, ref line, OwnerProject );
					// XXX can you have more than one? If so we need a List
					break;

				default:
					return false;
			}

			return true;
		}

		#region Member Variables

		public string m_name;
		public int m_property;
		public int m_sound;
		internal uint m_lineColour;
		public int m_viewable;

		public VariableEntry m_variable;

		public List<EffectParameterEntry> m_effectParameterEntries = new List<EffectParameterEntry>();
		public List<RpcPoint> m_rpcPoints = new List<RpcPoint>();

		#endregion
	}

	[Serializable]
	[DebuggerDisplay( "{m_rpcName}" )]
	public class RpcCurveEntry : Entity
	{
		public override bool SetProperty( string propertyName, string value, string[] source, ref int line )
		{
			switch ( propertyName )
			{
				case "RPC Name":
					m_rpcName = value;
					break;

				case "RPC Index":
					m_rpcIndex = Parser.ParseInt( value, line );
					break;

				case "RPC Curve Name":
					m_curveName = value;
					break;

				case "RPC Curve Index":
					m_curveIndex = Parser.ParseInt( value, line );
					break;

				default:
					return false;
			}

			return true;
		}

		public RpcCurve FindRpcCurve()
		{
			foreach ( Rpc rpc in OwnerProject.m_globalSettings.m_rpcs )
			{
				if ( rpc.m_name == m_rpcName )
				{
					foreach ( RpcCurve curve in rpc.m_rpcCurves )
					{
						if ( curve.m_name == m_curveName )
						{
							return curve;
						}
					}
				}
			}

			// Not found by name, which suggests index is likely to fail too (so why
			// the XAP file format insists on providing both, I don't understand)
			if ( m_rpcIndex >= 0 && m_curveIndex >= 0 )
			{
				return OwnerProject.m_globalSettings.m_rpcs[m_rpcIndex].m_rpcCurves[m_curveIndex];
			}

			return null;
		}

		#region Member variables

		public string m_rpcName;
		public int m_rpcIndex;
		public string m_curveName;
		public int m_curveIndex;

		#endregion
	}

	[Serializable]
	public class RpcPoint : Entity
	{
		public override bool SetProperty( string propertyName, string value, string[] source, ref int line )
		{
			switch ( propertyName )
			{
				case "X":
					m_x = Parser.ParseFloat( value, line );
					break;

				case "Y":
					m_y = Parser.ParseFloat( value, line );
					break;

				case "Curve":
					m_curve = Parser.ParseInt( value, line );
					break;

				default:
					return false;
			}

			return true;
		}

		#region Member variables

		public float m_x;
		public float m_y;
		public int m_curve;		// For enums see docs

		#endregion
	}
}
