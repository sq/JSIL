function loadExamples () {
  var username = "TryJSIL"; 
  var path = "/users/" + username + "/gists";
  loadGists(path, "examples");
};

function loadMyGists () {
  if (!githubLoginCode) {
    $("my_gists").fadeOut();
    return;
  } else {  
    document.getElementById("my_gists").style.display = "block";
    $("my_gists").fadeIn();
    var path = "/gists?access_token=" + githubLoginCode;
    loadGists(path, "my_gists");
  }
};

function loadGists (path, prefix) {
  var requestUrl = 'https://api.github.com' + path;

  $.ajax({
    url: requestUrl,
    dataType: 'jsonp',
    success: function (result) {
      displayGists(result, prefix);
    }
  });
};

function displayGists (result, prefix) {
  console.log("prefix=", prefix);
  
  var container = $("#" + prefix + "_list");
  container.empty();

  var entries = result.data;

  var makeClickHandler = function (entry) {
    return function () {
      var fadeLength = 250;

      $("#" + prefix + "_throbber").fadeIn(fadeLength);
      $("#" + prefix + "_list").fadeTo(fadeLength, 0.001);

      loadExistingGist(entry.id, function () {
        $("#" + prefix + "_throbber").fadeOut(fadeLength);
        $("#" + prefix + "_list").fadeTo(fadeLength, 1);
      });
    };
  };

  for (var i = 0; i < entries.length; i++) {
    var entry = entries[i];

    var fileKeys = Object.keys(entry.files);
    if (fileKeys.length !== 1)
      continue;

    // Ignore gists that aren't C#
    var fileLanguage = (entry.files[fileKeys[0]].language || "").trim();
    if (fileLanguage != "C#")
      continue;
    
    // Ignore gists without titles
    if ((entry.description || "").trim().length < 1)
      continue;

    var li = document.createElement("li");
    var a = document.createElement("a");

    a.href = "http://jsil.org/try/#" + entry.id;

    li.appendChild(a);

    a.appendChild(document.createTextNode(entry.description));

    $(a).click(makeClickHandler(entry));

    container.append(li);
  }

  $("#" + prefix + "_throbber").fadeOut();
  $("#" + prefix + "_list").fadeIn();
};

var githubLoginCode = null;
var githubUserId = null;
var githubUserName = null;
var githubLoginInterval = null;
var savingGist = false, savePending = false;
var existingGistName = null;
var existingGistOwnerId = null;
var existingGistId = null;
var existingGistFile = null;
var existingGistForkedFromId = null;
var existingGistForkedFromName = null;
var loadingGist = null;
var controlsEnabled = false;

function loadExistingGist (gistId, callback) {
  if (loadingGist !== null) {
    console.log("Already loading a gist. Aborting.");

    if (typeof (callback) === "function")
      callback();

    return;
  }

  if (gistId == existingGistId) {
    if (typeof (callback) === "function")
      callback();

    return;
  }

  loadingGist = gistId;

  var requestUrl = "https://api.github.com/gists/" + gistId;

  setControlsEnabled(false);
  setStatus("Loading gist #" + gistId + "...");

  $.ajax({
    url: requestUrl,
    dataType: 'jsonp',
    success: function (resultGist) {
      var user;
      if (typeof (callback) === "function")
        callback();

      setControlsEnabled(true);

      existingGistFile = Object.keys(resultGist.data.files)[0];
      var firstFile = resultGist.data.files[existingGistFile];

      document.getElementById("sourcecode").value = firstFile.content;

      highlightErrorLines(null);
      window.cseditor.setValue(firstFile.content);

      var forkedFromName = null, forkedFromId = null;

      if (resultGist.data.fork_of && resultGist.data.fork_of.id) {
        user = resultGist.data.fork_of.user || resultGist.data.fork_of.owner;
        forkedFromName = user.login;
        forkedFromId = resultGist.data.fork_of.id;
      }
      
      user = resultGist.data.user || resultGist.data.owner;

      setCurrentGist(
        gistId, resultGist.data.description, 
        user.login, user.id,
        forkedFromName, forkedFromId
      );

      setStatus("Loaded gist.");
    }
  });
};

function ownsExistingGist () {
  return String(existingGistOwnerId).trim() == String(githubUserId).trim();
};

