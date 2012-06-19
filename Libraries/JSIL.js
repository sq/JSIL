//
// JSIL loader. Synchronously loads all core JSIL scripts, adds essential libraries to the content manifest,
//  and loads your manifest scripts.
// Load this at the top of your document (optionally after declaring a jsilConfig dict in a previous script tag).
// Asset loading (after page load) is provided by JSIL.Browser.js.
//

if (typeof (contentManifest) !== "object") { 
	contentManifest = {}; 
};
contentManifest["JSIL"] = [
    ["Library", "JSIL.Storage.js"],
    ["Library", "JSIL.IO.js"],
    ["Library", "JSIL.JSON.js"],	
    ["Library", "JSIL.XML.js"]
];

(function loadJSIL (config) {

	if (config.showFullscreenButton) {
		document.write(
			'<button id="fullscreenButton">Full Screen</button>'
		);
	}

	if (config.showStats) {
		document.write(
			'<div id="stats"></div>'
		);
	}

	if (config.showProgressBar) {
		document.write(
			'<div id="loadingProgress">' +
      		'  <div id="progressBar"></div>' +
			'  <span id="progressText"></span>' +
	    	'</div>'
    	);

	}	
	
	var libraryRoot = config.libraryRoot || "../Libraries/";
	var manifestRoot = config.manifestRoot || "";

	function loadScript (uri) {
		if (window.console && window.console.log)
			window.console.log("Loading '" + uri + "'...");

		document.write(
			"<script type=\"text/javascript\" src=\"" + uri + "\"></script>"
		);
	};

	if (config.printStackTrace)
		loadScript(libraryRoot + "printStackTrace.js");

	if (config.webgl2d)
		loadScript(libraryRoot + "webgl-2d.js");

	loadScript(libraryRoot + "JSIL.Core.js");
	loadScript(libraryRoot + "JSIL.Bootstrap.js");
	loadScript(libraryRoot + "JSIL.Browser.js");

	var manifests = config.manifests || [];

	for (var i = 0, l = manifests.length; i < l; i++)
		loadScript(manifestRoot + manifests[i] + ".manifest.js");

	if (config.winForms) {
		contentManifest["JSIL"].push(
			["Library", "System.Drawing.js"]
		);
		contentManifest["JSIL"].push(
			["Library", "System.Windows.js"]
		);
	}

	if (config.xna) {
		contentManifest["JSIL"].push(
			["Library", "JSIL.XNACore.js"]
		);

		switch (Number(config.xna)) {
			case 3:
				contentManifest["JSIL"].push(
					["Library", "JSIL.XNA3.js"]
				);
				break;
			case 4:
				contentManifest["JSIL"].push(
					["Library", "JSIL.XNA4.js"]
				);
				break;
			default:
				throw new Error("Unsupported XNA version");
		}

		contentManifest["JSIL"].push(
			["Library", "JSIL.XNAStorage.js"]
		);
	}

	if (config.localStorage) {
		contentManifest["JSIL"].push(
	        ["Library", "JSIL.LocalStorage.js"]
        );
	}

})(jsilConfig || {});