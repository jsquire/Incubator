#include <math.h>
#include "BetterPhotonButton.h"
#include "HsiColor.h"
#include "Display.h"

// Constants

const int TICKS_PER_MOVE = 10;

// Globals

auto active         = false;
auto loopCount      = 0;
auto ticksSinceMove = 0;
auto internetButton = BetterPhotonButton();
auto display        = new Display(&internetButton);

// Function signatures

void buttonHandler(int button, bool pressed);

/**
* This function runs once, when the device is flashed or powered-on.  It is intended
* to allow for initialization.
*/
void setup()
{
    internetButton.setup();
    internetButton.setPixels(0, 0, 0);
    internetButton.setReleasedHandler(&buttonHandler);

    Serial.begin();
}

// Particle::loop() runs over and over again, as quickly as it can execute.
/**
* This function runs continuously, as quickly as it can be executed.  This is the main loop where
* any program logic should live.
*/
void loop()
{
    if (active)
    {
        if (++ticksSinceMove == TICKS_PER_MOVE)
        {
            ticksSinceMove = 0;

            if (!display->tickLedAdvance())
            {
                display->reverseLedDirection();

                // Once we can't reduce, there's one more step that can be taken.   We probably don't want to do this for the actual game, but
                // I want this for testing.   For real, this is where we decide the winner.  Flash the loss animation then light up the winner.
                if (!display->reduceAvailableLeds())
                {
                    active = false;
                    display->tickLedAdvance();
                }

                printLedState(display->getLedState());;
            }
        }

        delay(15);
    }
    else if (loopCount < 2)
    {
        // Haven't been able to figure out why, but if we don't do this for at least two loops,
        // LED 1 wants to light up green before we do any animation.
        //
        // ¯\_(ツ)_/¯
        //
        internetButton.setPixels(0, 0, 0);
        ++loopCount;
    }

    internetButton.update(millis());
}

void printLedState(LedState state)
{
    Serial.printlnf("activeLed: %d", state.activeLed);
    Serial.printlnf("minAllowedLed: %d", state.minAllowedLed);
    Serial.printlnf("maxAllowedLed: %d", state.maxAllowedLed);
    Serial.printlnf("ledMidPoint: %d", state.ledMidPoint);
    Serial.printlnf("activeDirection: %d", state.activeDirection);
    Serial.println("");
    Serial.println("");
}

/**
* This function is responsible for interpreting the button presses captured by
* the device and taking the appopriate response action.
*
* @param { int }  button  - The index of the button that the event was captured for
* @param { bool } pressed - true if the button is currently in the "pressed" state; otherwise, false
*/
void buttonHandler(int  button,
                   bool pressed)
{
    switch (button)
    {
        case 0:
            Serial.println("Starting/Stopping.");
            active  = (!active);
            display = new Display(&internetButton);

            if (!active) { internetButton.setPixels(0, 0, 0); }
            break;

        case 1:
            display->reverseLedDirection();
            break;

        case 2:
            active = false;
            internetButton.setPixels(0, 0, 0);
            break;

        case 3:
            display->reverseLedDirection();
            break;




    }
}
