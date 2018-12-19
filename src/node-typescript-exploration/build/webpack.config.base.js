"use strict";

const path                        = require("path");
const webpack                     = require("webpack");
const Stylish                     = require('webpack-stylish');
const FriendlyErrorsWebpackPlugin = require("friendly-errors-webpack-plugin");

const { buildBundles } = require("./webpack-bundler");

const bundleDirectoryDepth = 1;
const testPathExpression   = /tests/;
const sourceFilePattern    = "/*.{js,ts}";
const testFilePattern      = "/*.spec.{js,ts}";
const outputPath           = path.resolve(__dirname, "../dist");
const sourceRootPath       = path.resolve(__dirname, "../src");
const testRootPath         = path.resolve(__dirname, "../src/tests");

const sourceBundleExcludePaths = [
    testRootPath,
    path.resolve(sourceRootPath, "shared")
];

const testBundleExcludePaths = [
    // No test subdirectories are currently being excluded
];

// Dyamically build the entry point mapping for bundles based on the directory
// structures.

const bundles = buildBundles(sourceRootPath,
                             sourceFilePattern,
                             sourceBundleExcludePaths,
                             testRootPath,
                             testFilePattern,
                             testBundleExcludePaths,
                             bundleDirectoryDepth);

// Export the baseline WebPack configuration

module.exports = {
    mode    : "none",
    stats   : "minimal",
    target  : "node",
    context : sourceRootPath,
    entry   : bundles,

    output : {
        filename : "[name].js",
        path     : outputPath
    },

    resolve : {
        extensions : [".ts", ".tsx", ".js", ".jsx"]
    },

    module : {
        rules : [
            {
                test    : /\.(ts|js|jsx|tsx)$/,
                exclude : [ /node_modules/ ],

                include : [
                    sourceRootPath,
                    testPathExpression
                ],

                use : [
                    {
                        loader  : "babel-loader",
                        options : {
                            cacheDirectory : false,
                            babelrc        : true
                        }
                    }
                ]
            }
        ]
    },

    optimization : {
        minimizer : []
    },

    plugins : [
        new Stylish(),
        new FriendlyErrorsWebpackPlugin(),
        new webpack.ProgressPlugin()
    ]
};