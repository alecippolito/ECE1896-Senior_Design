#include <TimerOne.h>

int led = 8, button = 9;
int incoming = 0;
int delayTime = 500;

int out = 10;

void setup()
{
  pinMode(led, OUTPUT);
  pinMode(button, INPUT);

  // Setup transmission for high frequency PWM:
  pinMode(out,OUTPUT);
  Timer1.initialize(1);
  Timer1.pwm(out,512);
  Timer1.stop();

  Serial.begin(9600);
}

void loop()
{
  // Read from PC
  if(Serial.available() > 0)
  {
    incoming = Serial.read();
    //Serial.write(incoming);
    //blinkByte(incoming);
    pwmByte(incoming);
  }
  // Write to PC
  else if(digitalRead(button) == 1)
  {
    Serial.write("A");
    Serial.flush();
    delay(delayTime);
  }
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

void pwmByte(int byteRead)
{
  if(byteRead == 0)
  {
    return;
  }
  
  for(int i=0; i<8; i++)
  {
    if(bitRead(byteRead, i) == 1)
    {
      Timer1.setPeriod(1);
      Timer1.start();
      //delay(delayTime);
      Timer1.stop();
      //delay(delayTime);
    }
    else
    {
      Timer1.setPeriod(2);
      Timer1.start();
      //delay(delayTime);
      Timer1.stop();
      //delay(delayTime);
    }

    digitalWrite(led, HIGH);
    delay(delayTime / 2);
  }
}
