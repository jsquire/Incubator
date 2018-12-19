import debug from "debug";
import express from "express";

import { buildConfiguration, determineEnvironment } from "core/config";
import { resolve } from "path";

// Environment and Configuration

const environment    = determineEnvironment();
const configFilePath = resolve(__dirname, `../../config/settings.${ environment }.json`);
const config         = buildConfiguration(configFilePath);
const log            = debug("app:api:server");

// Create the server and routes
const app = express();

log(config);
console.log("");

console.log("ENV:" + process.env.NODE_ENV);
console.log("ENV:" + process.env.DEBUG);
console.log("ENV:" + process.argv);
console.log("");

console.log(configFilePath);
console.log("Done");