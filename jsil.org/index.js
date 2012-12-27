function displayCommits (username, reponame, branch, result) { 
  var container = $("#changelog_entries");
  container.empty();

  var template = $("#changelog_entry_template");
  
  var data = result.data;
  
  for (var i = 0, l = data.length; i < l; i++) {
    var item = data[i];
    var entry = template.clone();
    
    var commit = item.commit;
    
    var gravatar_url = item.author.avatar_url;
    var author_url = "http://www.github.com/" + item.author.login;
    var commit_url = "http://www.github.com/" + username + "/" + reponame + "/commit/" + item.sha;
    var commitDate = new Date(commit.author.date);
    
    entry.children(".author-image").first().attr("src", gravatar_url);
    entry.children(".message").first().children(".text").first().text(commit.message);
    
    var header = entry.children(".header").first();        
    
    header.children(".author").first().text(commit.author.name);
    header.children(".author").first().attr("href", author_url);
    header.children(".author").first().attr("title", "View author's page on github");
    
    var datetime = header.children(".datetime").first();        
    datetime.children(".date").first().text(commitDate.toLocaleDateString());
    datetime.children(".time").first().text(commitDate.toLocaleTimeString());
    datetime.attr("href", commit_url);
    datetime.attr("title", "View commit on github");
    
    entry.css("display", "block");
    
    container.append(entry);
  }
  
  $("#changelog_loading_placeholder").fadeOut();
  container.fadeIn();
};

function displayBranches (activeBranch, result) {
  var container = $("#branch_list");
  container.empty();
  
  var data = result.data;
  
  for (var i = 0, l = data.length; i < l; i++) {
    var branch = data[i];
    var entry = document.createElement("a");
    
    $(entry).text(branch.name);
    $(entry).attr("href", "javascript:loadChangelog('" + branch.name + "')");
    $(entry).attr("title", "View most recent commits on branch '" + branch.name + "'");
    $(entry).addClass("branch");
    
    if (branch.name == activeBranch)
      $(entry).addClass("active");
    
    container.append(entry);
  }
};

function loadChangelog (branch) {
  var username = "kevingadd";
  var reponame = "JSIL";
  
  if (typeof (branch) === "undefined")
    branch = "master";
  
  $("#changelog_loading_placeholder").fadeIn();
  $("#changelog_entries").fadeOut();
  
  $.ajax({
    url: 'https://api.github.com/repos/' + username + '/' + reponame + '/branches',
    dataType: 'jsonp',
    success: function (result) {
      displayBranches(branch, result);      
        
      $.ajax({
        url: 'https://api.github.com/repos/' + username + '/' + reponame + '/commits?sha=' + branch,
        dataType: 'jsonp',
        success: function (result) {
          displayCommits(username, reponame, branch, result);
        }
      });
    }
  });
};

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
  
  var carouselE = $("#demo_carousel");
  carouselE.carousel({
    loop: true,
    direction: "horizontal",
    dispItems: 3,
    pagination: false,
    autoSlide: true,
    autoSlideInterval: 6000,
    animSpeed: 800
  });
  carouselE.fadeIn();
}