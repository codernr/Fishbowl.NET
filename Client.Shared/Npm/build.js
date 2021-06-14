const fs = require('fs');
const sass = require('sass');
const autoprefixer = require('autoprefixer');
const postcss = require('postcss');
const { PurgeCSS } = require('purgecss');
const { createHash } = require('crypto');

const PATH = './../wwwroot/css/';
const FILE_PATH = `${PATH}styles.css`;
const HASH_LENGTH = 32;
const REGEXP = new RegExp(`styles.[a-z0-9]{${HASH_LENGTH}}.css$`, 'g');

fs.readdirSync(PATH)
    .filter(f => REGEXP.test(f))
    .forEach(f => fs.unlinkSync(PATH + f));

const sassResult = sass.renderSync({
    file: 'styles.scss',
    includePaths: ['node_modules'],
    outputStyle: 'compressed',
    sourceMap: false,
    outFile: FILE_PATH
});

executeAsync().then(() => console.log('Build success'));

async function executeAsync()
{
    const postcssResult = await postcss([autoprefixer])
        .process(sassResult.css.toString(), { from: FILE_PATH, to: FILE_PATH, map: false });

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

    const hash = createHash('sha256');
    hash.update(purgeCssResult[0].css);
    const digest = hash.digest('hex').toString().substring(0, HASH_LENGTH);

    fs.writeFileSync(`${PATH}styles.${digest}.css`, purgeCssResult[0].css);
}