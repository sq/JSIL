function loadExamples () {
  var username = "TryJSIL";
  
  $.ajax({
    url: 'https://api.github.com/users/' + username + '/gists',
    dataType: 'jsonp',
    success: function (result) {
      displayExamples(result);
    }
  });
};

function displayExamples (result) {
  var container = $("#examples_list");
  container.empty();

  var entries = result.data;

  var makeClickHandler = function (entry) {
    var fadeLength = 250;

    // What the fuck, GitHub?
    // var firstFile = entry.files[Object.keys(entry.files)[0]];
    // var actualUrl = firstFile.raw_url.replace("gist.github.com/raw/", "raw.github.com/gist/");
    var requestUrl = "https://api.github.com/gists/" + entry.id;

    return function () {
      $("#examples_throbber").fadeIn(fadeLength);
      $("#examples_list").fadeTo(fadeLength, 0.001);

      $.ajax({
        url: requestUrl,
        dataType: 'jsonp',
        success: function (resultGist) {
          $("#examples_throbber").fadeOut(fadeLength);
          $("#examples_list").fadeTo(fadeLength, 1);

          var firstFile = resultGist.data.files[Object.keys(resultGist.data.files)[0]];

          document.getElementById("sourcecode").value = firstFile.content;
          window.cseditor.setValue(firstFile.content);
        }
      });
    };
  };

  for (var i = 0; i < entries.length; i++) {
    var entry = entries[i];

    var li = document.createElement("li");
    var a = document.createElement("a");
    li.appendChild(a);

    a.appendChild(document.createTextNode(entry.description));

    $(a).click(makeClickHandler(entry));

    container.append(li);
  }

  $("#examples_throbber").fadeOut();
  $("#examples_list").fadeIn();
};

function beginCompile () {
  $("#throbber").fadeIn();

  var btn = $("#compile");
  btn.attr("disabled", "disabled");
  btn.fadeOut();

  clearOutputWindow();

  var sourceCode = window.cseditor.getValue() || document.getElementById("sourcecode").value;
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
    setJavascript(data.javascript);
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
  var maxWidth = totalWidth - 110;

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
  window.setTimeout(function runTranslatedJS () {
    try {
      if (warnings && warnings.length > 0) {
        outputWindow.JSIL.Host.logWriteLine("// Begin Warnings //");
        outputWindow.JSIL.Host.logWriteLine(warnings);
        outputWindow.JSIL.Host.logWriteLine("// End Warnings //");
      }

      outputWindow.runScript({
        javascript: javascript, 
        entryPoint: entryPoint
      });
    } finally {
      setOutputThrobberVisible(outputDoc, false);
    }
  }, 10);
};

function setJavascript (text) {
  document.getElementById("javascript").value = text;
  window.jseditor.setValue(text);
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

  $("#compile").click(beginCompile);

  function initSplitter (x, y) {
    splitterDragStart = [x, y];
    leftColumnWidthStart = document.getElementById("left_column").offsetWidth;
  };

  var body = document.getElementsByTagName("body")[0];

  $("#splitter").mousedown(function (evt) {
    isDraggingSplitter = true;
    initSplitter(evt.clientX, evt.clientY);
    window.cseditor.refresh();
    window.jseditor.refresh();
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
      window.cseditor.refresh();
      window.jseditor.refresh();
      evt.preventDefault();
      evt.stopPropagation();
    }
  }, true);

  window.cseditor = CodeMirror.fromTextArea(document.getElementById("sourcecode"), {
    mode: "clike",
    lineNumbers: true,
    lineWrapping: true
  });

  window.jseditor = CodeMirror.fromTextArea(document.getElementById("javascript"), {
    mode: "javascript",
    lineNumbers: true,
    lineWrapping: true
  });

  setJavascript("");

  initSplitter(0, 0);
  updateSplitter(0);

  window.cseditor.refresh();
  window.jseditor.refresh();

  loadExamples();

  $("#compile").removeAttr("disabled");
};