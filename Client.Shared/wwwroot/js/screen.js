export const ScreenModule = {
    objectReference: undefined,

    matchMedia: window.matchMedia('(display-mode: standalone)'),

    initialize: function(objectReference) {
        this.objectReference = objectReference;

        if (!this.requestFullscreenEnabled()) return;

        window.document.addEventListener('fullscreenchange', () =>
            this.objectReference.invokeMethodAsync('OnFullscreenChange', window.document.fullscreenElement ? true : false));

        this.matchMedia.addEventListener('change', () =>
            this.objectReference.invokeMethodAsync('OnStandaloneChange', this.matchMedia.matches));
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
    },

    isStandalone: function() {
        return (window.navigator.standalone || this.matchMedia.matches) ? true : false;
    }
};