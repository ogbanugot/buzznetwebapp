﻿"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/testHub").build();
document.getElementById("sendButton").disabled = true;

connection.on("ReceiveMessage", function (message) {    
    var length = Math.sqrt(message.length);
    var bigcanvas = document.getElementById("BigCanvas");
    var bgcnxt = bigcanvas.getContext("2d");
    var smallcanvas = document.getElementById("SmallCanvas");
    var smctx = smallcanvas.getContext("2d");
    var imgData = smctx.createImageData(length, length);

    var i, j;
    for (i = 0, j = 0; i < imgData.data.length; i += 4) {
        imgData.data[i] = message[j];
        imgData.data[i + 1] = message[j];
        imgData.data[i + 2] = message[j];
        imgData.data[i + 3] = message[j];
        j++;
    }
    smctx.putImageData(imgData, 0, 0);
    smctx = smallcanvas.getContext("2d");
    bgcnxt.scale(10.0, 10.0);
    bgcnxt.drawImage(smallcanvas, 0, 0);     
});

connection.on("ReceiveLabel", function (message) {
    document.getElementById("predict").value = "";
    document.getElementById("target").value = "";
    document.getElementById("predict").value = message[1];
    document.getElementById("target").value = message[0];   
});

connection.start().then(function () {
    document.getElementById("sendButton").disabled = false;
}).catch(function (err) {
    return console.error(err.toString());
});

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

document.getElementById("predictButton").addEventListener("click", function (event) {
    var dataset = getCookie("dataset");
    var model = getCookie("model");
    connection.invoke("Test", dataset, model).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});

document.getElementById("sendButton").addEventListener("click", function (event) {
    var dataset = document.getElementById("dataset").value;
    var model = document.getElementById("model").value;
    document.cookie = "dataset=" + dataset;
    document.cookie = "model=" + model;
    var x = document.getElementById("canvas");
    if (x.style.display === "none") {
        x.style.display = "block";
    } else {
        x.style.display = "none";
    }
    var y = document.getElementById("choice");
    if (y.style.display === "none") {
        y.style.display = "block";
    } else {
        y.style.display = "none";
    }
    event.preventDefault();
});

