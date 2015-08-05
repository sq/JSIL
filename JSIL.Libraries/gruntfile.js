module.exports = function (grunt) {
    'use strict';

    grunt.loadNpmTasks('grunt-preprocessor');

    grunt.initConfig({
        preprocessor: {
            main: {
                options:
                {
                    root: "Includes"
                },
                files: [{
                    expand: true,
                    cwd: 'Sources',
                    src: ['**/*.js'],
                    dest: '../Libraries'
                }]
            }
        }
    });

    grunt.registerTask('build-Debug', ['preprocessor']);
    grunt.registerTask('build-Release', ['preprocessor']);
};