function setCurrentGist (gistId, gistName, ownerName, ownerId, forkedFromName, forkedFromId) {
  var elt = document.getElementById("source_caption");
  elt.innerHTML = "";

  var shareUrl = "http://jsil.org/try/#" + gistId;

  var gistLink = document.createElement("a");
  gistLink.href = shareUrl;
  gistLink.appendChild(document.createTextNode(gistName + " by " + ownerName));

  elt.appendChild(gistLink);

  if (forkedFromName && forkedFromId) {
    elt.appendChild(document.createTextNode(" (fork of "));

    var forkLink = document.createElement("a");
    forkLink.href = "http://jsil.org/try/#" + forkedFromId;
    forkLink.appendChild(document.createTextNode(forkedFromName + "'s version"));

    elt.appendChild(forkLink);
    elt.appendChild(document.createTextNode(")"));
  }

  existingGistName = gistName;
  existingGistOwnerId = ownerId;
  existingGistForkedFromName = forkedFromName;
  existingGistForkedFromId = forkedFromId;
  existingGistId = String(gistId).trim();
  loadingGist = null;
  window.location.hash = "#" + gistId;

  document.getElementById("share_link").href = shareUrl;

  document.getElementById("save_gist").innerHTML = 
    ownsExistingGist() ? "Update Gist" : "Fork Gist";
};

function beginSaveGist () {
  if (githubLoginCode === null) {
    savePending = true;
    beginGithubLogin();
    return;
  }

  savePending = false;
  setControlsEnabled(false);

  if (existingGistName) {
    document.getElementById("gist_name").value = existingGistName;
  }

  savingGist = true;
  $("#save_gist_container").fadeIn();
};

function confirmSaveGist () {
  $("#save_gist_container").fadeOut();

  setStatus("Saving gist...");

  var requestUrl, method;
  var isForked = false;

  var makePatchUrl = function (gistId) {
    return "https://api.github.com/gists/" + gistId + "?access_token=" + githubLoginCode;
  };

  if (ownsExistingGist() && (existingGistId !== null)) {
    requestUrl = makePatchUrl(gistId);
    method = "PATCH";
  } else if (existingGistId !== null) {    
    requestUrl = "https://api.github.com/gists/" + existingGistId + "/fork?access_token=" + githubLoginCode;
    method = "POST";
    isForked = true;
  } else {
    requestUrl = "https://api.github.com/gists?access_token=" + githubLoginCode;
    method = "POST";
  }

  var gistName = document.getElementById("gist_name").value;

  var files = {};
  var fileKey = "tryjsil.cs";

  if (existingGistFile) {
    fileKey = existingGistFile;
    files[existingGistFile] = { "content": null };
  }

  files[fileKey] = {
    "content": window.cseditor.getValue() || document.getElementById("sourcecode").value
  };

  existingGistFile = fileKey;

  var postData = {
    public: true,
    description: gistName,
    files: files
  };

  var onSuccessful = function (result) {
    setStatus("Save successful.");
    var user = result.user || result.owner;
    setCurrentGist(result.id, result.description, user.login, user.id);
    setControlsEnabled(true);
    window.setTimeout(loadMyGists, 500);
  };

  $.ajax({
    url: requestUrl,
    type: method,
    data: JSON.stringify(postData),
    dataType: "json",
    success: function (result) {
      if (isForked) {
        setStatus("Forked gist. Saving...");
        $.ajax({
          url: makePatchUrl(result.id),
          type: "PATCH",
          data: JSON.stringify(postData),
          dataType: "json",
          success: function (result2) {
            onSuccessful(result2);
          },
          error: function (xhr, status, moreStatus) {
            setStatus("Save failed: " + status + ": " + moreStatus);
            setControlsEnabled(true);
          }
        });
      } else {
        onSuccessful(result);
      }
    },
    error: function (xhr, status, moreStatus) {
      setStatus("Save failed: " + status + ": " + moreStatus);
      setControlsEnabled(true);
    }
  });
};

function cancelSaveGist () {
  setControlsEnabled(true);
  savingGist = false;
  $("#save_gist_container").fadeOut();

  setStatus("Save cancelled.");
};

