#include <functional>
#include <math.h>
#include "BetterPhotonButton.h"
#include "HsiColor.h"
#include "Display.h"

using namespace std::placeholders;

// Constants

const std::function<int(int)> INCREMENT_OPS [] =
{
    std::bind(std::plus<int>(),  _1, 1),
    std::bind(std::minus<int>(), _1, 1)
};

// Local functions

/**
* Determines the color of a LED based on it's position in the arc and the number of available
* steps for it to move in the cirection that it is traveling.
*
* @param { LedState } ledState  - The current state of the LED
* @param { float }    safeHue   - The hue to be used for a purely safe location
* @param { float }    dangerHue - The hue to be used for a location that is nearly invalid
*
* @returns { HsiColor } The color that the LED should be set to for its current state
*/
HsiColor calculateLedColor(LedState ledState,
                           float    safeHue,
                           float    dangerHue)
{
    static const std::function<int(int, int)> add      = std::minus<int>();
    static const std::function<int(int, int)> subtract = std::plus<int>();

    // If the current LED is right at the midpoint, it's definitively in the safe zone.

    if (ledState.activeLed == ledState.ledMidPoint)
    {
        return HsiColor { safeHue, 1, 1 };
    }

    // Calculate how much the hue should change by determining how many available LED positions
    // are in the hemisphere that the active Led is in.

    int                          ledRange;
    std::function<int(int, int)> operation;

    if (ledState.activeLed < ledState.ledMidPoint)
    {
        ledRange  = std::abs(ledState.ledMidPoint - ledState.minAllowedLed);
        operation = (ledState.activeDirection == Direction::Forward) ? subtract : add;
    }
    else
    {
        ledRange  = std::abs(ledState.maxAllowedLed - ledState.ledMidPoint);
        operation = (ledState.activeDirection == Direction::Forward) ? add : subtract;
    }

    auto hueDelta = ceil(fabs(safeHue - dangerHue) / ledRange);
    return HsiColor { operation(ledState.activeColor.hue, hueDelta), 1, 1 };
}

// Class members

/**
* Initializes a new intance of the Display class.
*
* @param { BetterPhotonButton }  button            - The internet button to animate each tick
* @paam  { int }                 minimumAllowedLed - The index of the minimum available LED for animation; defaults to MIN LED
* @param { int }                 maximumAllowedLed - The index of the maximum available LED for animation; defaults to MAX LED
* @param { float }               safeHue           - The color hue to use when indicating that the LED is in a safe position; defaults to green
* @param { float }               dangerHue         - The color hue to use when indicating that the LED is in a dangerous position; defaults to red
* @param { float }               unavailableHue    - The color hue to use when indicating that a LED position is unavailable; defaults to red
*/
Display::Display(BetterPhotonButton* button,
                 int                 minimumAllowedLed,
                 int                 maximumAllowedLed,
                 float               safeHue,
                 float               dangerHue,
                 float               unavailbleHue)
{
    auto midPoint = ((maximumAllowedLed - minimumAllowedLed) / 2);

    this->button           = button;
    this->unavailableColor = HsiColor { unavailbleHue, 1, 1 };
    this->safeHue          = safeHue;
    this->dangerHue        = dangerHue;

    this->ledState = LedState
    {
        midPoint,
        minimumAllowedLed,
        maximumAllowedLed,
        midPoint,
        Direction::Forward,
        HsiColor { safeHue, 1 , 1 }
    };
}

/**
* Performs a tick of the LED animation, equivilent to advancing a frame.  Note that no delay
* will be applied.  Any timing adjustment is the purview of the caller.
*
* @returns { bool } true if the advance was successful; otherwise, false if the minimum/maximum allowed LED was violated
*/
bool Display::tickLedAdvance()
{
    auto state            = this->ledState;
    auto button           = this->button;
    auto unavailableColor = this->unavailableColor.toPixelColor();

    // If advancing the LED would violate a minimum or maximum constraint, take no action and
    // and signal failure;

    if (((state.activeLed <= state.minAllowedLed) && (state.activeDirection == Direction::Backward)) ||
        ((state.activeLed >= state.maxAllowedLed) && (state.activeDirection == Direction::Forward)))
    {
        Serial.printlnf("Cannot advance.  Active: %d || Min: %d || Max: %d || Direction: %d", state.activeLed, state.minAllowedLed, state.maxAllowedLed, state.activeDirection);
        return false;
    }

    // Advance the LED, determine the color, and capture changes to the state.

    state.activeLed   = INCREMENT_OPS[state.activeDirection](state.activeLed);
    state.activeColor = calculateLedColor(state, this->safeHue, this->dangerHue);

    this->ledState = state;

    // Repaint the LEDs.  If there are any unavailable LEDs, color them as such.

    button->setPixels(0, 0, 0);
    button->setPixel(state.activeLed, state.activeColor.toPixelColor());

    for (auto index = MIN_LED; index < state.minAllowedLed; ++index)
    {
        button->setPixel(index, unavailableColor);
    }

    for (auto index = MAX_LED; index > state.maxAllowedLed; --index)
    {
        button->setPixel(index, unavailableColor);
    }

    return true;
}

/**
* Reverses the direction of the animation, to be applied when next a tick is
* performed.
*/
void Display::reverseLedDirection()
{
    this->ledState.activeDirection = static_cast<Direction>(std::abs(this->ledState.activeDirection - 1));
};

/**
* Allows the current LED state to be retrieved.
*
* @returns { LedState } The current state of the LED display
*/
LedState Display::getLedState()
{
    return this->ledState;
}

/**
* Reduces the available range of LEDs legal for animation by one unit.
*
* @param { LedSide } side - Indicates the side of the range to reduce; defaults to Neither, which determines the side based on the current animation direction
*
* @return { bool } true if there were LEDs that could be reduced; otherwise, false.
*/
bool Display::reduceAvailableLeds(LedSide side)
{
    // If the side chosen was "Neither" then attempt to calculate the side.

    if (side == LedSide::Neither)
    {
        side = determineLedSide(this->ledState);
    }

    // If the side was still "Neither" after calculation, then the LED is currently at the midPoint.  Tweak the side by using the
    // direction that the LED is moving.  There may be nowhere to go or we may need to update the last allowed LED.

    if (side == LedSide::Neither)
    {
        side = (this->ledState.activeDirection == Direction::Forward) ? LedSide::Maximum : LedSide::Minimum;
    }

    if ((side == LedSide::Minimum) && ((this->ledState.minAllowedLed + 1) < this->ledState.ledMidPoint))
    {
      ++(this->ledState.minAllowedLed);
      return true;
    }

    if ((side == LedSide::Maximum) && ((this->ledState.maxAllowedLed - 1) > this->ledState.ledMidPoint))
    {
        --(this->ledState.maxAllowedLed);
        return true;
    }

    Serial.printlnf("Cannot reduce.  Side: %d || Min: %d || Max: %d || Mid: %d", side, this->ledState.minAllowedLed, this->ledState.maxAllowedLed, this->ledState.ledMidPoint);
    return false;
}

/**
* Determines which side of the device the active LED is on.
*
* @param { LedState } ledState - The current state of the LED
*
* @returns { LedSide } The side that the LED is currently on
*/
LedSide Display::determineLedSide(LedState ledState)
{
    return (ledState.activeLed == ledState.ledMidPoint)
        ?  LedSide::Neither
        :  (ledState.activeLed < ledState.ledMidPoint) ? LedSide::Minimum : LedSide::Maximum;
}
