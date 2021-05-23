module.exports = {
    content: [
        './../wwwroot/index.html',
        './../Shared/**/*.razor',
        './../Pages/**/*.razor',
        './../Components/**/*.razor'
    ],
    css: ['./../wwwroot/css/styles.css'],
    safelist: {
        greedy: [/fade-container/, /bg-info/, /bg-warning/]
    },
    keyframes: true,
    output: './../wwwroot/css/'
}