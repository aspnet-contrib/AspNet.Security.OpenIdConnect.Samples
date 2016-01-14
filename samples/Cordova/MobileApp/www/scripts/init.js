var app = angular.module('mainApp', ['LocalStorageModule', 'ionic', 'angular-jwt']);

app.constant('authConfig', {
    clientId: 'myClient',
    logInApi: 'http://localhost:54540/connect/authorize',
    logOutApi: 'http://localhost:54540/connect/logout',
    redirect_uri: 'http://localhost/callback',
    post_logout_redirect_uri: 'http://localhost/callback'
});

app.config(function ($httpProvider) {
    $httpProvider.interceptors.push('authInterceptorService');
});

app.run(['authService', function (authService) {
    authService.fillAuthData();
}]);

