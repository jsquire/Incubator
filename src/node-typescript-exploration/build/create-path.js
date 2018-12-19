'use strict';

const fs   = require('fs');
const path = require('path');

let target  = path.normalize(...process.argv.slice(2) || '../dist');
let current = path.resolve(__dirname);

for (const segment of target.split(path.sep)) {
    current = path.resolve(current, segment);

    if (!fs.existsSync(current)) {
        fs.mkdirSync(current);
    }
}