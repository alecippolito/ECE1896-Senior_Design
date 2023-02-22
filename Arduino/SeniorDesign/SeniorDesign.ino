#include <TimerOne.h>

int led = 8, button = 9;
int incoming = 0;
int delayTime = 500;

int up;
int down;
int out = 10;

void setup()
{
  pinMode(led, OUTPUT);
  pinMode(button, INPUT);

  // Setup transmission for high frequency PWM:
  pinMode(out,OUTPUT);
  Timer1.initialize(1);
  Timer1.pwm(out,512);

  Serial.begin(9600);
}

void loop()
{
  // Read from PC
  if(Serial.available() > 0)
  {
    incoming = Serial.read();
    //Serial.write(incoming);
    blinkByte(incoming);
  }
  // Write to PC
  else if(digitalRead(button) == 1)
  {
    Serial.write("A");
    Serial.flush();
    delay(delayTime);
  }

  //analogWrite(out,1);
  //analogWrite(out,0);
}

void blinkByte(int byteRead)
{
  if(byteRead == 0)
  {
    return;
  }
  
  for(int i=0; i<8; i++)
  {
    if(bitRead(byteRead, i) == 1)
    {
      digitalWrite(led, HIGH);
      delay(delayTime / 2);
      digitalWrite(led, LOW);
      delay(delayTime / 2);
      digitalWrite(led, HIGH);
      delay(delayTime / 2);
      digitalWrite(led, LOW);
      delay(delayTime / 2);
    }
    else
    {
      digitalWrite(led, HIGH);
      delay(delayTime);
      digitalWrite(led, LOW);
      delay(delayTime);
    }
    
    delay(delayTime / 2);
  }
}
