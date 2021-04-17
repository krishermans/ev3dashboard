#!/usr/bin/env python3
from ev3dev2.led import Leds
from time import sleep
import paho.mqtt.client as mqtt
import os

leds = Leds()
os.system('setfont Lat15-TerminusBold14')

MQTT_ClientID = 'ev3_eleven'
MQTT_Broker = '192.168.0.30'
MQTT_Topic = 'Eleven'

def on_connect(client, userdata, flags, rc):
    print("Connected!")
    client.subscribe(MQTT_Topic)

def on_message(client, userdata, msg):
    print(msg.payload.decode())
    if msg.payload.decode() == 'led left on':
        leds.set_color('LEFT', 'RED')
    if msg.payload.decode() == 'led left off':
        leds.set_color('LEFT', 'BLACK')
    if msg.payload.decode() == 'led right on':
        leds.set_color('RIGHT', 'GREEN')
    if msg.payload.decode() == 'led right off':
        leds.set_color('RIGHT', 'BLACK')

leds.all_off()
client = mqtt.Client()
client.connect(MQTT_Broker, 1883, 60)
client.on_connect = on_connect
client.on_message = on_message
client.loop_forever()