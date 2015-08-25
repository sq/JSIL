var gulp = require('gulp');
var metascriptPipe = require('gulp-metascript');

gulp.task('metascript', function () {
  return gulp.src('Sources/**/*.js')
        .pipe(metascriptPipe())
        .on('error', logError)
        .pipe(gulp.dest('../Libraries/'));
});

gulp.task('build-Debug', ['metascript']);
gulp.task('build-Release', ['metascript']);



/*Helpers*/
function logError(error) {
  console.error(error.message);
  process.exit(1);
}
