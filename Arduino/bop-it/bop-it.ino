#include <Wire.h>
#include <SPI.h>
#include <Adafruit_LIS3DH.h>
#include <Adafruit_Sensor.h>
#include <LiquidCrystal.h>

LiquidCrystal lcd(13, 12, 11, 10, 9, 8);

// attach pin D2 Arduino to pin Echo of HC-SR04
#define echoPin 2
#define trigPin 7
#define buttonPin 0
#define buzzerPin 1

// I2C
Adafruit_LIS3DH lis = Adafruit_LIS3DH();

long duration;
int distance;
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

bool button() 
{
  lcd.setCursor(0,0);
  lcd.print("   Press It !  ");

  tone(buzzerPin,261);
  delay(200);
  noTone(buzzerPin);

  //digitalWrite(buzzerPin,HIGH);
  //delay(2000);
  //digitalWrite(buzzerPin,LOW);

  bool pressed = false;
  int timeCount = 0;

  while(timeCount<globalTime) 
  {
    if(digitalRead(0)>0) 
    {
      pressed = true;
    }
    
    timeCount++;
    delay(2);
  }

  
  if(pressed) 
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

bool accel(float initAcc) 
{
  int timeCount = 0;
  float acc = 0;
  bool shaken = false;

  lcd.setCursor(0,0);
  lcd.print("   Shake It !  ");

  tone(buzzerPin,522);
  delay(200);
  tone(buzzerPin,261);
  delay(200);
  noTone(buzzerPin);
  
  while(timeCount<globalTime) 
  {
    acc = getAccel();
    
    if(acc < initAcc - 1 || acc > initAcc + 1)
    {
      shaken = true;
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

bool ultrasonic() 
{
  bool waved = false;
  int timeCount = 0;
  
  lcd.setCursor(0,0);
  lcd.print("   Wave It!   ");

  tone(buzzerPin,1044);
  delay(200);
  tone(buzzerPin,522);
  delay(200);
  noTone(buzzerPin);

  while(timeCount<globalTime) 
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
      waved = true;
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
    if(digitalRead(0)>0) 
    {
      pressed = true;
    }

    lcd.setCursor(0,0);
    lcd.print("  Press Button  ");
    lcd.setCursor(0,1);
    lcd.print("    to Start    ");

    if(digitalRead(0)>0) 
    {
      pressed = true;
    }
    
    delay(500);

    if(digitalRead(0)>0) 
    {
      pressed = true;
    }
    
    lcd.clear();
    delay(500);

    if(digitalRead(0)>0) 
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

  while(success==true) 
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
    }
    else 
    {
      lcd.setCursor(0,0);
      lcd.print("    Failure!    ");

      tone(buzzerPin,600);
      delay(200);
      tone(buzzerPin,100);
      delay(200);
      noTone(buzzerPin);

      delay(1000);
      lcd.clear();
      delay(1000);
    }
  }
}
