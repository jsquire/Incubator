pragma solidity ^0.5.2;

contract MagicNumber {

    // Events
    event MagicNumberChanged (
        uint64  value,
        address lastIncrementor
    );

    // Internal Types
    struct State {
        uint64  value;
        address lastIncrementor;
    }

    // Storage
    State           currentState;
    address payable owner;

    // Constructors and Destructors
    constructor() public {
        currentState = State(0, address(0x00));
        owner         = msg.sender;
    }

    function destroy() external {
        require(msg.sender == owner, "Only the owner can destroy the contract");
        selfdestruct(owner);
    }

    // Behavior
    function getCurrent() public view returns (uint64, address) {
        return (currentState.value, currentState.lastIncrementor);
    }

    function increment() public returns (uint64, address) {
        currentState.lastIncrementor = msg.sender;
        ++currentState.value;

        emit MagicNumberChanged(currentState.value, currentState.lastIncrementor);
        return (currentState.value, currentState.lastIncrementor);
    }

    function reset() public returns (uint64, address) {
        currentState = State(0, address(0x00));

        emit MagicNumberChanged(currentState.value, currentState.lastIncrementor);
        return (currentState.value, currentState.lastIncrementor);
    }
}
