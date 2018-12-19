import nconf from "nconf";
import { argv } from "yargs";

/**
 * Specifies the contract to be implemented by suppliers of application configuration.
 * @interface IConfiguration
 */
export interface IConfiguration {
    readonly environment : string;
    readonly serverPort  : (number | null);
}

/**
 * Determines the current runtime environment for the application.
 *
 * @returns {string} The current runtime environment name
 */
function determineEnvironment() : string {
    return (argv.NODE_ENV || argv.environment || process.env.APP_ENV || process.env.NODE_ENV || "local");
}

/**
 * Performs the actions needed to build configuration for the application usung a hierarchical approach
 * with the following priorities:
 *     - Command line arguments
 *     - Environment variables
 *     - Configuration file
 *     - Default values
 *
 * @param {((string | null | undefined))} configurationFilePath : If provided, the configuration file to be used for values
 *
 * @returns {IConfiguration} The configuration for the application
 */
function buildConfiguration(configurationFilePath : (string | null | undefined)) : IConfiguration {
    nconf
        .argv({ parseValues : true })
        .env({ parseValues : true });

    if ((configurationFilePath) && (typeof(configurationFilePath) === "string") && (configurationFilePath.length > 0)) {
        nconf.file(configurationFilePath);
    }

    nconf.defaults({
        serverPort : 3000
    });

    // Build the configuration to return.

    return {
        environment : determineEnvironment(),
        serverPort  : nconf.any(["PORT", "serverPort"])
    };
}

// Exports

export default buildConfiguration;

export {
    buildConfiguration,
    determineEnvironment
};