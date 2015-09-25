(function() {
    'use strict';

    var theModule = angular.module('common.download.service', [
        'common.logging.service',
        'ngFileSaver'
    ]);

    theModule.factory('DownloadService', function($q, $http, $window, FileSaver, LoggingService) {
        var service = {};

        var log = LoggingService.getLogger('DownloadService');
        var window = $window;
        window.requestFileSystem  = window.requestFileSystem || window.webkitRequestFileSystem;
        window.storageInfo = window.storageInfo || window.webkitStorageInfo;

        service.downloadChunkedResource = function(url, fileName) {
            var hiddenElement = document.createElement('a');

            hiddenElement.href = url;
            hiddenElement.target = '_blank';
            hiddenElement.download = fileName;
            hiddenElement.click();
        };

        service.download = function(url, fileName) {
            var deferred = $q.defer();
            $http({
                    method: 'GET',
                    url: url,
                    responseType: 'blob',
                    cache: true
                }
            ).success(
                function(data) {
                    FileSaver.saveAs({
                        data: data,
                        filename: fileName
                    });

                    deferred.resolve();
                }
            ).error(function(e) {
                    log.error('download error.', e);
                    deferred.reject('Failed to load localized resources');
            });

            return deferred.promise;
        };

        service.download1 = function(url, fileName) {
            var defer = $q.defer();

            var xhr = new XMLHttpRequest();
            xhr.open('GET', url, true);
            xhr.responseType = 'blob';
            xhr.onerror = function(e) {
                log.error('download error.', e);
                defer.reject(e);
            };
            xhr.onload = function(e) {
                if (this.status === 200) {
                    FileSaver.saveAs({
                        data: this.response,
                        filename: fileName
                    });

                    defer.resolve();
                } else {
                    log.error('download failed, response status:' + this.status, e);
                    defer.reject(e);
                }
            };
            xhr.send();

            return defer.promise;
        };

        service.downloadChunkedResourceEx = function(url, grantBytes) {
            var defer = $q.defer();

            var fsType = window.TEMPORARY;
            var fsSize = angular.isDefined(grantBytes) ? grantBytes : 10 * 1024 * 1024;
            var path = 'observation.csv';
            window.storageInfo.requestQuota(fsType, fsSize, function(gb) {
                window.requestFileSystem(fsType, gb, function(fileSystem) {
                    fileSystem.root.getFile(path, {create: true}, function(fileEntry) {
                        fileEntry.createWriter(function(writer) {

                            //compose xhr
                            var xhr = new XMLHttpRequest();
                            xhr.open('GET', url, true);
                            xhr.responseType = 'blob';
                            xhr.onerror = function(e) {
                                deferReject(defer, 'Error occurred during XHR download.', e);
                            };
                            xhr.onload = function(e) {
                                if (this.status === 200) {
                                    deferResolve(defer, 'download successfully.');
                                } else {
                                    deferReject(defer, 'Error XHR call reply ' + this.status, e);
                                }

                                defer.resolve(xhr.status);
                            };
                            xhr.onprogress = function(e) {
                                //handle trunk append file here
                                console.log('PROGRESS:', e.responseText);
                                writer.write(e.responseText);
                            };
                        }, function(e) {
                            deferReject(defer, 'Error creating FileWriter', e);
                        });
                    }, function(e) {
                        deferReject(defer, 'Error create fle ' + path, e);
                    });
                }, function(e) {
                    deferReject(defer, 'Error requesting File System access', e);
                });
            }, function(e) {
                deferReject(defer, 'Error requesting Quota', e);
            });

            return defer.promise;
        };

        return service;
    });
})();
