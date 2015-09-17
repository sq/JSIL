var gulp = require('gulp');
var metascriptPipe = require('gulp-metascript');
var chmod = require('gulp-chmod');
var clean = require('gulp-clean');
var replace = require('gulp-replace');
var eol = require('gulp-eol');

var header = "/* It is auto-generated file. Do not modify it. */";

gulp.task('metascript', ['clean'], function () {
  return gulp.src(['Sources/**/*.js', 'Generated/**/*.js'])
    .pipe(metascriptPipe())
    .pipe(replace(/^\uFEFF/gm, '')) // remove BOM from file internal parts
    .pipe(replace(/(.)/, '\uFEFF' + header + '\n$1')) // add BOM with header
    .pipe(eol(undefined, false))
    .pipe(chmod({ write: false }))
    .on('error', logError)
    .pipe(gulp.dest('../Libraries/'));
});

gulp.task('clean', function () {
  return gulp.src('../Libraries/*', { read: false })
    .pipe(clean({ force: true }));
});

gulp.task('build-Debug', ['metascript']);
gulp.task('build-Release', ['metascript']);



/*Helpers*/
function logError(error) {
  console.error(error.message);
  process.exit(1);
}
