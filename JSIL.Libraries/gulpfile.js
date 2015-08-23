var gulp = require('gulp');
var gutil = require('gulp-util');
var metascriptPipe = require('gulp-metascript');

gulp.task('metascript', function () {
  return gulp.src('Sources/**/*.js')
        .pipe(workDirFix(metascriptPipe()))
        .on('error', logError)
        .pipe(gulp.dest('../Libraries/'));
});

gulp.task('build-Debug', ['metascript']);
gulp.task('build-Release', ['metascript']);



/*Helpers*/
function workDirFix(innerPipe) {
  var through = require('through2'), path = require('path');
  function fixWorkingDirectory(file, encoding, callback) {
    var dir = process.cwd();
    try {
      process.chdir(path.dirname(file.path));
      return innerPipe._transform(file, encoding, callback);
    } catch (e) {
      console.log("!!!")
    } finally {
      process.chdir(dir);
    }
  }
  return through.obj(fixWorkingDirectory);
}

function logError(error) {
  console.error(error.message);
  process.exit(1);
}
