'use strict';
app.factory('utilitiesService', [function () {

    var utilitiesServiceFactory = {};
    
    var _startsWith = function (str, searchvalue) {
        return str.indexOf(searchvalue) == 0;
    };

    var _getURLParameter = function (url, name) {
        return decodeURIComponent((new RegExp('[?|&]' + name + '=' + '([^&;]+?)(&|#|;|$)').exec(url) || [, ""])[1].replace(/\+/g, '%20')) || null
    };

    utilitiesServiceFactory.startsWith = _startsWith;
    utilitiesServiceFactory.getURLParameter = _getURLParameter;

    return utilitiesServiceFactory;
}]);