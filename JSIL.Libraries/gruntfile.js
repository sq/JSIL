module.exports = function(grunt) {
  'use strict';

  grunt.loadNpmTasks('grunt-metascript');

  grunt.initConfig({
    metascript: {
      main: {
        options:
        {
          mode: 'transform'
        },
        files: [
          {
            expand: true,
            cwd: 'Sources',
            src: ['**/*.js'],
            dest: '../Libraries'
          }
        ]
      }
    }
  });

  grunt.registerTask('build-Debug', ['metascript']);
  grunt.registerTask('build-Release', ['metascript']);
};