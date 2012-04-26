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

function updateSplitter (x) {
  var leftColumn = document.getElementById("left_column");
  var rightColumn = document.getElementById("right_column");
  var splitter = document.getElementById("splitter");
  var totalWidth = document.getElementById("columns").offsetWidth - (leftColumn.offsetLeft * 2);
  var xDelta = x - splitterDragStart[0];

  var minWidth = 200;
  var maxWidth = totalWidth - 200;

  var splitterWidth = 14;
  var newLeftWidth = (leftColumnWidthStart + xDelta);
  if (newLeftWidth < minWidth)
    newLeftWidth = minWidth;
  else if (newLeftWidth > maxWidth)
    newLeftWidth = maxWidth;

  leftColumn.style.width = newLeftWidth + "px";
  rightColumn.style.width = (totalWidth - newLeftWidth - splitterWidth) + "px";
  splitter.style.left = (newLeftWidth + leftColumn.offsetLeft + 4) + "px";
}

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

var isDraggingSplitter = false;
var splitterDragStart = [0, 0];
var leftColumnWidthStart = 0;

function onLoad () {
  $("#throbber").hide();

  document.getElementById("javascript").value = "";

  $("#compile").click(beginCompile);

  function initSplitter (x, y) {
    splitterDragStart = [x, y];
    leftColumnWidthStart = document.getElementById("left_column").offsetWidth;
  };

  var body = document.getElementsByTagName("body")[0];

  $("#splitter").mousedown(function (evt) {
    isDraggingSplitter = true;
    initSplitter(evt.clientX, evt.clientY);
    evt.preventDefault();
    evt.stopPropagation();
  });

  body.addEventListener("mousemove", function (evt) {
    if (isDraggingSplitter) {
      updateSplitter(evt.clientX);
      evt.preventDefault();
      evt.stopPropagation();
    }
  }, true);

  body.addEventListener("mouseup", function (evt) {
    if (isDraggingSplitter) {
      updateSplitter(evt.clientX);
      isDraggingSplitter = false;
      evt.preventDefault();
      evt.stopPropagation();
    }
  }, true);

  initSplitter(0, 0);
  updateSplitter(0);
};