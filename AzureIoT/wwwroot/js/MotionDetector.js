/*********************************************************************
*  #### JS Motion Visualiser ####
*  Coded by Jason Mayes. www.jasonmayes.com
*  Please keep this disclaimer with my code if you use it anywhere. 
*  Thanks. :-)
*  Got feedback or questions, ask here:
*  Github: https://github.com/jasonmayes/JS-Motion-Detection/
*  Updates will be posted to this site.
*********************************************************************/

// Cross browser support to fetch the correct getUserMedia object.
navigator.getUserMedia = navigator.getUserMedia || navigator.webkitGetUserMedia
    || navigator.mozGetUserMedia || navigator.msGetUserMedia;

// Cross browser support for window.URL.
window.URL = window.URL || window.webkitURL || window.mozURL || window.msURL;


var MotionDetector = (function () {
    var alpha = 0.5;
    var version = 0;
    var greyScale = true;

    var canvas = document.getElementById('canvas');
    var canvasFinal = document.getElementById('canvasFinal');
    var canvasUpload = document.getElementById('canvasUpload');
    var video = document.getElementById('camStream');
    var ctx = canvas.getContext('2d');
    var ctxFinal = canvasFinal.getContext('2d');
    var ctxUpload = canvasUpload.getContext('2d');
    var localStream = null;
    var imgData = null;
    var imgDataPrev = [];

    var pictureThreshold = 10;
    var currentValue = 0;
    var timeOut = false;


    function success(stream) {
        localStream = stream;
        // Create a new object URL to use as the video's source.
        video.srcObject = stream;
        video.play();
    }

    function handleError(error) {
        console.error(error);
    }

    function snapshot() {
        canvas.width = video.offsetWidth;
        canvas.height = video.offsetHeight;
        canvasFinal.width = video.offsetWidth;
        canvasFinal.height = video.offsetHeight;

        if (localStream) {
            ctx.drawImage(video, 0, 0);

            // Must capture image data in new instance as it is a live reference.
            // Use alternative live referneces to prevent messed up data.
            imgDataPrev[version] = ctx.getImageData(0, 0, canvas.width, canvas.height);
            version = (version == 0) ? 1 : 0;

            imgData = ctx.getImageData(0, 0, canvas.width, canvas.height);

            var length = imgData.data.length;
            var x = 0;
            while (x < length) {
                if (!greyScale) {
                    // Alpha blending formula: out = (alpha * new) + (1 - alpha) * old.
                    imgData.data[x] = alpha * (255 - imgData.data[x]) + ((1 - alpha) * imgDataPrev[version].data[x]);
                    imgData.data[x + 1] = alpha * (255 - imgData.data[x + 1]) + ((1 - alpha) * imgDataPrev[version].data[x + 1]);
                    imgData.data[x + 2] = alpha * (255 - imgData.data[x + 2]) + ((1 - alpha) * imgDataPrev[version].data[x + 2]);
                    imgData.data[x + 3] = 255;
                } else {
                    // GreyScale.
                    var av = (imgData.data[x] + imgData.data[x + 1] + imgData.data[x + 2]) / 3;
                    var av2 = (imgDataPrev[version].data[x] + imgDataPrev[version].data[x + 1] + imgDataPrev[version].data[x + 2]) / 3;
                    var blended = alpha * (255 - av) + ((1 - alpha) * av2);
                    imgData.data[x] = blended;
                    imgData.data[x + 1] = blended;
                    imgData.data[x + 2] = blended;
                    imgData.data[x + 3] = 255;
                }
                x += 4;
            }
            ctxFinal.putImageData(imgData, 0, 0);

            var imageData = ctxFinal.getImageData(0, 0, canvas.width, canvas.height);

            var imageScore = 0;
            var totalScore = 0;
            for (var i = 0; i < imageData.data.length; i += 4) {
                var r = imageData.data[i] / 3;
                var g = imageData.data[i + 1] / 3;
                var b = imageData.data[i + 2] / 3;
                var pixelScore = r + g + b;
                totalScore = totalScore + pixelScore;

                if (pixelScore > 132 || pixelScore < 123) {
                    imageScore++;
                }
            }
            if (imageScore > 5000) {
                currentValue++;
                if (currentValue > pictureThreshold && !timeOut) {
                    captureAndUploadImage();
                    currentValue = 0;
                }
            } else {
                if (currentValue > 0) {
                    currentValue--;
                }
            }
        }
    }

    function captureAndUploadImage() {
        canvasUpload.width = video.offsetWidth;
        canvasUpload.height = video.offsetHeight;
        ctxUpload.drawImage(video, 0, 0, canvasUpload.width, canvasUpload.height);

        var dataUrl = canvasUpload.toDataURL();
        dataUrl = dataUrl.replace(/^data:image\/(png|jpg);base64,/, "");
        var data = {
            "ImageData": dataUrl,
            "DeviceName": $('#DeviceName').html()
        };

        $.ajax({
            type: "POST",
            contentType: "application/json",
            url: '/home/detect',
            data: JSON.stringify(data),
            dataType: "text",
            success: function (response) {
                console.log(response);
                $('#message').html(response);
            },
            error: function () {
                $('#message').html("Rate Limit exceeded! Please wait a bit.");
                console.log("failure");
            }
        });
        timeOut = true;
        setTimeout(clearTimer, 5000);
    }

    function clearTimer() {
        timeOut = false;
    }

    function init() {
        var constraints = {
            audio: false,
            video: { width: 640, height: 480 }
        };
        if (navigator.getUserMedia) {
            navigator.getUserMedia(constraints, success, handleError);
        } else {
            console.error('Your browser does not support getUserMedia');
        }
        window.setInterval(snapshot, 100);
    }

    return {
        init: init
    };
})();

MotionDetector.init();
