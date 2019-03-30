var MagicNumber = artifacts.require("MagicNumber");
var Playground = artifacts.require("Playground");

module.exports = function(deployer) {
    deployer.deploy(MagicNumber);
    deployer.deploy(Playground);
}