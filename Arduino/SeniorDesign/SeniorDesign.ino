
int led = 8;
int incoming = 0;
int delayTime = 1000;

void setup()
{
  pinMode(led, OUTPUT);

  Serial.begin(9600);
}

void loop()
{
  if(Serial.available() > 0)
  {
    incoming = Serial.read();
    blinkByte(incoming);
    Serial.write(incoming);
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
