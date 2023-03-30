#define TIMER_INTERRUPT_DEBUG     2
#define _TIMERINTERRUPT_LOGLEVEL_ 0

#define USE_TIMER_1 false
#define USE_TIMER_2 true

#include <TimerInterrupt.h>
#include <TimerOne.h>

int receiverIn = A0, transmitterOut = 10;
uint8_t charDelay = 20, comma = 1;
uint8_t startMessage = 1, endMessage = 4, startChunk = 2, endChunk = 3;
int outputTest = 9, led = 8, errorChance = 100;

int delayTime = 250;
bool delayEnable = false;

uint8_t incoming = 0, outgoing = 0, transmittedBits = 0, receivedBits = 0;
bool transmitFlag = false, receiveFlag = false, newTransmission = false, newReception = false;
bool o = false;

// Hardware Timer 1 frequency values:
int highFreq = 41; // 24KHz square wave
//int highFreq = 20; // 50KHz square wave
int lowFreq = 62; // 16KHz square wave
//int lowFreq = 40; // 25KHz square wave

// Hardware Timer 2 frequency value:
int interruptFreq = 20000;

void setup()
{
  pinMode(led, OUTPUT);
  pinMode(receiverIn, INPUT);
  pinMode(outputTest, OUTPUT);

  // Setup PWM pin based on 1MHz timer1 for transmitter:
  pinMode(transmitterOut, OUTPUT);
  Timer1.initialize(lowFreq);
  Timer1.pwm(transmitterOut, 512);

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
  if (Serial.available() > 0 && !newTransmission && transmittedBits == 0)
  {
    incoming = Serial.read();
    newTransmission = true;
  }

  if (transmitFlag)
  {
    //transmitBitExtraBit();
    transmitBitStartStop();
    transmitFlag = false;
  }

  if (receiveFlag)
  {
    //receiveBitExtraBit();
    receiveBitStartStop();
    receiveFlag = false;
  }//*/
}

/*/  
 *  Breaks down incoming serial bytes and transmits bits one at a time by sending them as 24kHz square wave for 1, 16kHz square wave for 0.
 *  To differentiate bytes on the receiver end, adds one extra bit to the front of every byte.
 *  This increases the number of bits required to transmit messages by 1.125, which is not ideal.
/*/
void transmitBitExtraBit()
{
  delayMicroseconds(charDelay);
  if(incoming == 0)
  {
    return;
  }

  if(newTransmission)
  {
    transmittedBits = 0;
  }

  if(newTransmission || transmittedBits < 1)
  {
    Timer1.setPeriod(highFreq);
    Timer1.setPwmDuty(transmitterOut, 512);

    digitalWrite(outputTest, true);

    doDelay();
  }
  else if (bitRead(incoming, transmittedBits-1) == 1)
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

  /*if(digitalRead(outputTest) == true) Serial.write(1);
  else Serial.write(0);//*/

  if (delayEnable)
  {
    digitalWrite(led, HIGH);
    doDelay();
    digitalWrite(led, LOW);
    doDelay();
  }

  if (transmittedBits < 8)
  {
    newTransmission = false;
    transmittedBits++;
  }
  else
  {
    //Serial.write(incoming);
    transmittedBits = 0;
    incoming = 0;

    Timer1.setPeriod(lowFreq);
    Timer1.setPwmDuty(transmitterOut, 512);

    digitalWrite(outputTest, false);
  }
}

/*/
 *  Breaks down incoming serial bytes and transmits bits one at a time by sending them as 24kHz square wave for 1, 16kHz square wave for 0.
 *  Differentiation between differing bytes is entirely handled by the receiving end, no additional bits/bytes are added per transmitted byte.
/*/
void transmitBitStartStop()
{
  delayMicroseconds(charDelay);
  if(incoming == 0)
  {
    return;
  }

  if(newTransmission)
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

  /*if(digitalRead(outputTest) == true) Serial.write(1);
  else Serial.write(0);//*/

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

    Timer1.setPeriod(lowFreq);
    Timer1.setPwmDuty(transmitterOut, 512);

    digitalWrite(outputTest, false);
  }
}

/*/
 *  For each byte, receives the extra 'start' bit and then receives eight data bits to then reconstruct into a data byte from the transmitter.
 *  While the 'start' bit has not been found, the receiver is always checking for start bits - if 0 is received instead, repeat.
/*/
void receiveBitExtraBit()
{
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

  if(!newReception)
  {
    if(holder > 0.5)
    {
      carry = 1;
      outgoing <<= 1;
      outgoing += carry;
    }
    else
    {
      carry = 0;
      outgoing <<= 1;
      outgoing += carry;
    } 
  }
  else
  {
    if(holder > 0.5)
    {
      carry = 1;
      //if(random(0,99) < errorChance) carry = 0;
      outgoing |= carry << receivedBits;
    }
    else
    {
      carry = 0;
      //if(random(0,99) < errorChance) carry = 1;
      outgoing |= carry << receivedBits;
    }
  }

  if(outgoing == comma && !newReception)
  {
    newReception = true;
    receivedBits = 0;
    outgoing = 0;
  }
  else if(newReception)
  {
    if (receivedBits < 7) receivedBits++;
    else
    {
      if(outgoing != 0)
      {
        Serial.write(outgoing);
      }

      newReception = false;
      receivedBits = 0;
      outgoing = 0;
    }    
  }
}

/*
 *  While a message is not being received, wait for start byte to be read.
 *  Once start byte is received, synchronize eight bit reconstruction in a stream.
 *  Once stop byte is received, go back to waiting for start byte.
 */
void receiveBitStartStop()
{
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

  if(!newReception)
  {
    if(holder > 0.5)
    {
      carry = 1;
      outgoing <<= 1;
      outgoing += carry;
    }
    else
    {
      carry = 0;
      outgoing <<= 1;
      outgoing += carry;
    } 
  }
  else
  {
    if(holder > 0.5)
    {
      carry = 1;
      //if(random(0,99) < errorChance) carry = 0;
      outgoing |= carry << (receivedBits - 1);
    }
    else
    {
      carry = 0;
      //if(random(0,99) < errorChance) carry = 1;
      outgoing |= carry << (receivedBits - 1);
    }
  }

  uint8_t h1 = 0, h2 = 0;
  bool corruptFlag = false;

  /*if(!(outgoing == startMessage || outgoing == endMessage || outgoing == startChunk || outgoing == endChunk))
  {
    for(int i = 0; i < 8; i++)
    {
      if(random(0,99) < errorChance) h1 = 0;
      h2 |= (bitRead(outgoing, i) ^ h1) << i;
    }
    corruptFlag = true;
  }//*/

  if(outgoing == startMessage && !newReception)
  {
    Serial.write(outgoing);
    newReception = true;
    receivedBits = 0;
    outgoing = 0;
  }
  else if(newReception)
  {
    if (receivedBits < 7) receivedBits++;
    else
    {
      if(corruptFlag)
      {
        if(h2 != 0)
        {
          Serial.write(h2);
        }
      }
      else
      {
        if(outgoing != 0)
        {
          Serial.write(outgoing);
        }
  
        if(outgoing == endMessage)
        {
          newReception = false;
        }
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

void doDelay()
{
  if (delayEnable)
  {
    delay(delayTime);
  }
}
