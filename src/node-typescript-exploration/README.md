# Node with Typescript Exploration #

### Summary ###

Begun in early 2018, this exploration was intended primarily to determine the feasibility of a two-stage transpilation process, allowing TypeScript to be transpiled to ECMA Script 6 and then using Babel to perform the ES6->ES5 transpilation.  At the time, there were some minor incompatibilities between the TypeScript and Babel results when reduced to ES5.

This project was also meant to allow me to familiarize myself with some of the tools in the Node ecosystem that I hadn't previously worked with and reacquaint myself with some that had undergone non-trivial changes since I had last worked with them.  Because the exploration was largely about understanding the tools, building out infrastructure, and scaffolding an actual project, there is little to no concept of an application.

The code is prototype-level, which is to say that there’s a bunch of best practices and polish missing for something that I’d consider to be production-worthy.  For example, there are no *real* unit tests; the infrastructure was put into place with some placeholder stubs to familiarize with the scaffolding and to verify the environment.

Despite the rough patches, I did try to comment extensively to narrate what was going on and why.

### Project Goals ###

- Build out scaffolding for a Node project, incorporating a modern tool set and organized into a lucid structure
- Develop with TypeScript and modern ECMA Script, ensuring consistency in the final transpiled ES5 output
- Allow for local development iteration (exec, watch, test) without an explicit build step, using on-the-fly transpiling
- Allow debugging via VS Code and Chrome Debug Tools
- Account for a formal build step (per environment) which does bundling, chunking, minification, etc.

### Structure ###

* **.vscode**
  <br />_The container for tasks and settings for the [Visual Studio Code](https://code.visualstudio.com/) editor.  These are portable and not specific to my particular environment._

* **build**
  <br />_The container for artifacts related to build activities, such as transpiling and bundling._

* **config**
  <br />_The container for application configuration artifacts for different environments._

* **src**
  <br />_The container for project source code._

* **.babelrc**
  <br />_The configuration and options for Babel transpilation._

* **.editorconfig**
  <br />_The standardized [editor configuration](https://editorconfig.org/) for defining and maintaining common conventions for project code across developers._

* **.gitignore**
  <br />_The local overrides for the default ignore configuration from the repository root._

* **.nycrc**
  <br />_The configuration for code test coverage inspection._

* **package.json**
  <br />_The standard Node project file, defining NPM scripts and package dependencies._

* **tsconfig.json**
  <br />_The configuration for the TypeScript compiler (as invoked by Babel)._

* **tslint.json**
  <br />_The configuration for TSLint, responsible for static analysis of the TypeScript and ECMA Script code._

### NPM Scripts ###

- Tasks named "name" and "name-name" are meant to be invoked directly.
- Tasks in the pattern of "name/name" are meant to be child tasks and are not intended to be directly invoked

### Transpiling ###

- Babel is the primary transpiler in charge of the pipeline; it has been configured to delegate to the TypeScript compiler as needed.

- The TypeScript compiler is responsible for transpiling TypeScript to ES2016 for Babel to consume; it is invoked as a child of Babel in the pipeline.

### Bundling ###

- Because Babel is handling TypeScript, there no need for a special TS loader for WebPack.  Just map the TypeScript source to Babel as you would any other script.

### Testing ###

- **Runner:**  Mocha
- **Assertions:**  Chai
- **Mocking:**  Sinon
- **Coverage:** Istanbul / NYC

### Environment Notes ###

- Development and testing on Windows took place using the standard command line environment as well as under the Windows Subsystem for Linux (WSL).  Testing also took place on Ubuntu 18.04; both were utilizing Node Version Manager (NVM) to managed multiple concurrent versions of Node.

- Because neither NVM or Node Version Switcher (NVS) on Windows can upgrade NPM via the normal method of running "npm I -g npm@latest", there needs to be a standard Node install at the system level as well as the one installed with NVM.

- NPM must be upgraded at the system level using the package `npm-windows-upgrade`;  The NVM/NVS instances use the system-level NPM, by coincidence, instead of the one bundled with them.

- The above configuration causes some pathing issues where NPM is not able to see the local path correctly for NPM scripts, making the behavior of resolving `node_modules` behave poorly.  It isn't possible to implicitly call a local script in `node_modules`, including with `npx` without using a path.

- With the above configuration, node modules called from NPM scripts on Windows must be fully qualified to properly resolve.  This, of course, leaves the scripts long and ugly.  The NPM scripts in this project attempt to make things a bit less cluttered by defining their own alias.

- Note that the NPM script referencing issue exists only on Windows when using NVM/NVS from the standard command line environment.  The scripts behave normally on Linux, and on Windows without NVM or under WSL.

- When debugging tests, breakpoints may not be honored without including a "debugger" call in the test; I believe this may have something to do with source maps for transpiling on the fly.

- When attaching to tests started with "--inspect-brk" you may need to restart the VS Code debugger;  it seems to get stuck in internal code.  Restarting hits the "debugger" call.