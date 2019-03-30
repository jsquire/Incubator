const MagicNumber   = artifacts.require('MagicNumber');
const should        = require('chai').should();
const truffleAssert = require('truffle-assertions');

contract("MagicNumber", function(accounts) {
    const magicNumberChangeEventName = "MagicNumberChanged";
    const ownerAccount               = accounts[0];

    let magicNumber;

    beforeEach(async () => {
        magicNumber = await MagicNumber.new({ from: ownerAccount });
    });

    afterEach(async () => {
        await magicNumber.destroy({ from: ownerAccount });
    });

    it ("should have a default initial state", async function() {
        const result = await magicNumber.getCurrent();
        should.exist(result, "A result should have been returned");

        const value = result['0'];
        should.exist(value, "There should have been a tuple returned");
        value.isZero().should.be.true;

        const incrementor = result['1'];
        should.exist(incrementor, "There should have been a second member of the tuple returned");
        web3.utils.isAddress(incrementor).should.be.true;
        web3.utils.hexToNumber(incrementor).should.equal(0);
    });

    it ("should change state after a single increment", async function() {
        const expectedValue       = web3.utils.toBN(1);
        const expectedIncrementor = accounts[1];

        // Increment to modify the state.

        await magicNumber.increment({ from: expectedIncrementor });

        // Retrieve the current state and verify that it changed as expected.

        const result = await magicNumber.getCurrent({ from: expectedIncrementor });
        should.exist(result, "A result should have been returned");

        const value = result['0'];
        should.exist(value, "There should have been a tuple returned");
        value.eq(expectedValue).should.be.true;

        const incrementor = result['1'];
        should.exist(incrementor, "There should have been a second member of the tuple returned");
        web3.utils.isAddress(incrementor).should.be.true;
        incrementor.should.equal(expectedIncrementor);
    });

    it ("should change state after multiple increments", async function() {
        const increments          = 5;
        const expectedValue       = web3.utils.toBN(increments + 1);
        const expectedIncrementor = accounts[1];

        // Increment the specified number of times from the owner account to advance the state.

        for (let index = 0; index < increments; ++index) {
            await magicNumber.increment({ from: ownerAccount });
        }

        // Increment one final time from the expected account to ensure that the current state reflects
        // the last account that mutated it.

        await magicNumber.increment({ from: expectedIncrementor });

        // Retrieve the current state and verify that it changed as expected.

        const result = await magicNumber.getCurrent({ from: expectedIncrementor });
        should.exist(result, "A result should have been returned");

        const value = result['0'];
        should.exist(value, "There should have been a first member of the tuple returned");
        value.eq(expectedValue).should.be.true;

        const incrementor = result['1'];
        should.exist(incrementor, "There should have been a second member of the tuple returned");
        web3.utils.isAddress(incrementor).should.be.true;
        incrementor.should.equal(expectedIncrementor);
    });

    it ("should emit an event during a single increment", async function() {
        const expectedValue       =  web3.utils.toBN(1);
        const expectedIncrementor = accounts[1];

        // Increment one final time from the expected account to ensure that the current state reflects
        // the last account that mutated it.

        const transaction = await magicNumber.increment({ from: expectedIncrementor });
        should.exist(transaction, "A transaction should have been returned");

        truffleAssert.eventEmitted(transaction, magicNumberChangeEventName, event => {
            should.exist(event.value, "The event should have a value member");
            should.exist(event.lastIncrementor, "The event should have an lastIncrementor member");

            event.value.eq(expectedValue).should.be.true;
            web3.utils.isAddress(event.lastIncrementor).should.be.true;
            event.lastIncrementor.should.equal(expectedIncrementor);

            return true;
        });
    });

    it ("should emit an event during each increment", async function() {
        const increments          = 5;
        const expectedValue       = web3.utils.toBN(increments + 1);
        const expectedIncrementor = accounts[1];

        let transaction;

        // Increment the specified number of times from the owner account to advance the state.

        for (let index = 0; index < increments; ++index) {
            transaction = await magicNumber.increment({ from: ownerAccount });

            truffleAssert.eventEmitted(transaction, magicNumberChangeEventName, event => {
                should.exist(event.value, "The event should have a value member");
                should.exist(event.lastIncrementor, "The event should have an lastIncrementor member");

                event.value.eq(web3.utils.toBN(index + 1)).should.be.true;
                web3.utils.isAddress(event.lastIncrementor).should.be.true;
                event.lastIncrementor.should.equal(ownerAccount);

                return true;
            });
        }

        // Increment one final time from the expected account to ensure that the current state reflects
        // the last account that mutated it.

        transaction = await magicNumber.increment({ from: expectedIncrementor });

        truffleAssert.eventEmitted(transaction, magicNumberChangeEventName, event => {
            should.exist(event.value, "The event should have a value member");
            should.exist(event.lastIncrementor, "The event should have an lastIncrementor member");

            event.value.eq(expectedValue).should.be.true;
            web3.utils.isAddress(event.lastIncrementor).should.be.true;
            event.lastIncrementor.should.equal(expectedIncrementor);

            return true;
        });
    });
});