# Canoe

**WORK IN PROGRESS!**

Canoe is a websocket server for controlling Amateur Radio hardware on a Windows Machine. 


* Sends audio (PCM) from the default audio output device (headphones) to a websocket
* Receives audio (PCM) from websocket and pipe it to default audio input device (microphone)
* Controls rotator
 

TODO:

* Add low-latecy Opus codec encode/decode
* Add command processor
* Start/stop audio streams
* Choose audio codec (PCM|Opus)
* Choose number of channels (radio only needs 1 channel)
* Choose audio device sample rate
* Calculate end-to-end latency
* Add rotator control
* Add front-end
