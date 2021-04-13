export const storageModule = {
    setItem: function (key, value) {
        window.localStorage.setItem(key, value);
    },
    getItem: function (key) {
        return window.localStorage.getItem(key);
    }
}