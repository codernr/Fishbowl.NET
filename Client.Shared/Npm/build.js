const fs = require('fs');
const sass = require('sass');
const autoprefixer = require('autoprefixer');
const postcss = require('postcss');
const { PurgeCSS } = require('purgecss');

const PATH = './../wwwroot/css/';
const DEFAULT_FILEPATH = PATH + 'styles.css';

const release = process.argv.includes('Release');

const regex = /^styles(?:\.?[a-z0-9]{32})?\.css$/;

fs.readdirSync(PATH)
    .filter(f => regex.test(f))
    .forEach(f => fs.unlinkSync(PATH + f));

const sassResult = sass.renderSync({
    file: 'styles.scss',
    includePaths: ['node_modules'],
    outputStyle: 'compressed',
    sourceMap: false,
    outFile: DEFAULT_FILEPATH
});

executeAsync().then(() => console.log('Build success'));

async function executeAsync()
{
    const postcssResult = await postcss([autoprefixer])
        .process(sassResult.css.toString(), { from: DEFAULT_FILEPATH, to: DEFAULT_FILEPATH, map: false });

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

    let outputPath = DEFAULT_FILEPATH;

    if (release) {
        const { createHash } = require('crypto');

        const hash = createHash('sha256');
        hash.update(purgeCssResult[0].css);
        const digest = hash.digest('hex');

        outputPath = outputPath.replace('.css', `.${digest.substring(0,32)}.css`);
    }

    fs.writeFileSync(outputPath, purgeCssResult[0].css);
}