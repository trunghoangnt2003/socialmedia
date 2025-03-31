"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/signalRService").build();
connection.start().then(function () {

    console.log("currentUser " + currentUser + "id " + connection.connectionId);
});
var currentUser = @Context.Session.GetString("User");

connection.on("SendMessageTo", function (reciver) {
    console.log("Send to" + `${reciver}`);
    if (reciver != currentUser) LoadChatData(reciver, false);
});

connection.on("ReceiveMessageFrom", function (sender) {
    console.log("Receive from" + `${sender}`);
    if (sender != currentUser) LoadChatData(sender, true);
});