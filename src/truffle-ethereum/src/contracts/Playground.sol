pragma solidity ^0.5.2;

contract Playground {
    // Events
    event ThingHappened (
        string  description
    );

    // Storage
    address payable owner;
    mapping (bytes32 => address) locations;

    // Constructors and Destructors
    constructor() public {
        owner = msg.sender;
    }

    function destroy() external {
        require(msg.sender == owner, "Only the owner can destroy the contract");
        selfdestruct(owner);
    }

    function makeKey(string memory source) public pure returns (string memory, bytes32) {
        return (source, keccak256(abi.encode(source)));
    }

    function getLocation(bytes32 key) public view returns (address) {
        return locations[key];
    }

    function getLocation(string memory key) public view returns (address) {
        bytes32 encodedKey;

        (, encodedKey) = this.makeKey(key);
        return getLocation(encodedKey);
    }

    function recordLocation(string memory key, address location) public {
        bytes32 encodedKey;

        (, encodedKey) = this.makeKey(key);
        locations[encodedKey] = location;

        // NOTE: This should likely fire an event detailing the key, encoded key, and stored address.
    }

    function removeLocation(string memory key) public {
        bytes32 encodedKey;

        (, encodedKey) = this.makeKey(key);
        delete locations[encodedKey];
    }
}
