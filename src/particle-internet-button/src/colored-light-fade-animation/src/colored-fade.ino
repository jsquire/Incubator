#include <math.h>
#include "BetterPhotonButton.h"

// Constants

const int MIN_PIXEL = 0;
const int MAX_PIXEL = 10;
const int MIN_HUE   = 0;
const int MAX_HUE   = 360;

// Type Definitions

struct HsiColor
{
    float hue;
    float saturation;
    float intensity;

    PixelColor to_PixelColor()
    {
        int r;
        int g;
        int b;

        auto hue        = this->hue;
        auto saturation = this->saturation;
        auto intensity  = this->intensity;

        if (hue > 360)
        {
            hue -= 360;
        }

        // Cycle the hue around to 0-360 degrees.

        hue = fmod(hue, 360);

        // Convert to radians.

        hue = 3.14159 * hue / (float)180;

        // Clamp saturation and intensity to interval [0,1].

        saturation = saturation > 0 ? (saturation < 1 ? saturation : 1) : 0;
        intensity  = intensity  > 0 ? (intensity  < 1 ? intensity  : 1) : 0;

        // Perform the conversion.

        if (hue < 2.09439)
        {
            r = 255 * intensity / 3 * (1 + saturation * cos(hue) / cos(1.047196667 - hue));
            g = 255 * intensity / 3 * (1 + saturation * (1 - cos(hue) / cos(1.047196667 - hue)));
            b = 255 * intensity / 3 * (1 - saturation);
        }
        else if (hue < 4.188787)
        {
            hue -= 2.09439;
            g = 255 * intensity / 3 * (1 + saturation * cos(hue) / cos(1.047196667 - hue));
            b = 255 * intensity / 3 * (1 + saturation * (1 - cos(hue) / cos(1.047196667 - hue)));
            r = 255 * intensity / 3 * (1 - saturation);
        }
        else
        {
            hue -= 4.188787;
            b = 255 * intensity / 3 * (1 + saturation * cos(hue) / cos(1.047196667 - hue));
            r = 255 * intensity / 3 * (1 + saturation * (1 - cos(hue) / cos(1.047196667 - hue)));
            g = 255 * intensity / 3 * (1 - saturation);
        }

        return PixelColor { r, g, b };
    }
};

// Globals

auto active         = false;
auto currentLed     = 0;
auto ledColor       = HsiColor { 0, 1, 1 };
auto internetButton = BetterPhotonButton();

// Function signatures

void animate(HsiColor* color, int targetLed, int speed);
void buttonHandler(int button, bool pressed);

// Particle::setup runs once when the device is flashed or
// powers on.

void setup()
{
    internetButton.setup();
    internetButton.setPixels(0);
    internetButton.setPressedHandler(&buttonHandler);
    internetButton.update(millis());
}

// Particle::loop() runs over and over again, as quickly as it can execute.

void loop()
{
    if (active)
    {
        animate(&ledColor, currentLed, 15);
    }
    else
    {
        internetButton.setPixels(0);
    }

    internetButton.update(millis());
}

// Custom Functions

void animate(HsiColor* color, int targetLed, int speed)
{
    using namespace std::placeholders;

    static const std::function<int(int)> ops [] =
    {
        std::bind(std::plus<int>(),  _1, 1),
        std::bind(std::minus<int>(), _1, 1)
    };

    static auto opIndex = 0;

    auto currentHue = color->hue;

    if ((currentHue < MIN_HUE) || (currentHue > MAX_HUE))
    {
        currentHue = (currentHue > MAX_HUE) ? MAX_HUE : MIN_HUE;
        opIndex = abs(opIndex - 1);
    }

    color->hue = ops[opIndex](currentHue);
    internetButton.updatePixel(targetLed, color->to_PixelColor());
    delay(speed);
}

void buttonHandler(int button, bool pressed)
{
    switch (button)
    {
        case 1:
            internetButton.updatePixel(currentLed, 0);
            if (++currentLed > MAX_PIXEL) { currentLed = MAX_PIXEL; }
            break;

        case 3:
            internetButton.updatePixel(currentLed, 0);
            if (--currentLed < MIN_PIXEL) { currentLed = MIN_PIXEL; }
            break;

        default:
            active = (!active);
            break;
    }
}
