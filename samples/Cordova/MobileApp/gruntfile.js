/// <binding Clean='bowerInstall, productionCss' ProjectOpened='bowerInstall, productionCss' />
// This file in the main entry point for defining grunt tasks and using grunt plugins.
// Click here to learn more. http://go.microsoft.com/fwlink/?LinkID=513275&clcid=0x409

module.exports = function (grunt) {
    grunt.initConfig({
        bower: {
            install: {
                options: {
                    targetDir: 'www',
                    layout: function (type, component, source) { return type; },
                    install: true,
                    verbose: true,
                    cleanTargetDir: false,
                    bowerOptions: {}
                }
            }
        }
    });

    grunt.loadNpmTasks("grunt-bower-task");

};