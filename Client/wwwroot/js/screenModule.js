window.ScreenModule = {
    requestWakeLock: function() {
        if ('wakeLock' in window.navigator) window.navigator.wakeLock.request('screen');
    },
    requestFullScreen: function() {
        window.document.documentElement.requestFullscreen();
    }
};