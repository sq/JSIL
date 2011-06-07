function onLoad () {
  var height = Math.max(
    $("#javascript .gist .gist-file .gist-data").height(),
    $("#csharp .gist .gist-file .gist-data").height()
  );
  
  $("div .gist .gist-file .gist-data").css("height", height);
  
  var totalHeight = Math.max(
    $("#javascript .gist").height(),
    $("#csharp .gist").height()
  );  
  
  $(parent.document.getElementById("codesample")).animate(
    {"min-height": totalHeight + "px"},
    500
  );
}