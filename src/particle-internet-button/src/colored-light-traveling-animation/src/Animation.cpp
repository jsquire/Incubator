#include <math.h>
#include "BetterPhotonButton.h"
#include "HsiColor.h"
#include "Animation.h"

using namespace std::placeholders;

// Constants

const int MIN_PIXEL = 0;
const int MAX_PIXEL = 10;
const int MIN_HUE   = 0;
const int MAX_HUE   = 360;

static const std::function<int(int)> INCREMENT_OPS [] =
{
    std::bind(std::plus<int>(),  _1, 1),
    std::bind(std::minus<int>(), _1, 1)
};

// Local functions

/**
* Calculates the next value of the pixel position in the animation sequence, ensuring that
* the value is clamped to the legal range and wraps correctly.
*
* @param { int }        currentPixel - The current value of the pixel, before it is advanced
* @param { Direction }  direction    - The direction of the animation
*
* @returns { int } The value of the next pixel position in the animation sequence.
*/
int calculateNextPixel(int       currentPixel,
                       Direction direction)
{
    static int adjustedMax = (MAX_PIXEL + 1);

    auto value = fmod((INCREMENT_OPS[direction](currentPixel)), adjustedMax);
    return (value >= MIN_PIXEL) ? value : (adjustedMax + value);
}

// Class members

/**
* Initializes a new intance of the Animation class.
*
* @param { BetterPhotonButton }  button            - The internet button to animate each tick
* @param { Direction }           direction         - The direction that the animation should flow
* @param { int }                 ticksPerMove      - The number of ticks that should take place before the animation moves the active LED
* @param { float }               trailMaxIntensity - The maximum color intenstity (brightness) of the trailing LED in the animation
*/
Animation::Animation(BetterPhotonButton* button,
                     Direction           direction,
                     int                 ticksPerMove,
                     float               trailMaxIntensity)
{
    // Capture the supplied parameters (or defaults)

    this->button            = button;
    this->direction         = direction;
    this->ticksPerMove      = ticksPerMove;
    this->trailMaxIntensity = trailMaxIntensity;

    // Initialize the static animation state

    this->color        = HsiColor { 0, 1, 1 };
    this->hueDirection = Direction::Forward;

    // Set the starting LED positions based on the desired animation direction, intended
    // to smooth the initial startup to avoid jumping over the USB connector gap.

    if (direction == Direction::Forward)
    {
        this->currentPixel = MIN_PIXEL;
        this->trailPixel   = MAX_PIXEL;
    }
    else
    {
        this->currentPixel = MAX_PIXEL;
        this->trailPixel   = MIN_PIXEL;
    }
};

/**
* Performs a tick of the animation, equivilent to advancing a frame.  Note that no delay
* will be applied.  Any timing adjustment is the purview of the caller.
*/
void Animation::tick()
{
    static int ticksSinceMove = 0;

    auto currentHue   = this->color.hue;
    auto hueDirection = this->hueDirection;

    // Calculate the current color hue for the glow effect.  If the hue has hit the upper or
    // lower boundary, reset it and flip the direction of the glow effect.

    if ((currentHue < MIN_HUE) || (currentHue > MAX_HUE))
    {
        hueDirection = static_cast<Direction>(std::abs(hueDirection - 1));
        currentHue   = (currentHue > MAX_HUE) ? MAX_HUE : MIN_HUE;

        this->hueDirection = hueDirection;
    }

    this->color.hue = INCREMENT_OPS[hueDirection](currentHue);

    // If the requested number have dicks have passed since we last moveed, calculate and update
    // the LED positions for the circular animation.

    if (++ticksSinceMove >= this->ticksPerMove)
    {
        ticksSinceMove = 0;

        this->currentPixel = calculateNextPixel(this->currentPixel, this->direction);
        this->trailPixel   = calculateNextPixel(this->trailPixel, this->direction);

        // Since the LEDs are moving, reset the state so that all LEDs default to off.

        this->button->setPixels(0);
    }

    // Set the color values for the current and trailing LEDs.  This will appear to move the light around
    // the circle while also doing the color glow effect.  The trailing LED will be less bright than the current
    // one to simulate a motion trail.

    this->button->updatePixel(this->currentPixel, this->color.to_PixelColor());
    this->button->updatePixel(this->trailPixel, HsiColor { this->color.hue, this->color.saturation, this->trailMaxIntensity }.to_PixelColor());
};

/**
* Reverses the direction of the animation, to be applied when next a tick is
* performed.
*/
void Animation::reverse()
{
    this->trailPixel = calculateNextPixel(this->currentPixel, this->direction);
    this->direction  = static_cast<Direction>(std::abs(this->direction - 1));
};
