export const ScreenModule = {
    objectReference: undefined,

    initialize: function(objectReference) {
        this.objectReference = objectReference;

        if (!this.requestFullscreenEnabled()) return;

        window.document.addEventListener('fullscreenchange', () =>
            this.objectReference.invokeMethodAsync('OnFullscreenChange', window.document.fullscreenElement ? true : false));
    },

    requestWakeLock: function() {
        if ('wakeLock' in window.navigator) window.navigator.wakeLock.request('screen').catch(() => {});
    },

    requestFullscreen: function() {
        if (this.requestFullscreenEnabled())
            window.document.documentElement.requestFullscreen().catch(() => {});
    },

    exitFullscreen: function() {
        if (window.document.exitFullscreen) window.document.exitFullscreen().catch(() => {});
    },

    requestFullscreenEnabled: function() {
        return window.document.documentElement?.requestFullscreen ? true : false;
    }
};