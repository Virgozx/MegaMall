"use strict";

// UI Elements
const chatWidget = document.getElementById('chat-widget');
const chatMessages = document.getElementById('chat-messages');
const chatInput = document.getElementById('chat-input');
const chatSendBtn = document.getElementById('chat-send-btn');
const chatToggleBtn = document.getElementById('chat-toggle-btn');

if (chatWidget) {
    var chatConnection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();

    if (chatToggleBtn) {
        chatToggleBtn.addEventListener('click', () => {
            chatWidget.classList.toggle('d-none');
        });
    }

    if (chatSendBtn) {
        chatSendBtn.addEventListener('click', (event) => {
            var message = chatInput.value;
            if (message) {
                // Determine if user is Admin or User to call appropriate method
                // For simplicity, we'll assume this script is for User sending to Admin
                // Admin side would need a different UI or logic
                
                // Check a global variable or data attribute to know if we are admin
                var isAdmin = document.body.dataset.isAdmin === "true";
                
                if (isAdmin) {
                    // Admin logic (needs target user ID) - simplified for now
                    // In a real app, Admin would select a chat session
                    alert("Admin chat interface not fully implemented in this widget.");
                } else {
                    chatConnection.invoke("SendMessageToAdmin", message).catch(err => console.error(err.toString()));
                    appendMessage("You", message);
                }
                chatInput.value = '';
            }
            event.preventDefault();
        });
    }

    function appendMessage(user, message) {
        var li = document.createElement("div");
        li.className = "mb-2";
        li.innerHTML = `<strong>${user}:</strong> ${message}`;
        chatMessages.appendChild(li);
        chatMessages.scrollTop = chatMessages.scrollHeight;
    }

    chatConnection.on("ReceiveMessage", function (user, message) {
        appendMessage(user, message);
        if (chatWidget.classList.contains('d-none')) {
            // Show badge or open
            chatWidget.classList.remove('d-none');
        }
    });

    chatConnection.start().then(function () {
        console.log("Chat Connected.");
    }).catch(function (err) {
        return console.error(err.toString());
    });
}

