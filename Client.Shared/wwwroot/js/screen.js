export const ScreenModule = {
    requestWakeLock: function() {
        if ('wakeLock' in window.navigator) window.navigator.wakeLock.request('screen').catch(() => {});
    },
    requestFullScreen: function() {
        if (this.requestFullScreenEnabled())
            window.document.documentElement.requestFullscreen().catch(() => {});
    },
    requestFullScreenEnabled: function() {
        return window.document.documentElement?.requestFullscreen ? true : false;
    }
};