"use strict";
console.log("noti.js loaded");

function formatTimeDifference(sendTime) {
    const now = new Date();
    const time = new Date(sendTime); // Sửa từ timestamp thành sendTime
    const timeDifference = (now - time) / 60000;

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

function addNotificationToast(sender, message, sendTime, postId,notiId) {
    const toastContainer = document.getElementById('notification-toast-container');
    const toastId = `toast-${notiId || Date.now()}`;

    const toastHtml = `
        <div id="${toastId}" class="notification-toast">
            <div class="toast-icon">
                <i class="fas fa-bell"></i>
            </div>
            <div class="toast-content">
                <p>${sender} ${message}</p>
                <span class="time">${formatTimeDifference(sendTime)}</span>
            </div>
        </div>
    `;

    toastContainer.insertAdjacentHTML('afterbegin', toastHtml);

    const toast = document.getElementById(toastId);
    setTimeout(() => toast.classList.add('show'), 100);

    toast.addEventListener('click', () => {
        if (notiId) markNotificationAsRead(notiId);
        if (postId != 0) window.location.href = `/Post/PostDetail/${postId}`;
        else window.location.href = `/Home/FriendRequests`;
    });

    setTimeout(() => {
        toast.classList.remove('show');
        setTimeout(() => toast.remove(), 300);
    }, 5000);

    updateUnreadCount();
}

function markNotificationAsRead(notificationId) {
    fetch('/Post/MarkNotificationAsRead', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(notificationId)
    })
        .then(response => {
            if (!response.ok) throw new Error('Failed to mark notification as read');
            console.log(`Notification ${notificationId} marked as read`);
            loadNotifications();
        })
        .catch(err => console.error("Error marking notification as read: ", err));
}

function markAllNotificationsAsRead() {
    fetch('/Post/MarkAllNotificationsAsRead', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' }
    })
        .then(response => {
            if (!response.ok) throw new Error('Failed to mark all notifications as read');
            console.log("All notifications marked as read");
            loadNotifications();
        })
        .catch(err => console.error("Error marking all notifications as read: ", err));
}

function loadNotifications() {
    fetch('/Post/GetNotifications')
        .then(response => response.json())
        .then(notifications => {
            console.log("Notifications data:", notifications);
            const notificationList = document.getElementById('notificationList');
            if (!notificationList) {
                console.error("notificationList element not found");
                return;
            }

            notificationList.innerHTML = '';

            if (notifications.length === 0) {
                notificationList.innerHTML = '<div class="no-notifications">Bạn chưa có thông báo nào</div>';
                updateUnreadCount(0);
                return;
            }

            notifications.forEach(n => {
                console.log("Rendering notification:", n);
                const itemHtml = `
                    <div class="notification-item ${n.status === 1 ? 'unread' : ''}" data-id="${n.id}">
                        <p>${n.sender} ${n.message}</p>
                        <span class="time">${formatTimeDifference(n.sendTime)}</span>
                    </div>
                `;
                notificationList.insertAdjacentHTML('beforeend', itemHtml);
            });

            document.querySelectorAll('.notification-item').forEach(item => {
                item.addEventListener('click', () => {
                    const notificationId = item.getAttribute('data-id');
                    if (notificationId) markNotificationAsRead(notificationId);
                });
            });

            updateUnreadCount();
        })
        .catch(err => console.error("Error loading notifications: ", err));
}

function updateUnreadCount(count) {
    const unreadNotiCount = document.getElementById('unreadNotiCount');
    if (!unreadNotiCount) return;

    const unreadCount = count !== undefined ? count : document.querySelectorAll('.notification-item.unread').length;
    if (unreadCount > 0) {
        unreadNotiCount.textContent = unreadCount;
        unreadNotiCount.classList.remove('d-none');
    } else {
        unreadNotiCount.classList.add('d-none');
    }
}

function initializeNotification(connection, currentUser) {
    const parsedUser = typeof currentUser === 'string' ? JSON.parse(currentUser) : currentUser;
    const currentUserName = parsedUser.Name;

    console.log("Initializing notification for user: " + currentUserName);

    connection.on("ReceiveNotification", function (sender, message, sendTime, postId, notiId) {
        console.log("Received notification:", { sender, message, sendTime, postId, notiId });
        if (sender !== currentUserName) {
            addNotificationToast(sender, message, sendTime, postId, notiId);
            loadNotifications();
        }
    });

    loadNotifications();

    const markAllReadBtn = document.getElementById('markAllRead');
    if (markAllReadBtn) {
        markAllReadBtn.addEventListener('click', () => {
            markAllNotificationsAsRead();
        });
    }
}

window.loadNotifications = loadNotifications;
window.initializeNotification = initializeNotification;