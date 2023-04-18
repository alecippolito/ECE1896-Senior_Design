#define TIMER_INTERRUPT_DEBUG     2
#define _TIMERINTERRUPT_LOGLEVEL_ 0

#define USE_TIMER_1 false
#define USE_TIMER_2 true

#include <TimerInterrupt.h>
#include <TimerOne.h>

// Functional value definitions/initializations:
const int transmitterOut = 10,    // Define transmitter square wave out pin (needs to be PWM pin)
          receiverIn = A0,        // Define receiver signal in pin (needs to be ADC pin)
          voltageLevel = 471;     // Define voltage threshold value (512 is ~2.5v)
          //voltageLevel = 512;
          
const uint8_t startMessage = 65,  // Define messagestart byte
              endMessage = 67,    // Define message end byte
              startChunk = 2,     // Define chunk start byte
              endChunk = 3,       // Define chunk end byte
              comma = 1,          // Define comma bit for separating transmitted bytes
              charDelay = 12;     // Define delay for each transmitted bit

uint8_t incoming = 0,             // 8-bit integer to store byte sent over serial for transmitting
        outgoing = 0,             // 8-bit integer to store reconstructed byte from received bits
        transmittedBits = 0,      // Number of bits transmitted in current byte
        receivedBits = 0,         // Number of bits received in current byte
        numExtraBits = 1;

bool transmitFlag = false,        // Flag set by ISR to allow transmitter to send one bit
     receiveFlag = false,         // Flag set by ISR to allow receiver to receive one bit
     newTransmission = false,     // Flag set by receiving a serial byte to prevent reading and sending more than one byte at a time
     newReception = false;        // Flag set by receiving either a 'comma' bit, or start byte to reconstruct bits into bytes

// Testing pin/value definitions:
const int delayTime = 250,        // Delay time in milliseconds for artificially delaying transmitter bit output
          outputTest = 9,         // Digital output for simulating receiver hardware signal to microcontroller receive pin
          led = 8,                // LED output for signaling transmitter bit delay over
          errorChance = 1;        // Percentage of bits artificially flipped
          
const bool delayEnable = false;   // Enable transmitter bit delay to view square wave output

// Hardware Timer 1 frequency values:
const int highFreq = 53;          // 19KHz square wave output for logical high
const int lowFreq = 67;           // 15KHz square wave output for logical low

// Receiver sampling votes:
const int samplingVotes = 1;      // Number of times to sample receiver output to decide the current bit
float votesStored = 0;            // Current value of all samples - averaged after final sample is taken 
bool lastVote = false;            // Determines if sampling is complete

// Hardware Timer 2 frequency values:
const int frequencyScaler = 0;    // divides interruptFreq by (frequencyScaler + 1) for effective bitrate
int scaler = 0;                   // Scaler iterator for stalling transmit/receive functions
int interruptFreq = 100;         // Frequency of hardware timer 2 - min of 6600 Hz for accurate data recovery

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
  ITimer2.attachInterrupt(interruptFreq, interruptHandler);

  Serial.begin(1200);
}

void loop()
{  
  // Read from PC
  if(Serial.available() > 0 && !newTransmission && transmittedBits == 0)
  {
    incoming = Serial.read();
    newTransmission = true;
  }//*/

  delayMicroseconds(charDelay);

  if(transmitFlag)
  {
    transmitBitExtraBit();
    //transmitBitStartStop();
    transmitFlag = false;
  }

  delayMicroseconds(charDelay);

  if(receiveFlag)
  {
    for(int i = 0; i < samplingVotes; i++)
    {
      lastVote = i == (samplingVotes - 1) ? true : false;
      receiveBitExtraBit(lastVote);
      //receiveBitStartStop(lastVote);
    }
    receiveFlag = false;
  }
}

/*/  
 *  Breaks down incoming serial bytes and transmits bits one at a time by sending them as 24kHz square wave for 1, 16kHz square wave for 0.
 *  To differentiate bytes on the receiver end, adds one extra bit to the front of every byte.
 *  This increases the number of bits required to transmit messages by 1.125, which is not ideal.
/*/
void transmitBitExtraBit()
{
  if(incoming == 0)
  {
    return;
  }

  if(newTransmission)
  {
    transmittedBits = 0;
  }

  if(newTransmission || transmittedBits < numExtraBits)
  {
    Timer1.setPeriod(highFreq);
    Timer1.setPwmDuty(transmitterOut, 512);

    digitalWrite(outputTest, true);

    doDelay();
    /*if(transmittedBits < (numExtraBits - 1))
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
    }//*/
  }
  else if (bitRead(incoming, transmittedBits-numExtraBits) == 1)
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

  if (transmittedBits < (7 + numExtraBits))
  {
    newTransmission = false;
    transmittedBits++;
  }
  else
  {
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
 *  
/*/
void receiveBitExtraBit(bool lastSample)
{
  if(!newReception)
  {
    votesStored = observeAnalogPin();
  }
  else if(!lastSample && newReception)
  {
    votesStored += observeAnalogPin();
    return;
  }
  else if(newReception)
  {
    votesStored += observeAnalogPin();
    votesStored /= samplingVotes;
  }

  uint8_t carry;

  if(!newReception)
  {
    if(votesStored > 0.5)
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
    if(votesStored > 0.5)
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

  votesStored = 0;

  if(outgoing == comma && !newReception)
  {
    delayMicroseconds(charDelay);
    Serial.write(240);

    newReception = true;
    receivedBits = 0;
    outgoing = 0;
  }
  else if(newReception)
  {
    Serial.write(carry);
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
void receiveBitStartStop(bool lastSample)
{ 
  if(!newReception)
  {
    votesStored = observeAnalogPin();
  }
  else if(!lastSample && newReception)
  {
    votesStored += observeAnalogPin();
    return;
  }
  else if(newReception)
  {
    votesStored += observeAnalogPin();
    votesStored /= samplingVotes;
  }

  uint8_t carry;

  if(!newReception)
  {
    if(votesStored > 0.5)
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
    if(votesStored > 0.5)
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

  Serial.write(outgoing);

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
      if(outgoing != 0)
      {
        Serial.write(outgoing);
      }

      if(outgoing == endMessage || outgoing == 0)
      {
        newReception = false;
      }

      receivedBits = 0;
      outgoing = 0;
    }    
  }
}

int observeAnalogPin()
{
  if (analogRead(receiverIn) >= voltageLevel)
  {
    return 1;
  }
  else
  {
    return 0;
  }
}

void interruptHandler()
{
  if(scaler == 0)
  {
    transmitFlag = true;
    receiveFlag = true;
  }
  if(scaler < frequencyScaler) scaler++;
  else scaler = 0;
}

void doDelay()
{
  if (delayEnable)
  {
    delay(delayTime);
  }
}
