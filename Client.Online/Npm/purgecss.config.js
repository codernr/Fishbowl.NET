module.exports = {
    content: [
        './../wwwroot/index.html',
        './../Shared/**/*.razor',
        './../Pages/**/*.razor',
        './../Components/**/*.razor'
    ],
    css: ['./../wwwroot/css/styles.css'],
    safelist: {
        greedy: [/fade-container/, /bg-info/, /bg-warning/, /bg-danger/]
    },
    keyframes: true,
    output: './../wwwroot/css/'
}