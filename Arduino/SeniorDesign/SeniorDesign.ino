#define TIMER_INTERRUPT_DEBUG     2
#define _TIMERINTERRUPT_LOGLEVEL_ 0

#define USE_TIMER_1 false
#define USE_TIMER_2 true

#include <TimerInterrupt.h>
#include <TimerOne.h>

int led = 8, receiverIn = A0, transmitterOut = 10;
uint8_t startByte = 1, endByte = 4, charDelay = 10;
int outputTest = 9;

int delayTime = 250;
bool delayEnable = false, startFlagOverride = true;

uint8_t incoming = 0, outgoing = 0, transmittedBits = 0, receivedBits = 0;
bool transmitFlag = false, receiveFlag = false, newTransmission = false, newReception = false, startFlag = false;
bool o = false;

// Hardware Timer 1 frequency values:
int highFreq = 41; // 24KHz square wave
//int highFreq = 20; // 50KHz square wave
int lowFreq = 62; // 16KHz square wave
//int lowFreq = 40; // 25KHz square wave
int standbyFreq = 100; // 10KHz square wave

// Hardware Timer 2 frequency value:
int interruptFreq = 10000;

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
  if (Serial.available() > 0 && (!newTransmission || transmittedBits == 0))
  {
    incoming = Serial.read();
    newTransmission = true;
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
  }//*/
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
  delayMicroseconds(charDelay);
  if(incoming == 0)
  {
    return;
  }

  //Serial.write(incoming);

  if(newTransmission)
  {
    transmittedBits = 0;
    if(startFlagOverride) receivedBits = 0;
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

  if (transmittedBits < 7)
  {
    newTransmission = false;
    transmittedBits++;
  }
  else
  {
    //Serial.write(incoming);
    transmittedBits = 0;
    incoming = 0;

    Timer1.setPeriod(standbyFreq);
    Timer1.setPwmDuty(transmitterOut, 512);

    digitalWrite(outputTest, false);
  }
}

void receiveBit()
{
  delayMicroseconds(charDelay);
  uint8_t votes = 5, carry;
  float holder = 0;

  for(int i=0; i<votes; i++)
  {
    if (analogRead(receiverIn) > 512)
    {
      holder += 1;
    }
    else
    {
      holder += 0;
    }
  }

  holder /= votes;

  if(startFlag || startFlagOverride)
  {
    if(holder > 0.5) 
    {
      outgoing |= 1 << receivedBits;
      carry = 1;
    }
    else
    {
      outgoing |= 0 << receivedBits;
      carry = 0;
    }
  }
  else
  {
    if(holder > 0.5)
    {
      outgoing <<= 1;
      outgoing += 1;
      carry = 1;
    }
    else
    {
      outgoing <<= 1;
      outgoing += 0;
      carry = 0;
    }
  }

  //if(outgoing !=0) Serial.write(carry);

  if(outgoing !=0 && !newReception) newReception = true;
  else if(outgoing == 0 && newReception) newReception = false;

  if(outgoing == startByte && !startFlag)
  {
    startFlag = true;
    receivedBits = 0;
    outgoing = 0;
  }
  else if(outgoing == endByte && startFlag)
  {
    startFlag = false;
  }

  if(startFlag || newReception)
  {
    if (receivedBits < 7) receivedBits++;
    else
    {
      if(outgoing != 0)
      {
        Serial.write(outgoing);
      }
  
      receivedBits = 0;
      outgoing = 0;
    } 
  }
}

void interruptHandler()
{
  transmitFlag = true;
  receiveFlag = true;
}
