using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle( "Xap" )]
[assembly: AssemblyProduct( "XapParse" )]
[assembly: AssemblyDescription( "Functionality to parse XAP (XACT Audio Project) files" )]
[assembly: AssemblyCompany( "Kensei" )]
[assembly: AssemblyCopyright( "Copyright © Andy Patrick 2008" )]
[assembly: AssemblyTrademark( "" )]
[assembly: AssemblyCulture( "" )]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible( false )]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid( "5e50ed5a-1df1-4535-84bb-76557f631b2d" )]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
[assembly: AssemblyVersion( "1.0.1.0" )]

// CLS compliant, see EC# and FxCop
[assembly: CLSCompliant( true )]

// ---------------------------------------------------------
// XapParse - tool for parsing XAP format XACT Project Files
// ---------------------------------------------------------
// Reads XAP files into memory, for querying and manipulation. A game might use the data contained within the XAP file
// to extend behaviour beyond what is expected - for example,  programmatically generating lists of cues that can be
// made available to the game at runtime, rather than relying on hard-coded enums that inevitably go out of date.
//
// Current Features:
//
//	- Reads any valid XAP file into memory, into publically accessible data structures
//		- Note: even supports many features not appearing (or appearing incorrectly) in the documentation
//		- Note: of course, because of the poor documentation, it's hard to *guarantee* all valid files can be handled...
//	- Fully serializable
//	- CLS compliant
//
// To Do in Future Work:
//
//	- better error handling (at the moment it simply throws an unhandled exception)
//	- write out to a file (to later support programmatically making changes)
//	- validate the written out project by ensuring it is identical to the input project (to ensure the syntax is correct)
//	- allow manipulation of entities within the project
//		- eg. moving waves between wavebanks, fixing up all references to those waves
//			- note: the fix up will be slow and complicated due to the way XACT files use "entries"
//		- eg. being able to set a value for all entities of a specific type
//		- eg. automatically determining which waves should be in which wavebanks based on their tracks/sounds/cues/use in game
//	- make members private, use accessors instead (note: lots of work for minimal gain)
//	- keep up to date with new versions of the XNA Framework
//
// Disclaimer: you may do whatever you wish with these files. Anything. Commercial projects? Go for it. Hobby projects?
// Even better. Change the files? Sure, but if you find and fix any bugs, let me know. I hereby declare you have free
// rein to do with these files exactly as you see fit. It'd be pretty rude if you started pretending you created them,
// though, but you wouldn't be so impolite, right? And it'd be nice to give me a credit in your game if you find this
// code useful. But if you don't, what am I doing to do, hunt you down? Well... no. Do as you wish, with my blessing.
// Oh - but don't try blaming me if your computer breaks or something as I take absolutely no responsibility for that.
// So use these files with my blessing, but at your own discretion.
//
// Author: Andy Patrick
// Special Thanks: Scott Selfon, Glenn Doren, Ian Lewis, Chris Pigas
// Inspired by - but most definitely not based upon (in case any lawyers are watching) - Xapper