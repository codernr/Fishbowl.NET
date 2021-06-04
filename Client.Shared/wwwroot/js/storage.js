export const StorageModule = {
    setItem: function (key, value) {
        window.localStorage.setItem(key, value);
    },
    removeItem: function(key) {
        window.localStorage.removeItem(key);
    },
    getItem: function (key) {
        return window.localStorage.getItem(key);
    }
}