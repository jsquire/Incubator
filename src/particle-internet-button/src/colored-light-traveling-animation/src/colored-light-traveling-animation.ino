#include <math.h>
#include "BetterPhotonButton.h"
#include "HsiColor.h"
#include "Animation.h"

// Globals

auto active         = false;
auto internetButton = BetterPhotonButton();
auto animation      = new Animation(&internetButton, Direction::Backward);

// Function signatures

void buttonHandler(int button, bool pressed);

/**
* This function runs once, when the device is flashed or powered-on.  It is intended
* to allow for initialization.
*/
void setup()
{
    internetButton.setup();
    internetButton.setPixels(0);
    internetButton.setPressedHandler(&buttonHandler);
    internetButton.update(millis());
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
        animation->tick();
        delay(15);
    }
    else
    {
        internetButton.setPixels(0);
    }

    internetButton.update(millis());
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
        case 1:
        case 3:
            animation->reverse();
            break;

        default:
            active = (!active);
            break;
    }
}