function beginGithubLogin () {
  if (githubLoginInterval !== null)
    return;

  setStatus("Logging in to GitHub...");

  setControlsEnabled(false);

  var loginWindow = window.open(
    "oauth_login.html",
    "GitHub Login",
    "width=960,height=500,menubar=no,toolbar=no,location=no,personalbar=no,status=no,resizable=no,scrollbars=no,dependent=yes,dialog=yes,minimizable=yes"
  );

  githubLoginInterval = window.setInterval(function () {
    if (loginWindow.closed) {
      window.clearInterval(githubLoginInterval);
      githubLoginInterval = null;
      setControlsEnabled(true);
    }
  }, 10);
};

function getUserInfo () {
  var requestUrl = "https://api.github.com/user?access_token=" + githubLoginCode;

  var onFailed = function () {
    $.cookie("githubAccessToken", null);
    $.cookie("githubUserId", null);
    $.cookie("githubUserName", null);
  };

  // GitHub's OAuth API is dumb and requires us to send them a secret just to get a token back.
  $.ajax({
    type: 'GET',
    url: requestUrl,
    dataType: 'jsonp',
    success: function (result) {
      // Not logged in or bad token
      if (parseInt(result.meta.status) >= 400) {
        console.log("Login cookie invalid: ", result.data.message);
        onFailed();
        return;
      }

      githubUserName = result.data.login;
      githubUserId = result.data.id;
      $.cookie("githubUserId", result.data.id);
      $.cookie("githubUserName", result.data.login);

      console.log("Logged in as ", githubUserName, " (", githubUserId, ")");
      setControlsEnabled(controlsEnabled);

      loadMyGists();
    },
    error: function (xhr, status, moreStatus) {
      console.log("Failed to get logged in user info: " + status + ": " + moreStatus);
      onFailed();
    }
  });  
}

function endGithubLogin (uri) {
  if (uri.indexOf("?error") >= 0) {
    setStatus("GitHub login failed");
    githubLoginCode = null;
    return;
  }

  var code = uri.replace("?code=", "").trim();
  console.log("Login code: " + code);

  // GitHub's OAuth API is dumb and requires us to send them a secret just to get a token back.
  $.ajax({
    type: 'GET',
    url: "http://jsil.org/try/oauth_get_token.aspx",
    data: {
      code: code
    },
    dataType: 'json',
    success: function (result) {
      if (result.ok) {
        githubLoginCode = result.response.access_token;
        $.cookie("githubAccessToken", githubLoginCode);
        setStatus("GitHub login successful.");
        getUserInfo();

        if (savePending)
          beginSaveGist();
      } else {
        setStatus("GitHub authorization failed: " + JSON.stringify(result.response));
      }
    },
    error: function (xhr, status, moreStatus) {
      setStatus("GitHub authorization failed: " + status + ": " + moreStatus);
    }
  });  
};

function setControlsEnabled (enabled) {
  controlsEnabled = enabled;

  if (enabled) {
    $("#throbber").fadeOut();
  } else {
    $("#throbber").fadeIn();
  }

  var btn = $("#compile");
  if (enabled) {
    btn.removeAttr("disabled");
    btn.fadeIn();
  } else {
    btn.attr("disabled", "disabled");
    btn.fadeOut();
  }

  var btn = $("#save_gist");
  if (enabled) {
    btn.removeAttr("disabled");
    btn.fadeIn();
  } else {
    btn.attr("disabled", "disabled");
    btn.fadeOut();
  }
};

function beginCompile () {
  setControlsEnabled(false);

  clearOutputWindow();

  var sourceCode = window.cseditor.getValue() || document.getElementById("sourcecode").value;
  setStatus("Compiling...");

  $.ajax({
    // jQuery is so stupid
    processData: false,
    contentType: "text/plain; charset=UTF-8",
    
    type: 'POST',
    url: "http://jsil.org/try/compile.aspx",
    data: sourceCode,
    success: compileComplete,
    error: function (xhr, status, moreStatus) {
      compileComplete(false, status + ": " + moreStatus);
    },
    cache: false,
    dataType: "json"
  });
};

