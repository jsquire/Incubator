'use strict';

const fs     = require('fs');
const path   = require('path');
const rimraf = require('rimraf');

const target = path.resolve(__dirname, path.normalize(...process.argv.slice(2) || '../dist'));

if (fs.existsSync(target)) {
    rimraf.sync(target, fs, err => console.log(`Unable to clean the build directory [${target}] due to ${err.message}`))
}
