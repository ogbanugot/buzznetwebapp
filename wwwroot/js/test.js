﻿"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/testHub").build();

connection.on("ReceiveMessage", function (message, type) {
    switch (type) {
        case 1:
            var length = Math.sqrt(message.length);
            var bigcanvas = document.getElementById("BigCanvas");
            var bgcnxt = bigcanvas.getContext("2d");
            var imgData = bgcnxt.createImageData(length, length);

            var i, j;
            for (i = 0, j = 0; i < imgData.data.length; i += 4) {
                imgData.data[i] = message[j];
                imgData.data[i + 1] = message[j];
                imgData.data[i + 2] = message[j];
                imgData.data[i + 3] = message[j];
                j++;
            }
            bgcnxt.putImageData(imgData, 0, 0);
            break;

        case 3:
            var length = Math.sqrt(message.length / 3);
            var imglength = length * length;
            var bigcanvas = document.getElementById("BigCanvas");
            var bgcnxt = bigcanvas.getContext("2d");
            var imgData = bgcnxt.createImageData(length, length);
            var j = 0;
            var i;
            for (i = 0; i < imgData.data.length; i += 4) {
                imgData.data[i] = message[j];
                imgData.data[i + 1] = message[j + imglength];
                imgData.data[i + 2] = message[j + (imglength * 2)];
                imgData.data[i + 3] = 255;
                j++;
            }
            bgcnxt.putImageData(imgData, 0, 0);
            break;

        default:
            var length = Math.sqrt(message.length);
            var bigcanvas = document.getElementById("BigCanvas");
            var bgcnxt = bigcanvas.getContext("2d");
            var imgData = bgcnxt.createImageData(length, length);

            var i, j;
            for (i = 0, j = 0; i < imgData.data.length; i += 4) {
                imgData.data[i] = message[j];
                imgData.data[i + 1] = message[j];
                imgData.data[i + 2] = message[j];
                imgData.data[i + 3] = message[j];
                j++;
            }
            bgcnxt.putImageData(imgData, 0, 0);
    }
});

connection.on("ReceiveLabel", function (message) {
    document.getElementById("predict").value = "";
    document.getElementById("target").value = "";
    document.getElementById("predict").value = message[1];
    document.getElementById("target").value = message[0];   
});

document.getElementById("predictButton").addEventListener("click", function (event) {
    var dataset = getCookie("dataset");
    var model = getCookie("model");
    connection.invoke("Test", dataset, model).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});

window.onload = function (e) {
    var h5 = document.createElement("h5");
    var dataset = this.getCookie("dataset");
    var model = this.getCookie("model");
    h5.textContent = "Testing model on " + dataset + " dataset...";

    document.getElementById("test").appendChild(h5);
    connection.start().then(function () {
        connection.invoke("Test", dataset, model).catch(function (err) {
            return console.error(err.toString());
        });
    }).catch(function (err) {
        return console.error(err.toString()); 
    });     
    e.preventDefault();
}

function getCookie(cname) {
    var name = cname + "=";
    var decodedCookie = decodeURIComponent(document.cookie);
    var ca = decodedCookie.split(';');
    for (var i = 0; i < ca.length; i++) {
        var c = ca[i];
        while (c.charAt(0) == ' ') {
            c = c.substring(1);
        }
        if (c.indexOf(name) == 0) {
            return c.substring(name.length, c.length);
        }
    }
    return "";
}