document.addEventListener("click", function () {
    document.getElementById("popupMenu").style.display = "none";
    document.getElementById("popupNoti").style.display = "none";
    document.getElementById("popupMess").style.display = "none";
});
//---------------- menu----------------------------------------
document.getElementById("avatar").addEventListener("click", function (event) {

    document.getElementById("popupNoti").style.display = "none";
    document.getElementById("popupMess").style.display = "none";

    let popup = document.getElementById("popupMenu");
    popup.style.display = (popup.style.display === "block") ? "none" : "block";
    event.stopPropagation();
});
document.getElementById("popupMenu").addEventListener("click", function (event) {
    event.stopPropagation();
});
//------------- notifiaction------------------------------------
document.getElementById("notification").addEventListener("click", function (event) {

    document.getElementById("popupMenu").style.display = "none";
    document.getElementById("popupMess").style.display = "none";

    let popup = document.getElementById("popupNoti");
    popup.style.display = (popup.style.display === "block") ? "none" : "block";
    event.stopPropagation();
});
document.getElementById("popupNoti").addEventListener("click", function (event) {
    event.stopPropagation();
});
//---------------- messenger------------------------------------
document.getElementById("messenger").addEventListener("click", function (event) {

    document.getElementById("popupNoti").style.display = "none";
    document.getElementById("popupMenu").style.display = "none";
    FetchMessageNoti();
    let popup = document.getElementById("popupMess");
    popup.style.display = (popup.style.display === "block") ? "none" : "block";
    event.stopPropagation();
});
document.getElementById("popupMess").addEventListener("click", function (event) {
    event.stopPropagation();
});

function OpenChatInHeader(id) {
    document.getElementById("popupMess").style.display = "none";
    OpenChat(id);
};

FetchMessageNoti();
function FetchMessageNoti() {
    fetch(`/Chat/getMessageNotification`)
        .then(response => response.json())
        .then(res => {
            var messageNoti = ""
            res.forEach(element => {
                messageNoti += `<div class="element-noti d-flex align-items-center justify-content-between" onclick="OpenChatInHeader(${element.id})">
                                        <div class="d-flex align-items-center">
                                            <img src="${element.senderNavigation.avatar}"
                                             alt="Avatar of user"
                                             class="rounded-circle "
                                             width="40" height="40">
                                             <div class="ms-2  ${element.status == 1 && element.id == element.sender ? `fw-bold` : ''}">
                                                <div> ${element.senderNavigation.name}</div>
                                                <div class="text-truncate content-message-noti"> ${element.contents} </div>
                                             </div>
                                         </div>
                                         ${element.id == element.sender ?
                        ` <div class="align-self-end mb-1">
                                                    ${element.status == 1 ? `<div class="status-messager bg-primary"></div>` : ''}
                                                </div>`
                        : ''
                    }
                                    </div>`
            });

            var popMes = document.getElementById("popupMess");
            popMes.innerHTML = messageNoti;
        })
        .catch(error => {
            console.error("Error", error);
        });
}

FetchNumberMessage();
function FetchNumberMessage() {
    fetch(`/Chat/getNumberMessageSend`)
        .then(response => response.json())
        .then(res => {
            updateUnreadMessages(res);
        })
        .catch(error => {
            console.error("Error", error);
        });
}

function updateUnreadMessages(count) {
    let unreadBadge = document.getElementById("unreadCount");

    if (count > 0) {
        unreadBadge.textContent = count;
        unreadBadge.classList.remove("d-none");
    } else {
        unreadBadge.classList.add("d-none");
    }
};
