#define TIMER_INTERRUPT_DEBUG     2
#define _TIMERINTERRUPT_LOGLEVEL_ 0

#define USE_TIMER_1 false
#define USE_TIMER_2 true

#include <TimerInterrupt.h>
#include <TimerOne.h>

int led = 8, button = 9;
int delayTime = 500;

int in = A0;
int out = 10;
int interruptPin = 2;
uint8_t incoming = 0;
uint8_t receivedBits = 0;
     
bool o = false;

int highFreq = 20; // 50KHz square wave from timer 1
int lowFreq = 40; // 25KHz square wave from timer 1

int timer2Freq = 1000;

byte data = 0;

void setup()
{  
  pinMode(led, OUTPUT);
  pinMode(button, INPUT);

  pinMode(interruptPin, INPUT_PULLUP);
  attachInterrupt(digitalPinToInterrupt(interruptPin), interruptReceive, RISING); 

  pinMode(in, INPUT);
  
  // Setup PWM pin based on 1MHz timer1 for transmitter:
  pinMode(out, OUTPUT);
  Timer1.initialize(100000);
  Timer1.pwm(out, 512);
  Timer1.stop();

  // Setup Timer2 for transmit/receive operations:
  ITimer2.init();
  //ITimer2.attachInterrupt(timer2Freq, timerHandler);
  ITimer2.attachInterrupt(timer2Freq, interruptReceive);

  Serial.begin(9600);
}

void timerHandler()
{
  digitalWrite(led, o);
  o = !o;
}

void loop()
{
  // Read from PC
  if (Serial.available() > 0)
  {
    incoming = Serial.read();
    //Serial.write(incoming);
    //blinkByte(incoming);
    pwmByte(incoming);
  }
}

void blinkByte(uint8_t byteRead)
{
  if (byteRead == 0)
  {
    return;
  }

  for (int i = 0; i < 8; i++)
  {
    if (bitRead(byteRead, i) == 1)
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

void pwmByte(uint8_t byteRead)
{
  if (byteRead == 0)
  {
    return;
  }

  for (int i = 0; i < 8; i++)
  {
    if (bitRead(byteRead, i) == 1)
    {
      Timer1.setPeriod(highFreq);
      Timer1.setPwmDuty(out, 512);
      
      Timer1.start();
      delay(delayTime);
      Timer1.stop();
      delay(delayTime);
    }
    else
    {
      Timer1.setPeriod(lowFreq);
      Timer1.setPwmDuty(out, 512);

      Timer1.start();
      delay(delayTime);
      Timer1.stop();
      delay(delayTime);
    }

    digitalWrite(led, HIGH);
    delay(delayTime / 2);
    digitalWrite(led, LOW);
    delay(delayTime / 2);
  }
}

void interruptReceive()
{
  byte holder;
  
  if(analogRead(in) > 512)
  {
    //digitalWrite(led, HIGH);
    holder = 1;
  }
  else
  {
    //digitalWrite(led, LOW);
    holder = 0;
  }
  
  data |= holder << receivedBits; // need this in final
  
  if(receivedBits < 8)
  {
    receivedBits++;
  }
  else
  {
    receivedBits = 0;
    Serial.write(data);
    Serial.flush();
  }
}
