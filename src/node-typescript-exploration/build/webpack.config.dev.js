"use strict";

const config = require("./webpack.config.base");

// Configure the Development-specific overrides to the WebPack baseline.

config.mode    = "development";
config.devtool = "inline-source-map";

module.exports = config;