function compileComplete (data, status) {
  setControlsEnabled(true);

  if (data && data.ok) {
    setJavascript(data.javascript);
    setStatus(
      "Compile successful.<br>" +
      "C# compile took " + data.compileElapsed + " second(s).<br>" +
      "Translation took " + data.translateElapsed + " second(s)."
    );

    highlightErrorLines(null);
    runInOutputWindow(data.javascript, data.entryPoint, data.warnings);
  } else {
    var errorText = String(data.error || status);
    highlightErrorLines(errorText);
  }
};

function highlightErrorLines(errorText) {
  for (var i = 0, sourceLineCount = (window.cseditor.getValue().split("\n").length); i < sourceLineCount; i++) {
    try {
      window.cseditor.setLineClass(i, null, null);
      window.cseditor.setMarker(i, null, null);
    } catch (exc) {
      // CodeMirror is buggy
    }
  }

  if (errorText === null)
    return;

  var markLine = function (i, type) {
    var lineHandle = window.cseditor.setLineClass(i, "compile" + type, "compile" + type + "Background");
    var markerHandle = window.cseditor.setMarker(i, "\u25CF", "compile" + type);
  };

  var errorRegex = /\(([0-9]*),([0-9]*)\) \: (error|warning) (CS[0-9]*)/;

  var newNodes = [];

  var createLineLinkHandler = function (lineIndex, colIndex) {
    return function () {
      var pos = {line: lineIndex, ch: colIndex};

      var lineText = window.cseditor.getLine(lineIndex);

      window.cseditor.setCursor(pos);
      window.cseditor.setSelection(pos, {
        line: lineIndex, ch: colIndex + (lineText.length - colIndex)
      });
    };
  };

  var lines = errorText.split('\n');
  for (var i = 0; i < lines.length; i++) {
    var line = lines[i];

    var match = errorRegex.exec(line);

    if (match) {
      var matchType = match[3];
      var lineIndex = parseInt(match[1]) - 1;
      var colIndex = parseInt(match[2]) - 1;

      try {
        markLine(lineIndex, matchType);
      } catch (exc) {
        // CodeMirror is buggy
      }

      var errorLink = document.createElement("a");
      errorLink.addEventListener("click", createLineLinkHandler(lineIndex, colIndex), true);
      errorLink.appendChild(document.createTextNode(line));
      newNodes.push(errorLink);
    } else {
      newNodes.push(document.createTextNode(line));
    }

    newNodes.push(document.createElement("br"));
  }

  var s = document.getElementById("status");
  s.innerHTML = "";

  for (var i = 0; i < newNodes.length; i++) {
    s.appendChild(newNodes[i]);
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
  splitter.style.left = (newLeftWidth + leftColumn.offsetLeft + 3) + "px";
}

function setStatus (text) {
  var s = document.getElementById("status");
  s.innerHTML = text;
};

var pendingRunInterval = null;

function runInOutputWindow (javascript, entryPoint, warnings) {
  var outputWindow = document.getElementById("iframe").contentWindow;
  var outputDoc = document.getElementById("iframe").contentDocument;

  setOutputThrobberVisible(outputDoc, true);

  var doRun = function runTranslatedJS () {
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
  }

  pendingRunInterval = window.setInterval(function () {
    if (outputWindow.isReady === true) {
      window.setTimeout(doRun, 10);
      window.clearInterval(pendingRunInterval);
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

var lastHash = null;
function checkHash () {
  var currentHash = null;
  if ((typeof (window.location.hash) === "string") && (window.location.hash.length > 1)) {
    currentHash = window.location.hash.substr(1);
  }

  if (currentHash !== null)
    loadExistingGist(currentHash.trim());
}

function onLoad () {
  $("#compile").click(beginCompile);
  $("#save_gist").click(beginSaveGist);
  $("#confirm_save_gist").click(confirmSaveGist);
  $("#cancel_save_gist").click(cancelSaveGist);

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
    mode: "text/x-csharp",
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

  setControlsEnabled(true);

  document.getElementById("iframe").contentDocument.getElementById("throbber").style.display = "none";

  githubLoginCode = $.cookie("githubAccessToken") || null;

  githubUserId = $.cookie("githubUserId") || null;
  githubUserName = $.cookie("githubUserName") || null;

  if (githubLoginCode)
    getUserInfo();

  checkHash();
  setInterval(checkHash, 1000);  
};