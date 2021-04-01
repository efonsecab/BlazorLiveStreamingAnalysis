export var webCamStream;
export var streamRecorder;
export var binaryData;

export function startRecording(videoElement) {
    if (navigator.mediaDevices && navigator.mediaDevices.getUserMedia) {
        navigator.mediaDevices.getUserMedia({
            audio: true, video: true
        }).then(function (stream) {
            webCamStream = stream;
            streamRecorder = new MediaRecorder(webCamStream);
            videoElement.srcObject = stream;
            streamRecorder.ondataavailable = function (e) {
                var data = e.data;
                var reader = new FileReader();
                reader.onloadend = function () {
                    binaryData = reader.result
                        .replace("data:", "")
                        .replace(/^.+,/, "");
                    DotNet.invokeMethodAsync('BlazorLiveStreamAnalysis.Client', 'OnNewVideoChunkFound');
                    streamRecorder.start();
                };
                reader.readAsDataURL(data);
            };
            streamRecorder.start();
            setInterval(saveData, 10000);
            //setInterval(saveData, 60000);
        });
    }
}

export function saveData() {
    streamRecorder.stop();
    //streamRecorder.requestData();
}

export function getBinaryData(element) {
    return binaryData;
}