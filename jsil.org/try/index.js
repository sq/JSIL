function beginCompile () {
  $("#throbber").fadeIn();

  var btn = $("#compile");
  btn.attr("disabled", "disabled");
  btn.fadeOut();

  clearOutputWindow();

  var sourceCode = document.getElementById("sourcecode").value;

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
    runInOutputWindow(data.javascript, data.entryPoint, data.warnings);
  } else {
    alert("Compile failed: " + String(data.error || status));
  }
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