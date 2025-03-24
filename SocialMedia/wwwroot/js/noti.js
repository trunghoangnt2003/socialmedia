"use strict";

function formatTimeDifference(timestamp) {
    const now = new Date();
    const time = new Date(timestamp);
    const timeDifference = (now - time) / 60000; // Phút

    if (timeDifference < 1) return "Vừa xong";
    if (timeDifference < 720) {
        if (timeDifference < 60) return `${Math.floor(timeDifference)} phút trước`;
        return `${Math.floor(timeDifference / 60)} giờ trước`;
    }
    const day = time.getDate().toString().padStart(2, '0');
    const month = (time.getMonth() + 1).toString().padStart(2, '0');
    const year = time.getFullYear();
    const hours = time.getHours().toString().padStart(2, '0');
    const minutes = time.getMinutes().toString().padStart(2, '0');
    return `${day}/${month}/${year} ${hours}:${minutes}`;
}

function addNotificationToast(sender, message, timestamp, postId) {
    const toastContainer = document.getElementById('notification-toast-container');
    const toastId = `toast-${Date.now()}`;

    const toastHtml = `
        <div id="${toastId}" class="notification-toast">
            <div class="toast-icon">
                <i class="fas fa-bell"></i>
            </div>
            <div class="toast-content">
                <p>${sender} ${message}</p>
                <span class="time">${formatTimeDifference(timestamp)}</span>
            </div>
        </div>
    `;

    toastContainer.insertAdjacentHTML('afterbegin', toastHtml);

    const toast = document.getElementById(toastId);
    setTimeout(() => toast.classList.add('show'), 100);

    // Use the postId parameter for redirection
    toast.addEventListener('click', () => {
        if (postId) {
            window.location.href = `/Post/PostDetail/${postId}`;
        }
    });

    setTimeout(() => {
        toast.classList.remove('show');
        setTimeout(() => toast.remove(), 300);
    }, 5000);

    const unreadNotiCount = document.getElementById('unreadNotiCount');
    unreadNotiCount.classList.remove('d-none');
    unreadNotiCount.textContent = parseInt(unreadNotiCount.textContent || 0) + 1;
}

function initializeNotification(connection, currentUserName) {
    console.log("Initializing notification for user: " + currentUserName);
    connection.on("ReceiveNotification", function (sender, message, timestamp, postId) {
        console.log("Received notification:");
        console.log("Sender: " + sender);
        console.log("Message: " + message);
        console.log("Timestamp: " + timestamp);
        console.log("Post ID: " + postId);
        console.log("Current user name: " + currentUserName);
        if (sender !== currentUserName) {
            console.log("Displaying notification toast...");
            addNotificationToast(sender, message, timestamp, postId);
            // Refresh the notification list
            if (typeof loadNotifications === "function") {
                loadNotifications();
            }
        } else {
            console.log("Notification not displayed: Sender is the current user.");
        }
    });
}