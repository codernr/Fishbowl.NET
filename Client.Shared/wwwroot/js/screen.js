export const ScreenModule = {
    objectReference: undefined,

    initialize: function(objectReference) {
        this.objectReference = objectReference;

        if (!this.requestFullScreenEnabled()) return;

        window.document.addEventListener('fullscreenchange', () =>
            this.objectReference.invokeMethodAsync('OnFullScreenChange', window.document.fullscreenElement ? true : false));
    },

    requestWakeLock: function() {
        if ('wakeLock' in window.navigator) window.navigator.wakeLock.request('screen').catch(() => {});
    },

    requestFullScreen: function() {
        if (this.requestFullScreenEnabled())
            window.document.documentElement.requestFullscreen().catch(() => {});
    },

    exitFullScreen: function() {
        if (window.document.exitFullscreen) window.document.exitFullscreen().catch(() => {});
    },

    requestFullScreenEnabled: function() {
        return window.document.documentElement?.requestFullscreen ? true : false;
    }
};