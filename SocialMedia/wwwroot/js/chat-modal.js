function OpenChat(friendId) {
    fetch(`/Chat/GetChatModal?friendId=${friendId}`)
        .then(response => response.text())
        .then(html => {
            var chatbox = document.getElementById("chat-modal");
            if (document.getElementById("chat-modal").children[1]) {
                var id = document.getElementById("friend-id").value;
                if (id != friendId) {
                    chatbox.innerHTML = html
                    chatbox.children[1].classList.toggle("active");
                    LoadChatData(friendId);
                    return;
                }
                chatbox.innerHTML = "";
                console.log("close");
                return;
            }

            console.log("open" + friendId);
            chatbox.innerHTML = html
            chatbox.children[1].classList.toggle("active");
            LoadChatData(friendId);
        })
        .catch(error => {
            console.error("Error", error);
        });

}

function LoadChatData(friendId) {
    let messages = " ";
    fetch(`Chat/getAllMessages?friendId=${friendId}`, {
        method: 'GET',
    })
        .then(response => response.json())
        .then(res => {
            res.forEach(v => {
                if (friendId == v.sender) {
                    messages +=
                        `<div>
                        <div class="row message-body">
                            <div class="col-sm-12 message-main-receiver">
                                 ${v.type == 5
                            ? `<img src="${v.contents}" alt="image" class="message-media" style="max-width: 100%;">`
                            : v.type == 6
                                ? `<video controls class="message-media">
                                            <source src="${v.contents}" type="video/mp4">
                                            Your browser does not support the video tag.
                                        </video>`
                                : v.type == 8
                                    ? `<audio controls class="message-media" style="height: 30px">
                                            <source src="${v.contents}" type="audio/mpeg">
                                            Your browser does not support the audio tag.
                                        </audio>`
                                    :
                                    `<div class="receiver">
                                        <div class="message-text">
                                        ${v.contents}
                                        </div>
                                        <div class="message-time">
                                            <span class="pull-right">
                                                ${formatDateTime(v.sendTime)}
                                            </span>
                                        </div>
                                    </div>`
                        }
                            </div>
                        </div>
                    </div>`;
                } else {
                    messages +=
                        `<div>
                        <div class="row message-body">
                            <div class="col-sm-12 message-main-sender">
                            ${v.type == 5
                            ? `<img src="${v.contents}" alt="image" class="message-media">`
                            : v.type == 6
                                ? `<video controls class="message-media">
                                        <source src="${v.contents}" type="video/mp4">
                                        Your browser does not support the video tag.
                                    </video>`
                                : v.type == 8
                                    ? `<audio controls class="message-media" style="height: 30px">
                                        <source src="${v.contents}" type="audio/mpeg">
                                        Your browser does not support the audio tag.
                                    </audio>`
                                    :
                                    `<div class="sender">
                                    <div class="message-text">
                                    ${v.contents}
                                    </div>
                                    <div class="message-time">
                                        <span class="pull-right">
                                            ${formatDateTime(v.sendTime)}
                                        </span>
                                    </div>
                                </div>`
                        }
                            </div>
                        </div>
                    </div>`;
                }
            });
            console.log(messages);
            document.getElementById("chat-body").innerHTML = messages;
        })
        .catch(error => {
            console.error('Error:', error);
        });
}

function formatDateTime(isoString) {
    const date = new Date(isoString);
    return `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, "0")}-${String(date.getDate()).padStart(2, "0")} ` +
        `${String(date.getHours()).padStart(2, "0")}:${String(date.getMinutes()).padStart(2, "0")}`;
}

function SendMessage() {
    console.log("send message");

    var content = document.getElementById("content").value;
    var friendID = document.getElementById("friend-id").value;
    var fileInput = document.getElementById("fileupload");
    var files = fileInput.files;

    var formData = new FormData();
    formData.append("friendID", friendID);
    formData.append("content", content);

    for (var i = 0; i < files.length; i++) {
        formData.append("images", files[i]);
    }

    fetch('/Chat/SendMessage', {
        method: 'POST',
        body: formData
    })
        .then(data => {
            console.log(data);
            document.getElementById("content").value = "";
            fileInput.value = "";
        })
        .catch(error => {
            console.error('Error:', error);
            alert('There was an error sending your message.');
        });
}

function preview() {
    let fileInput = document.getElementById("fileupload");
    let imageContainer = document.getElementById("preview");
    imageContainer.innerHTML = "";

    for (i of fileInput.files) {
        let reader = new FileReader();
        let figure = document.createElement("figure");
        let figCap = document.createElement("figcaption");
        // figCap.innerText = i.name.slice(0, 4) + ".. ." + i.name.slice(-3);
        figure.appendChild(figCap);

        let media;
        console.log(i)
        if (i.type.startsWith("video/")) {
            reader.onload = () => {
                media = document.createElement("video");
                media.setAttribute("controls", true);
                media.setAttribute("src", reader.result);
                media.style.width = "40px";
                media.style.height = "40px";
                figure.insertBefore(media, figCap);
            };
        } else if (i.type.startsWith("image/")) {
            reader.onload = () => {
                media = document.createElement("img");
                media.setAttribute("src", reader.result);
                media.style.width = "40px";
                media.style.height = "40px";
                figure.insertBefore(media, figCap);
            };
        } else if (i.type.startsWith("audio/")) {
            reader.onload = () => {
                media = document.createElement("img");
                media.setAttribute("src", 'https://res.cloudinary.com/ddg2gdfee/image/upload/v1738816200/Headphone_ddonpl.png');
                media.style.width = "40px";
                media.style.height = "40px";
                figure.insertBefore(media, figCap);
            };
        }
        imageContainer.appendChild(figure);
        reader.readAsDataURL(i);
    }
}