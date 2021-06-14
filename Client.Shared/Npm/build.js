const fs = require('fs');
const sass = require('sass');
const autoprefixer = require('autoprefixer');
const postcss = require('postcss');
const { PurgeCSS } = require('purgecss');

const PATH = './../wwwroot/css/styles.css';

const release = process.argv.includes('Release');

if (fs.existsSync(PATH)) fs.unlinkSync(PATH);

const sassResult = sass.renderSync({
    file: 'styles.scss',
    includePaths: ['node_modules'],
    outputStyle: 'compressed',
    sourceMap: false,
    outFile: PATH
});

executeAsync().then(() => console.log('Build success'));

async function executeAsync()
{
    const postcssResult = await postcss([autoprefixer])
        .process(sassResult.css.toString(), { from: PATH, to: PATH, map: false });

    const purgeCssResult = await new PurgeCSS().purge({
        content: [
            './../../Client.Online/wwwroot/index.html',
            './../../Client.Pwa/wwwroot/index.html',
            './../../Client.Shared/**/*.razor',
            './../../Client.Online/**/*.razor',
            './../../Client.Pwa/**/*.razor',
        ],
        safelist: {
            greedy: [/fade-container/, /bg-info/, /bg-warning/, /bg-danger/]
        },
        keyframes: true,
        output: './../wwwroot/css/',
        css: [
            { raw: postcssResult.css }
        ]
    });

    fs.writeFileSync(PATH, purgeCssResult[0].css);
}