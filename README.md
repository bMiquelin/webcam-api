# webcam-api
Minimal API to retrieve frames from webcamera

# How it works
At startup, it warms up creating a instance of a `Emgu.CV.VideoCapture` with the main video source (webcam).

Endpoint `/info` returns etc information:
* isWarming: Will be true after lib starts the camera (~10s)
* ip
* lastCapture: DateTime when last capture request ended.

Endpont `/frame` returns a binary jpeg file.
* It usually takes around 50ms to 250ms to capture an image
    * Capture: ~37ms to 340ms
    * Save: ~6ms to 30ms
    * Read&Del: ~3ms to 9ms
* Repeated requests seems to warm the video source, making the response faster
* If the camera failed to start or is is still initializing, 404 will be returned.