export const ScreenModule = {
    requestWakeLock: function() {
        if ('wakeLock' in window.navigator) window.navigator.wakeLock.request('screen').catch(() => {});
    },
    requestFullScreen: function() {
        if (window.document.documentElement?.requestFullscreen)
            window.document.documentElement.requestFullscreen().catch(() => {});
    }
};