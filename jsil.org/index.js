function loadChangelog () {
  $.ajax({
    url: 'http://github.com/api/v2/json/commits/list/kevingadd/JSIL/master',
    dataType: 'jsonp',
    success: function (data) {
      var container = $("#changelog-entries");
      var template = $("#changelog-entry-template");
      
      for (var i = 0, l = data.commits.length; i < l; i++) {
        var commit = data.commits[i];
        var entry = template.clone();
        
        var gravatar_url = "http://www.gravatar.com/avatar/" + hex_md5(commit.author.email);
        var author_url = "http://www.github.com/" + commit.author.login;
        var commit_url = "http://www.github.com" + commit.url;
        var commitDate = new Date(commit.committed_date);
        
        entry.children(".author-image").first().attr("src", gravatar_url);
        entry.children(".message").first().children(".text").first().text(commit.message);
        
        var header = entry.children(".header").first();        
        header.children(".author").first().text(commit.author.login);
        header.children(".author").first().attr("href", author_url);
        
        var datetime = header.children(".datetime").first();        
        datetime.children(".date").first().text(commitDate.toLocaleDateString());
        datetime.children(".time").first().text(commitDate.toLocaleTimeString());
        datetime.attr("href", commit_url);
        
        entry.css("display", "block");
        
        container.append(entry);
      }
      
      $("#changelog-loading-placeholder").fadeOut();
      container.fadeIn();
    }
  });
}

function loadNewSample () {
  var h = window.location.hash || "";
  if (h.length > 1) {
    var url = h.substr(1);
    
    if ($("#codesample").attr("src") != url) {
      $("#codesample").attr("src", url);
    }
  }
}

function onLoad () {
  window.addEventListener("hashchange", loadNewSample, false);
  
  $("ul#sample_list li a").each(function () {
    var link = $(this);
    link.click(function (evt) {
      evt.preventDefault();
      var sampleUrl = link.attr("href");
      window.location.hash = "#" + sampleUrl;
    });
  });
  
  loadChangelog();
  loadNewSample();
}