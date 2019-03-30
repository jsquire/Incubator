const Playground    = artifacts.require('Playground');
const should        = require('chai').should();
const truffleAssert = require('truffle-assertions');

contract("Playground", function(accounts) {
    const ownerAccount = accounts[0];

    let playground;

    beforeEach(async () => {
        playground = await Playground.new({ from: ownerAccount });
    });

    afterEach(async () => {
        await playground.destroy({ from: ownerAccount });
    });

    it ("should be able to make a key", async function() {
        const keyName = "test-key";

        const result = await playground.makeKey(keyName);
        should.exist(result, "A result should have been returned");

        const name = result['0'];
        should.exist(name, "There should have been a tuple returned");
        name.should.equal(keyName, "The requested name should have been used as the basis for the key");

        const key = result['1'];
        should.exist(key, "There should have been a second member of the tuple returned");
        key.length.should.be.gte(32, "The key should be at least 32 characters long");
    });

    it ("should support stable key generation", async function() {
        const keyName  = "test-key";

        const results = await Promise.all([ playground.makeKey(keyName), playground.makeKey(keyName) ]);
        should.exist(results, "A set of results should have been returned");
        results.should.have.lengthOf(2, "There should have been two results returned.");

        const firstKey  = results[0]['1'];
        const secondKey = results[1]['1'];

        should.exist(firstKey, "There should have been a second member of the tuple returned");
        should.exist(secondKey, "There should have been a second member of the tuple returned");
        firstKey.should.equal(secondKey, "The same name should produce a stable key for each call");
    });

    it ("should support reading a contract location by key", async function() {
        const keyName = "test-key";

        const key = (await playground.makeKey(keyName))['1'];
        should.exist(key, "A key should have been returned");

        const location = await playground.getLocation(key);
        should.exist(location, "There was no location returned.  When not recorded, a default should be sent.");
        web3.utils.hexToNumber(location).should.equal(0, "There was no location recorded, the default should have been sent.");
    });

    it ("should support reading a contract location by name", async function() {
        const keyName = "test-key";

        const location = await playground.getLocation(keyName);
        should.exist(location, "There was no location returned.  When not recorded, a default should be sent.");
        web3.utils.hexToNumber(location).should.equal(0, "There was no location recorded, the default should have been sent.");
    });

    it ("should support recording a contract location", async function() {
        const keyName          = "test-key";
        const expectedLocation = accounts[1];

        await playground.recordLocation(keyName, expectedLocation);

        const recordedLocation = await playground.getLocation(keyName);
        should.exist(recordedLocation, "There was no location returned.  When not recorded, a default should be sent.");
        web3.utils.isAddress(recordedLocation).should.be.true;
        recordedLocation.should.equal(expectedLocation, "The location that was recorded should be retrievable");
    });

    it ("should support removing a location that was not recorded", async function() {
        const keyName = "blah";

        await playground.removeLocation(keyName);

        const recordedLocation = await playground.getLocation(keyName);
        should.exist(recordedLocation, "There was no location returned.  When not recorded, a default should be sent.");
        web3.utils.isAddress(recordedLocation).should.be.true;
        web3.utils.hexToNumber(recordedLocation).should.equal(0, "The default address should have been returned, as the key is unset.");
    });

    it ("should support removing a location that was previously recorded", async function() {
        const keyName = "some1key";

        await playground.recordLocation(keyName, accounts[1]);
        await playground.removeLocation(keyName);

        const recordedLocation = await playground.getLocation(keyName);
        should.exist(recordedLocation, "There was no location returned.  When removed, a default should be sent.");
        web3.utils.isAddress(recordedLocation).should.be.true;
        web3.utils.hexToNumber(recordedLocation).should.equal(0, "The default address should have been returned, as the key is unset.");
    });
});