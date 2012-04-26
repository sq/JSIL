function beginCompile () {
  $("#throbber").fadeIn();

  var btn = $("#compile");
  btn.attr("disabled", "disabled");
  btn.fadeOut();

  clearOutputWindow();

  var sourceCode = document.getElementById("sourcecode").value;
  setStatus("Compiling...");

  $.ajax({
    type: 'POST',
    url: "http://jsil.org/try/compile.aspx",
    data: sourceCode,
    success: compileComplete,
    error: function (xhr, status, moreStatus) {
      compileComplete(false, status + ": " + moreStatus);
    },
    dataType: "json"
  });
};

function compileComplete (data, status) {
  $("#throbber").fadeOut();

  var btn = $("#compile");
  btn.removeAttr("disabled");
  btn.fadeIn();

  if (data && data.ok) {
    document.getElementById("javascript").value = data.javascript;
    setStatus(
      "Compile successful.<br>" +
      "C# compile took " + data.compileElapsed + " second(s).<br>" +
      "Translation took " + data.translateElapsed + " second(s)."
    );
    runInOutputWindow(data.javascript, data.entryPoint, data.warnings);
  } else {
    var errorText = String(data.error || status);
    errorText = (
      errorText.replace(/\&/g, "&amp;")
        .replace(/\</g, "&lt;")
        .replace(/\>/g, "&gt;")
        .replace(/\n/g, "<br>")
    );
    setStatus("Compile failed.<br>" + errorText);
  }
};

function setStatus (text) {
  var s = document.getElementById("status");
  s.innerHTML = text;
};

function runInOutputWindow (javascript, entryPoint, warnings) {
  var outputWindow = document.getElementById("iframe").contentWindow;
  var outputDoc = document.getElementById("iframe").contentDocument;

  setOutputThrobberVisible(outputDoc, true);
  try {
    if (warnings && warnings.length > 0) {
      outputWindow.JSIL.Host.logWriteLine("// Begin Warnings //");
      outputWindow.JSIL.Host.logWriteLine(warnings);
      outputWindow.JSIL.Host.logWriteLine("// End Warnings //");
    }

    outputWindow.runScript(javascript, entryPoint);
  } finally {
    setOutputThrobberVisible(outputDoc, false);
  }
};

function setOutputThrobberVisible (doc, isVisible) {
  doc.getElementById("throbber").style.display = isVisible ? "block" : "none";
};

function clearOutputWindow () {
  document.getElementById("iframe").contentWindow.location.reload();
};

function onLoad () {
  $("#throbber").hide();

  $("#compile").click(beginCompile);
};