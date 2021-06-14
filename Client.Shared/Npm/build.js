const hashFiles = require('hash-files');
const fs = require('fs');

const PATH = './../wwwroot/css/styles.css';

hashFiles({
    files: [PATH],
    algorithm: 'sha256',
    noGlob: true
}, (error, hash) => {
    if (error) throw error;

    fs.renameSync(PATH, `./../wwwroot/css/styles.${hash.substring(0, 32)}.css`);
})