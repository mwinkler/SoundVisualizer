
#include <Adafruit_NeoPixel.h>

const uint16_t PixelCount = 16; // make sure to set this to the number of pixels in your strip
const uint16_t PixelPin = 2;  // make sure to set this to the correct pin, ignored for Esp8266

Adafruit_NeoPixel strip = Adafruit_NeoPixel(PixelCount, PixelPin, NEO_GRBW + NEO_KHZ800);

byte buffer[16];
byte index;

byte color_r = 0;
byte color_g = 0;
byte color_b = 0;
byte color_w = 15;

void setup()
{
	strip.begin();
	strip.show();

	//strip.setBrightness(255);

	Serial.begin(115200);
}

void loop()
{
	

	if (Serial.available()) {
		index = 0;
		while (Serial.available()) {
			buffer[index++] = Serial.read();
		}

		//Serial.write(buffer, index);
		
		if (index > 3) {
			color_r = buffer[0];
			color_g = buffer[1];
			color_b = buffer[2];
			color_w = buffer[3];

			for (size_t i = 0; i < PixelCount; i++)
			{
				strip.setPixelColor(i, color_r, color_g, color_b, color_w);
			}

			strip.show();
		}
	}

	delay(20);
}
