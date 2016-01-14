'use strict';
app.factory('authService', ['$http', '$q', 'localStorageService', 'utilitiesService', 'jwtHelper', 'authConfig', function ($http, $q, localStorageService, utilitiesService, jwtHelper, authConfig) {

    var authServiceFactory = {};

    var _authentication = {
        isAuth: false,
        userName: ""
    };

    var _logIn = function () {
        var deferred = $q.defer()
        var ref = window.open(authConfig.logInApi + '?client_id=' + authConfig.clientId + '&redirect_uri=' + authConfig.redirect_uri + '&response_type=token', '_blank', 'location=no');
        ref.addEventListener('loadstart', function (event) {
            if (utilitiesService.startsWith(event.url, authConfig.redirect_uri)) {
                var requestToken = utilitiesService.getURLParameter(event.url, "access_token");
                var userName = jwtHelper.decodeToken(requestToken).unique_name;
                localStorageService.set('authorizationData', { token: requestToken, userName: userName });
                _fillAuthData();
                deferred.resolve(ref.close());
            };
        });
        return deferred.promise;
    };

    var _logOut = function () {
        var deferred = $q.defer()
        var ref = window.open(authConfig.logOutApi + '?post_logout_redirect_uri=' + authConfig.post_logout_redirect_uri, '_blank', 'location=no');
        ref.addEventListener('loadstart', function (event) {
            if (utilitiesService.startsWith(event.url, authConfig.post_logout_redirect_uri)) {
                localStorageService.remove('authorizationData');
                _authentication.isAuth = false;
                _authentication.userName = "";
                deferred.resolve(ref.close());
            };
        });
        return deferred.promise;
    };

    var _fillAuthData = function () {
        var authData = localStorageService.get('authorizationData');
        if (authData) {
            _authentication.isAuth = true;
            _authentication.userName = authData.userName;
        }
    };
    
    var _confirmMessage = function () {
        return $http.get("http://localhost:54540/api/message").then(function successCallback(response) {
            return response.data;
        });
    };

    authServiceFactory.logIn = _logIn;
    authServiceFactory.logOut = _logOut;
    authServiceFactory.fillAuthData = _fillAuthData;
    authServiceFactory.confirmMessage = _confirmMessage;
    authServiceFactory.authentication = _authentication;

    return authServiceFactory;
}]);