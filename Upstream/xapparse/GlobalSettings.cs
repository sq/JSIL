using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Xap
{
	[Serializable]
	[DebuggerDisplay( "{m_categories.Count} categories, {m_variables.Count} variables, {m_rpcs.Count} RPCs..." )]
	public class GlobalSettings : Entity
	{
		public override bool SetProperty( string propertyName, string value, string[] source, ref int line )
		{
			switch ( propertyName )
			{
				case "Xbox File":
					m_xboxFile = value;
					break;

				case "Windows File":
					m_windowsFile = value;
					break;

				case "Header File":
					m_headerFile = value;
					break;

				case "Shared Settings File":
					m_sharedSettingsFile = value;
					break;

				case "Exclude Category Names":
					m_excludeCategoryNames = Parser.ParseInt( value, line );
					break;

				case "Exclude Variable Names":
					m_excludeVariableNames = Parser.ParseInt( value, line );
					break;

				case "Last Modified Low":
					m_lastModifiedLow = Parser.ParseUint( value, line );
					break;

				case "Last Modified High":
					m_lastModifiedHigh = Parser.ParseUint( value, line );
					break;

				case "Category":
					Category category = new Category();
					category.Parse( source, ref line, OwnerProject );
					m_categories.Add( category );
					break;

				case "Variable":
					Variable variable = new Variable();
					variable.Parse( source, ref line, OwnerProject );
					m_variables.Add( variable );
					break;

				case "Effect":
					Effect effect = new Effect();
					effect.Parse( source, ref line, OwnerProject );
					m_effects.Add( effect );
					break;

				case "Codec Preset":		// So says the docs
				case "Compression Preset":	// So say XACT files!
					CompressionPreset preset = new CompressionPreset();
					preset.Parse( source, ref line, OwnerProject );
					m_compressionPresets.Add( preset );
					break;

				case "RPC":
					Rpc rpc = new Rpc();
					rpc.Parse( source, ref line, OwnerProject );
					m_rpcs.Add( rpc );
					break;

				default:
					return false;
			}

			return true;
		}		

		#region Member variables

		public string m_xboxFile;
		public string m_windowsFile;
		public string m_headerFile;
		public string m_sharedSettingsFile;
		public int m_excludeCategoryNames;
		public int m_excludeVariableNames;
		internal uint m_lastModifiedLow;
		internal uint m_lastModifiedHigh;

		public List<Category> m_categories = new List<Category>();
		public List<Variable> m_variables = new List<Variable>();
		public List<Rpc> m_rpcs = new List<Rpc>();
		public List<Effect> m_effects = new List<Effect>();
		public List<CompressionPreset> m_compressionPresets = new List<CompressionPreset>();

		#endregion

		#region Properties

		public string XboxFile
		{
			get { return m_xboxFile; }
			set { m_xboxFile = value; }
		}

		public string WindowsFile
		{
			get { return m_windowsFile; }
			set { m_windowsFile = value; }
		}

		#endregion
	}

	[Serializable]
	[DebuggerDisplay( "Verbose: {m_verboseReport}, C Headers: {m_generateCHeaders}" )]
	public class Options : Entity
	{
		#region Input

		public override bool SetProperty( string propertyName, string value, string[] source, ref int line )
		{
			switch ( propertyName )
			{
				case "Verbose Report":
					m_verboseReport = Parser.ParseInt( value, line );
					break;

				case "Generate C/C++ Headers":
					m_generateCHeaders = Parser.ParseInt( value, line );
					break;

				default:
					return false;
			}

			return true;
		}

		#endregion

		#region Member variables

		public int m_verboseReport;
		public int m_generateCHeaders;

		#endregion
	}
}
