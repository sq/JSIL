![JSIL logo](http://jsil.org/images/jsil_48px.png) JSIL
====

JSIL is a compiler that transforms .NET applications and libraries from their native executable format - CIL bytecode - into standards-compliant, cross-browser JavaScript. You can take this JavaScript and run it in a web browser or any other modern JavaScript runtime. Unlike other cross-compiler tools targeting JavaScript, JSIL produces readable, easy-to-debug JavaScript that resembles the code a developer might write by hand, while still maintaining the behavior and structure of the original .NET code.

For live demos and code samples, [visit the website](http://jsil.org).

For help on getting started using JSILc, see [the wiki](https://github.com/kevingadd/JSIL/wiki).

License
=======

Copyright 2011 Kevin Gadd  
License: MIT/X11

Acknowledgements
========

JSIL depends upon or is based on the following open source libraries:

 * Mono.Cecil: MIT/X11 (thanks to Jb Evain)
 * ICSharpCode.Decompiler: MIT/X11 (developed as part of ILSpy)
 * Mono.Options: MIT/X11 (Jonathan Pryor & Federico Di Gregorio)
 * printStackTrace: Public Domain (Eric Wendelin and others)
 * XAPParse: Microsoft Public License/Ms-PL (Andy Patrick)
 * webgl-2d: MIT (Corban Brook, Bobby Richter, Charles J. Cliffe, and others)
 * S3TC DXT1 / DXT5 Texture Decompression Routines (Benjamin Dobell)
 
The Upstream folder also contains:

 * Win32 build of the Spidermonkey command-line JavaScript shell. It is built from sources provided by the Mozilla project (http://www.mozilla.org/). This build is used for running JavaScript automated tests.
 * A specific version of the NUnit.Framework assembly, used by the automated tests. This ensures that they compile correctly regardless of which version of NUnit you have installed.
 * Win32 build of PNGQuant for optimizing PNG files. (Jef Poskanzer, Greg Roelofs)
 
Logo by [John Flynn](http://www.bryneshrimp.com).
 
Assorted code and test case contributions by the various contributors on the GitHub project page - already too many to list here.