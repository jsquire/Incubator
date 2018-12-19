"use strict";

const config         = require("./webpack.config.base");
const webpack        = require("webpack");
const UglifyJsPlugin = require("uglifyjs-webpack-plugin");

// Configure the Production-specific overrides to the WebPack baseline.

config.mode    = "production";
config.devtool = "source-map";

config.plugins.push(new webpack.optimize.AggressiveMergingPlugin());

config.optimization.minimizer.push(
    new UglifyJsPlugin({
        test : /\.(ts|js|jsx|tsx)$/,

        uglifyOptions : {
            parallel  : true,
            sourceMap : true,
            mangle    : true,
            output    : { beautify : false },

            compress : {
                passes       : 2,
                drop_console : true,
                dead_code    : true
            }
        }
    })
);


module.exports = config;