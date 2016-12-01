'use strict';
app.controller('authController', ['$sce', '$scope', '$location', 'authService', function ($sce, $scope, $location, authService) {

    $scope.message = "";

    $scope.logIn = function () {
        authService.logIn().then(function (response) {
            authService.confirmMessage().then(function (response) { $scope.message = response; });
        }, function (err) {
            $scope.message = err;
        });
    };

    $scope.logOut = function () {
        authService.logOut().then(function (response) {
            $scope.message = "";
        }, function (err) {
            $scope.message = err;
        });
    };

    $scope.queryServer = function () {
        authService.confirmMessage().then(function (response) { $scope.message = response; });
    };

    $scope.authentication = authService.authentication;
}]);