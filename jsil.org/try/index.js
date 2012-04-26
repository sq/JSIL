function beginCompile () {
  $("#compile").attr("disabled", "disabled");

  var sourceCode = document.getElementById("sourcecode").value;

  $.post(
    "http://jsil.org/try/compile.aspx",
    sourceCode,
    compileComplete,
    "json"
  );
};

function compileComplete (data, status) {
  $("#compile").removeAttr("disabled");

  if (data && data.ok) {
    document.getElementById("javascript").value = data.javascript;
  } else {
    alert("Compile failed: " + String(data.error || status));
  }
};

function onLoad () {
  var compileButton = $("#compile");
  compileButton.click(beginCompile);
};