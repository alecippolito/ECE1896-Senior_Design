#include <Wire.h>
#include <SPI.h>
#include <Adafruit_LIS3DH.h>
#include <Adafruit_Sensor.h>
#include <LiquidCrystal.h>

LiquidCrystal lcd(13, 12, 11, 10, 9, 8);

// attach pin D2 Arduino to pin Echo of HC-SR04
#define echoPin 4
#define trigPin 2
#define buttonPin 7
#define buzzerPin 1

// define colors for RGB LCD:
# define red 5
# define green 6
# define blue 3
# define redIntensity = 255

// I2C
Adafruit_LIS3DH lis = Adafruit_LIS3DH();

long duration;
int distance;
int maxScore = 99;
short globalTime = 2000;
short randomNumber;
short currentScore;
bool success;

sensors_event_t event;

void setup(void) 
{
  // Setup LCD's columns and rows:
  lcd.begin(16,2);

  // Setup I2C interface for accel:
  lis.begin();

  // Setup ultrasonic sensor:
  pinMode(trigPin,OUTPUT);
  pinMode(echoPin,INPUT);

  // Setup button:
  pinMode(buttonPin,INPUT);

  // Setup buzzer:
  pinMode(buzzerPin,OUTPUT);
}

bool buttonPressed()
{
  if(digitalRead(buttonPin)>0) 
  {
    return true;
  }
  
  return false;
}

bool button() 
{
  lcd.setCursor(0,0);
  lcd.print("   Press It !  ");

  tone(buzzerPin,261);
  delay(200);
  noTone(buzzerPin);

  bool pressed = false,other = false;
  int timeCount = 0;

  while(timeCount<globalTime/5) 
  {
    if(buttonPressed())
    {
      pressed = true;
    }

    if(wavedUs())
    {
      other = true;
    }

    delay(2);
    timeCount++;
  }

  
  if(pressed && !other) 
  {
    lcd.clear();
    delay(500);

    return true;
  }

  return false;
}

float getAccel() 
{
  lis.getEvent(&event);
  return sqrt(event.acceleration.x*event.acceleration.x + event.acceleration.y*event.acceleration.y + event.acceleration.z*event.acceleration.z);
}

bool shakenAcc(float initAcc)
{
  float acc = getAccel();
    
  if(acc < (initAcc - 2) || acc > (initAcc + 2))
  {
    return true;
  }

  return false;
}

bool accel(float initAcc) 
{
  int timeCount = 0;
  bool shaken = false,other = false;

  lcd.setCursor(0,0);
  lcd.print("   Shake It !  ");

  tone(buzzerPin,522);
  delay(200);
  tone(buzzerPin,261);
  delay(200);
  noTone(buzzerPin);
  
  while(timeCount<globalTime/10) 
  {
    if(shakenAcc(initAcc))
    {
      shaken = true;
    }

    if(wavedUs())
    {
      other = true;
    }

    if(buttonPressed())
    {
      other = true;
    }

    timeCount++;
  }

  if(shaken) 
  {
    lcd.clear();
    delay(500);
    
    return true;
  }

  return false;
}

bool wavedUs()
{
  // Clears the trigPin condition
  digitalWrite(trigPin, LOW);
  delayMicroseconds(2);
  
  // Sets the trigPin HIGH (ACTIVE) for 10 microseconds
  digitalWrite(trigPin, HIGH);
  delayMicroseconds(10);
  digitalWrite(trigPin, LOW);
  
  // Reads the echoPin, returns the sound wave travel time in microseconds
  duration = pulseIn(echoPin, HIGH);
  
  // Calculating the distance
  distance = duration * 0.034 / 2; // Speed of sound wave divided by 2 (go and back)

  if(distance < 5)
  {
    return true;
  }

  return false;
}

bool ultrasonic() 
{
  bool waved = false,other = false;
  int timeCount = 0;
  
  lcd.setCursor(0,0);
  lcd.print("   Wave It!   ");

  tone(buzzerPin,1044);
  delay(200);
  tone(buzzerPin,522);
  delay(200);
  noTone(buzzerPin);

  while(timeCount<(globalTime/5)) 
  {
    if(wavedUs())
    {
      waved = true;
    }

    if(buttonPressed())
    {
      other = true;
    }
    
    timeCount++;
  }
  
  if(waved)
  {
    lcd.clear();
    delay(500);
    
    return true;
  }
  
  return false;
}

void loop() 
{
  success = true;
  currentScore = 0;
  globalTime = 2000;

  bool pressed = false;

  while(!pressed) {
    if(digitalRead(buttonPin)>0) 
    {
      pressed = true;
    }

    lcd.setCursor(0,0);
    lcd.print("  Press Button  ");
    lcd.setCursor(0,1);
    lcd.print("    to Start    ");

    if(digitalRead(buttonPin)>0) 
    {
      pressed = true;
    }
    
    delay(500);

    if(digitalRead(buttonPin)>0) 
    {
      pressed = true;
    }
    
    lcd.clear();
    delay(500);

    if(digitalRead(buttonPin)>0) 
    {
      pressed = true;
    }
  }

  lcd.setCursor(0,0);
  lcd.print("    Starting    ");
  lcd.setCursor(0,1);
  lcd.print("    Game....    ");

  delay(1000);
  lcd.clear();
  delay(1000);

  while(success==true && currentScore<maxScore) 
  {
    lis.getEvent(&event);
    randomNumber = random(3); //random(3);

    switch (randomNumber) 
    {
      case 0:
        success = button();
        break;
      case 1:
        success = accel(sqrt(event.acceleration.x*event.acceleration.x + event.acceleration.y*event.acceleration.y + event.acceleration.z*event.acceleration.z));
        break;
      case 2:
        success = ultrasonic();
        break;
      default:
        lcd.print("Missing");
        delay(1000);
        break;
    }
    
    if(success==true) 
    {
      // Increment and display current score:
      currentScore++;
      globalTime = (2000 / currentScore) + 300;

      lcd.setCursor(0,0);
      lcd.print("    Success!    ");

      analogWrite(red, 0);
      analogWrite(green, 255);
      analogWrite(blue, 0);

      tone(buzzerPin,1044);
      delay(100);
      noTone(buzzerPin);
      delay(100);
      tone(buzzerPin,1044);
      delay(100);
      tone(buzzerPin,1044);
      delay(100);
      noTone(buzzerPin);

      delay(1000);
      lcd.clear();
      analogWrite(red, 0);
      analogWrite(green, 0);
      analogWrite(blue, 0);
      delay(1000);

      lcd.setCursor(0,0);
      lcd.print(" Current Score: ");
      lcd.setCursor(0,1);
      lcd.print("       ");
      lcd.print(currentScore);
      lcd.print("       ");

      delay(1000);
      lcd.clear();
      delay(1000);

      if(currentScore==maxScore)
      {
        lcd.setCursor(0,0);
        lcd.print("    YOU WIN!    ");
        lcd.setCursor(0,1);
        lcd.print("       ");
        lcd.print(currentScore);
        lcd.print("       ");
  
        delay(1000);
        lcd.clear();
        delay(1000);
      }
    }
    else 
    {
      lcd.setCursor(0,0);
      lcd.print("    Failure!    ");

      analogWrite(red, 255);
      analogWrite(green, 0);
      analogWrite(blue, 0);

      tone(buzzerPin,600);
      delay(200);
      tone(buzzerPin,100);
      delay(200);
      noTone(buzzerPin);

      delay(1000);
      lcd.clear();
      analogWrite(red, 0);
      analogWrite(green, 0);
      analogWrite(blue, 0);
      delay(1000);
    }
  }
}
