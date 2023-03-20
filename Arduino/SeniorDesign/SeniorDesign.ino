#define TIMER_INTERRUPT_DEBUG     2
#define _TIMERINTERRUPT_LOGLEVEL_ 0

#define USE_TIMER_1 false
#define USE_TIMER_2 true

#include <TimerInterrupt.h>
#include <TimerOne.h>

int led = 8, receiverIn = A0, transmitterOut = 10;
uint8_t startByte = 1, endByte;
int outputTest = 9;

int delayTime = 250;
bool delayEnable = false, startFlagOverride = false;

uint8_t incoming = 0, outgoing = 0, transmittedBits = 0, receivedBits = 0;
bool transmitFlag = false, receiveFlag = true, oFlag = false, startFlag = false;
bool o = false;

// Hardware Timer 1 frequency values:
int highFreq = 41; // 24KHz square wave
//int highFreq = 20; // 50KHz square wave
int lowFreq = 62; // 16KHz square wave
//int lowFreq = 40; // 25KHz square wave
int standbyFreq = 100; // 10KHz square wave

// Hardware Timer 2 frequency values:
int interruptFreq = 1;

void setup()
{
  pinMode(led, OUTPUT);
  pinMode(receiverIn, INPUT);
  pinMode(outputTest, OUTPUT);

  // Setup PWM pin based on 1MHz timer1 for transmitter:
  pinMode(transmitterOut, OUTPUT);
  Timer1.initialize(standbyFreq);
  Timer1.pwm(transmitterOut, 512);
  //Timer1.stop();

  // Setup Timer2 for transmit/receive operations:
  ITimer2.init();
  //ITimer2.attachInterrupt(interruptFreq, timerHandler);
  ITimer2.attachInterrupt(interruptFreq, interruptHandler);

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
    //pwmByte(incoming);
  }

  if (transmitFlag)
  {
    transmitBit();
    transmitFlag = false;
  }

  if (receiveFlag)
  {
    receiveBit();
    receiveFlag = false;
  }

  /*if(oFlag)
    {
    digitalWrite(led, o);
    o = !o;
    oFlag = false;
    }*/
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
      doDelay();
      digitalWrite(led, LOW);
      doDelay();
      digitalWrite(led, HIGH);
      doDelay();
      digitalWrite(led, LOW);
      doDelay();
    }
    else
    {
      digitalWrite(led, HIGH);
      doDelay();
      digitalWrite(led, LOW);
      doDelay();
    }
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
      Timer1.setPwmDuty(transmitterOut, 512);

      doDelay();
    }
    else
    {
      Timer1.setPeriod(lowFreq);
      Timer1.setPwmDuty(transmitterOut, 512);

      doDelay();
    }

    digitalWrite(led, HIGH);
    doDelay();
    digitalWrite(led, LOW);
    doDelay();
  }

  Timer1.setPeriod(standbyFreq);
  Timer1.setPwmDuty(transmitterOut, 512);
}

void doDelay()
{
  if (delayEnable)
  {
    delay(delayTime);
  }
}

void transmitBit()
{
  if (incoming == 0)
  {
    return;
  }

  if(incoming == startByte)
  {
    transmittedBits = 0;
  }

  if (bitRead(incoming, transmittedBits) == 1)
  {
    Timer1.setPeriod(highFreq);
    Timer1.setPwmDuty(transmitterOut, 512);

    digitalWrite(outputTest, true);

    doDelay();
  }
  else
  {
    Timer1.setPeriod(lowFreq);
    Timer1.setPwmDuty(transmitterOut, 512);

    digitalWrite(outputTest, false);

    doDelay();
  }

  if (delayEnable)
  {
    digitalWrite(led, HIGH);
    doDelay();
    digitalWrite(led, LOW);
    doDelay();
  }

  Serial.write(incoming);

  if (transmittedBits < 7)
  {
    transmittedBits++;
  }
  else
  {
    transmittedBits = 0;
    incoming = 0;

    Timer1.setPeriod(standbyFreq);
    Timer1.setPwmDuty(transmitterOut, 512);

    digitalWrite(outputTest, false);
  }
}

void receiveBit()
{
  uint8_t holder;

  if (analogRead(receiverIn) > 512)
  {
    holder = 1;
  }
  else
  {
    holder = 0;
  }

  Serial.write(holder);

  if(!startFlag || startFlagOverride)
  {
    outgoing <<= 1;
    outgoing += holder;
  }
  else
  {
    outgoing |= holder << receivedBits;
  }

  Serial.write(outgoing);

  if(startFlag)
  {
    if (receivedBits < 8) receivedBits++;
    else
    {
      if(outgoing != 0)
      {
        Serial.write(outgoing);
        Serial.flush();
      }
  
      receivedBits = 0;
      outgoing = 0;
    } 
  }

  if(outgoing == startByte && !startFlag)
  {
    startFlag = true;
    receivedBits = 0;
    outgoing = 0;
  }
}

void interruptHandler()
{
  transmitFlag = true;
  receiveFlag = true;
  oFlag = true;
}
