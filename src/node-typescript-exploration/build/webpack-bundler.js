"use strict";

const fs   = require("fs");
const path = require("path");
const glob = require("glob");

const isDirectory   = scanTarget => fs.lstatSync(scanTarget.path).isDirectory();
const isNotHidden   = scanTarget => (!scanTarget.name.startsWith("."));
const isNotExcluded = scanTarget => (!scanTarget.exclude.includes(scanTarget.path));

const makeChildName = (parentItem, childName) =>
    ((parentItem.name == null) || (parentItem.name === "")) ? childName : `${parentItem.name}.${childName}`;

const createChildTarget = (childName, parentTarget) => {
   return {
        name          : makeChildName(parentTarget, childName),
        path          : path.resolve(parentTarget.path, childName),
        exclude       : parentTarget.exclude,
        glob          : parentTarget.glob,
        depth         : (parentTarget.depth + 1),
        getBundleName : parentTarget.getBundleName
    };
};

const scanDirectories = scanTarget => fs.readdirSync(scanTarget.path)
    .map(childName => createChildTarget(childName, scanTarget))
    .filter(child => ((isDirectory(child)) && (isNotHidden(child)) && (isNotExcluded(child))));

const populateFiles = (scanTarget, recursive) =>
    glob.sync(`${ scanTarget.path }${ (recursive ? "/**" : "") }${ scanTarget.glob }`);

const stringValueOrDefault = (string, defaultValue) =>
    ((typeof(string) !== 'undefined') && (string !== null) && (string.length > 0)) ? string : defaultValue;

/**
 * Performs the tasks needed to build the WebPack bundle mappings used to populate
 * the "entry" configuration property for WebPack's configuration.
 *
 * The bundle set is prepared by inspecting the source and test roots.  The files directly under
 * the root will be considered the main entry point.  For each subdirectory directly under the root,
 * up to the maximum scan depth, will be considered a unique bundle.  Once the maximum scan depth
 * has been reached, all files from child subdirectories will be included in the parent bundle closest to
 * them.
 *
 * Bundles will be named:
 *     "main"                : Any files found directly in the source root
 *     {dirName}             : Where {dirName} is the name of a child of the source root
 *     {parent.dirName}      : Where {parent} is the name of the parent bundle and {dirName} is the name of the current child directory
 *     "main.spec"           : Any files found directly in the test root
 *     {dirName}.spec        : Where {dirName} is the name of a child of the test root
 *     {parent.dirName.spec} : Where {parent} is the name of the parent bundle and {dirName} is the name of the current child directory
 *
 * @param {string}   sourceRootPath           : The fully qualified path to the source root directory
 * @param {string}   sourceFilePattern        : The glob pattern that identifies source files to bundle
 * @param {string[]} sourceBundleExcludePaths : An array of fully qualified paths to exclude when discovering source bundles
 * @param {string}   testRootPath             : The fully qualified path to the test root directory
 * @param {string}   testFilePattern          : The glob pattern that identifies test files to bundle
 * @param {string[]} testBundleExcludePaths   : An array of fully qualified paths to exclude when discovering test bundles
 * @param {int}      maximumLevels            : The maximum number of depth levels to traverse in the directory structure before including recursive files
 *
 * @returns {object} An object that can be used as the "entry" mapping in the WebPack configuration.
 */
function buildBundles(sourceRootPath,
                      sourceFilePattern,
                      sourceBundleExcludePaths,
                      testRootPath,
                      testFilePattern,
                      testBundleExcludePaths,
                      maximumLevels)
{
    const maxDepth = (maximumLevels || 0);

    let scanTargets = [
        {
            name            : "",
            path            : sourceRootPath,
            glob            : sourceFilePattern,
            exclude         : sourceBundleExcludePaths,
            depth           : 0,
            getBundleName() { return stringValueOrDefault(this.name, "main"); }
        },
        {
            name            : "",
            path            : testRootPath,
            glob            : testFilePattern,
            exclude         : testBundleExcludePaths,
            depth           : 0,
            getBundleName() { return `${ stringValueOrDefault(this.name, "main") }.spec`; }
        }
    ];

    let bundles         = {};
    let maxDepthReached = false;
    let currentTarget   = null;

    while (scanTargets.length > 0)
    {
        currentTarget   = scanTargets.pop();
        maxDepthReached = (currentTarget.depth >= maxDepth)

        // Populate the files for the current target.  If the maximum scan depth
        // has been reached, then recursively include all child files for the
        // target.

        currentTarget.files = populateFiles(currentTarget, maxDepthReached);

        // If the maximum scan depth hasn't been reached, scan the target's children
        // and include them as targets.

        if (!maxDepthReached) {
            scanTargets = [...scanTargets, ...scanDirectories(currentTarget)];
        }

        // If there were files for the current target, include it as part of the bundles; otherwise,
        // ignore it.  WebPack does not take kindly to empty bundles.

        if (currentTarget.files.length > 0) {
            bundles[currentTarget.getBundleName(currentTarget)] = currentTarget.files;
        }
    }

    return bundles;
};

// Specify the exported members.

module.exports = {
    buildBundles
};

