module.exports = {
    content: [
        './../../Client.Online/wwwroot/index.html',
        './../../Client.Pwa/wwwroot/index.html',
        './../../Client.Shared/**/*.razor',
        './../../Client.Online/**/*.razor',
        './../../Client.Pwa/**/*.razor',
    ],
    css: ['./../wwwroot/css/styles.css'],
    safelist: {
        greedy: [/fade-container/, /bg-info/, /bg-warning/, /bg-danger/]
    },
    keyframes: true,
    output: './../wwwroot/css/'
}