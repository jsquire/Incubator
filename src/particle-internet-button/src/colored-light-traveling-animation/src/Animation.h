#include "BetterPhotonButton.h"
#include "HsiColor.h"

#ifndef Animation_H
#define Animation_H

using namespace std::placeholders;

/**
* The direction that the animation is moving
*/
enum Direction
{
    Forward  = 0,
    Backward = 1
};

/**
* The animation to perform for the button.
*/
class Animation
{
private:
    int                 currentPixel;
    int                 trailPixel;
    HsiColor            color;
    Direction           hueDirection;
    BetterPhotonButton* button;

public:
    Direction direction;
    int       ticksPerMove;
    float     trailMaxIntensity;

    /**
    * Initializes a new intance of the Animation class.
    *
    * @param { BetterPhotonButton }  button            - The internet button to animate each tick
    * @param { Direction }           direction         - The direction that the animation should flow
    * @param { int }                 ticksPerMove      - The number of ticks that should take place before the animation moves the active LED
    * @param { float }               trailMaxIntensity - The maximum color intenstity (brightness) of the trailing LED in the animation
    */
    Animation(BetterPhotonButton* button,
              Direction           direction,
              int                 ticksPerMove      = 5,
              float               trailMaxIntensity = 0.15);

    /**
    * Performs a tick of the animation, equivilent to advancing a frame.  Note that no delay
    * will be applied.  Any timing adjustment is the purview of the caller.
    */
    void tick();

    /**
    * Reverses the direction of the animation, to be applied when next a tick is
    * performed.
    */
    void reverse();
};

#endif
