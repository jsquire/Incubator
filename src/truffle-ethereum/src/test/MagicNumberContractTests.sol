pragma solidity ^0.5.2;

import "truffle/Assert.sol";
import "truffle/DeployedAddresses.sol";
import "../contracts/MagicNumber.sol";

contract MagicNumberContractTests {
    MagicNumber magicNumber   = MagicNumber(DeployedAddresses.MagicNumber());
    address     callerAddress = address(this);

    function testInitialStateIsDefaulted() public {
        (uint value, address incrementor) = magicNumber.getCurrent();

        Assert.equal(value, uint(0), "The initial value should be 0");
        Assert.equal(incrementor, address(0x00), "The initial address should be empty");
    }

    function testSingleIncrementChangesState() public {
        magicNumber.increment();

        (uint value, address incrementor) = magicNumber.getCurrent();

        Assert.equal(value, uint(1), "The value of a single increment should be 1");
        Assert.equal(incrementor,callerAddress, "The address should reflect the caller");
    }

    function testResetStateIsDefaulted() public {
        magicNumber.increment();

        (uint value, address incrementor) = magicNumber.reset();

        Assert.equal(value, uint(0), "The reset value should be 0");
        Assert.equal(incrementor, address(0x00), "The reset address should be empty");
    }

    function testMultipleIncrementsChangeState() public {
        uint8 incrementCalls = 15;

        magicNumber.reset();

        for (uint8 index = 0; index < incrementCalls; ++index) {
            magicNumber.increment();
        }

        (uint value, address incrementor) = magicNumber.getCurrent();

        Assert.equal(value, incrementCalls, "The value should match the number of increment calls.");
        Assert.equal(incrementor,callerAddress, "The address should reflect the caller");
    }
}