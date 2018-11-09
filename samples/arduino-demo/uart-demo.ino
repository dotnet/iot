// COMMANDS:
//   DIR - swap direction
//   SPIN - start/stop spinning
//   COL RRGGBB - set color to RRGGBB where RRGGBB is a hex number representing a color (i.e. COL FF0000 will set color to red)
//   RATE N - set rate of spinning (N is delay between "moving" the light), default is 100.
//            Low values might effectively have higher delay than expected (there is a delay related to reading from serial port)
//
// NOTE: incorrect numbers will get converted into 0 which will cause the light to turn off

#include <Adafruit_CircuitPlayground.h>
#include <errno.h>

#define NO_LIGHTS 10
#define serport Serial1

int dir = -1;
int color = 0x0000FF;
int spin_rate = 100;
bool spin = true;

int pixel1;
int pixel2;

// dir is +1 or -1
// adding number of lights so that modulo works correctly
// (negative numbers have different modulo rules in C++ than in math)
#define SPIN_PIXEL_ONCE(pix) (pix = (pix + dir + NO_LIGHTS) % NO_LIGHTS)

void setup() {
  CircuitPlayground.begin();
  serport.begin(9600);

  CircuitPlayground.setBrightness(255);
 
  // Can be any two pixels
  pixel1 = 0;
  pixel2 = 5;
}

// this should be called in a loop
// when there is data available on serial port it will read it and process, specifically
// when line of text is read it will call serial_on_line_read with a line as an argument
void serial_process() {
  static const int len = 101;
  static char serial_recv_buff[len];
  static char* curr = serial_recv_buff;
  static char* end = serial_recv_buff + len;
  while (serport.available() > 0) {
    char c = serport.read();
    if (c == '\n' || c == '\r') {
      if (curr != serial_recv_buff) {
        *curr = '\0';
        String line = serial_recv_buff;
        serial_on_line_read(line);
      }
      curr = serial_recv_buff;
    }
    else {
      *curr++ = c;
      if (curr == end) {
        // overflow detected...
        curr = serial_recv_buff;
      }
    }
  }
}

int hex_to_num(const String& hex) {
  char* end;
  int ret = strtol(hex.c_str(), &end, 16);
  if (errno != 0) {
    errno = 0;
    return 0;
  }
  return ret;
}

// called by serial_process when line is read over serial port
void serial_on_line_read(const String& line) {
  if (line == "DIR") {
    dir = -dir;
    serport.println("OK");
  }
  else if (line == "SPIN") {
    spin = !spin;
    serport.println("OK");
  }
  else if (line.startsWith("COL ")) {
    String hex = line.substring(4);
    color = hex_to_num(hex);
    serport.println("OK");
  }
  else if (line.startsWith("RATE ")) {
    String rate = line.substring(5);
    spin_rate = rate.toInt();
    serport.println("OK");
  }
  else {
    serport.println("ERROR");
  }
}

void loop() {
  serial_process();

  CircuitPlayground.clearPixels();
 
  CircuitPlayground.setPixelColor(pixel1, color);
  CircuitPlayground.setPixelColor(pixel2, color);

  if (spin) {
    SPIN_PIXEL_ONCE(pixel1);
    SPIN_PIXEL_ONCE(pixel2);
  }
 
  delay(spin_rate);
}
