"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/configureHub").build();

//Disable send button until connection is established
document.getElementById("sendButton").disabled = true;
document.getElementById("testButton").disabled = true;


connection.on("ReceiveMessage", function (message) {
    var msg = message.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
    var h5 = document.createElement("h5");
    h5.textContent = msg;
    document.getElementById("feedback").appendChild(h5);

    if (msg == "done") {
        document.getElementById("testButton").disabled = false;
        //document.getElementById("download").setAttribute('href', getCookie("model"));  
    }
});

connection.on("SetCookie", function (message, type) {
    switch (type) {
        case "dataset":
            document.cookie = "dataset=" + message;

        case "model":
            document.cookie = "model=" + message;    
    }
});

connection.start().then(function () {
    document.getElementById("sendButton").disabled = false;
}).catch(function (err) {
    return console.error(err.toString());
});

document.getElementById("testButton").addEventListener("click", function (event) {
    window.location.href = '/Test'; 
    event.preventDefault();
    });

document.getElementById("sendButton").addEventListener("click", function (event) {
    var configstring = document.getElementById("configstring").value;
    var algorithm = document.getElementById("algorithm").value;
    var dataset = document.getElementById("dataset").value;
    var filename = document.getElementById("filename").value;
    connection.invoke("ConfigureModel", configstring, algorithm, dataset, filename).catch(function (err) { 
        return console.error(err.toString());
    });
    event.preventDefault();
});

