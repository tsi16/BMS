$(document).ready(function () {

    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/chathub")
        .build();

    connection.on("ReceiveMessage", function (message) {
        console.log("Received message:", message);

        const messageContainer = document.getElementById('messages');
        const newMessage = document.createElement('div');
        newMessage.className = "chat-message";
        newMessage.setAttribute('data-message-id', message.id);
        newMessage.innerHTML = `
            Sender ${message.senderId}: ${message.Message}
            <small class="chat-time">${new Date().toLocaleString()}</small>
        `;
        messageContainer.appendChild(newMessage);
    });

    connection.start()
        .then(() => {
            console.log("SignalR connection established.");
        })
        .catch(err => console.error("Error establishing SignalR connection:", err));

    let userList = [];
    $('#showUsersButton').click(function () {
        $.ajax({
            type: 'POST',
            url: '/Administrator/Chats/GetAllUsersWithChatSummary',
            success: function (response) {
                if (response.success) {
                    userList = response.users;
                    displayUsers(userList);
                    $('#searchInput').val('');
                    $('#usersModal').modal('show');
                } else {
                    alert('Failed to load users. Please try again.');
                }
            },
            error: function (xhr, status, error) {
                console.error('Error fetching users:', error);
                alert('An error occurred. Please try again.');
            }
        });
    });

    $('#searchInput').on('input', function () {
        const searchTerm = $(this).val().toLowerCase();
        const filteredUsers = userList.filter(user =>
            user.firstName.toLowerCase().includes(searchTerm)
        );
        displayUsers(filteredUsers);
    });

    function displayUsers(users) {
        $('#usersList').empty();
        if (users.length > 0) {
            users.forEach(user => {
                const unreadBadge = user.unreadMessagesCount > 0
                    ? `<span class="badge bg-success float-end">${user.unreadMessagesCount}</span>`
                    : '';

                $('#usersList').append(`
                    <li class="list-group-item user-item d-flex justify-content-between align-items-center" data-user-id="${user.id}">
                        <span>${user.firstName}</span>
                        ${unreadBadge}
                    </li>
                `);
            });
        } else {
            $('#usersList').append('<li class="list-group-item">No users found</li>');
        }
    }

    $(document).on('click', '.user-item', function () {
        const partnerId = $(this).data('user-id');
        console.log('Selected Partner ID (Receiver ID):', partnerId);

        $('#usersModal').modal('hide');
        $('#chatModal').modal('show');

        let senderId;
        $.ajax({
            type: 'GET',
            url: '/Administrator/Chats/GetUserChatHistory',
            data: { partnerId },
            success: function (response) {
                senderId = response.senderId;
                $("#partnerName").text(response.partnerName);
                if (response.success) {
                    $('#chatContainer').empty();
                    response.chatHistory.forEach(item => {
                        const messageClass = item.isSentByMe ? 'sent' : 'received';
                        $('#chatContainer').append(`
                            <div class="chat-message ${messageClass}" data-message-type="${messageClass}"  data-message-id="${item.id}">
                                ${item.message}
                                <small class="chat-time">${item.date}</small>
                            </div>
                        `);
                    });

                    $('#sendMessageButton').data('sender-id', senderId);
                    $('#sendMessageButton').data('receiver-id', partnerId);

                    $('#chatModal').modal('show');
                } else {
                    alert('Failed to load chat history.');
                }
            },
            error: function (xhr, status, error) {
                console.error('Error fetching chat history:', error);
                alert('An error occurred while loading chat history.');
            }
        });
    });

    $('#sendMessageButton').click(function () {
        const receiverId = $(this).data('receiver-id');
        const message = $('#messageInput').val();

        if (!message.trim()) {
            alert('Please enter a message before sending.');
            return;
        }

        $.ajax({
            type: 'POST',
            url: '/Administrator/Chats/SendMessage',
            data: { receiverId, message },
            success: function (response) {
                if (response.success) {
                    $('#messageInput').val('');
                    $('#chatContainer').append(`
                        <div class="chat-message sent" message-id="${response.messageId}">
                            ${message}
                            <small class="chat-time">${new Date().toLocaleString()}</small>
                        </div>
                    `);
                } else if (response.error === "Session expired. Please log in again.") {
                    alert(response.error);
                    window.location.href = '/Users/Login';
                } else {
                    alert(`Failed to send message: ${response.error}`);
                }
            },
            error: function (xhr, status, error) {
                console.error('Error:', xhr.responseText || error);
                alert('An error occurred while sending the message.');
            }
        });
    });

    $(document).ready(function () {

        $('#chatContainer').on('dblclick', '.chat-message', function () { 
            const messageDiv = $(this);
            const messageText = messageDiv.contents().filter(function () {
                return this.nodeType === 3;
            }).text().trim();

            const messageId = messageDiv.data('message-id');
            const message_type = messageDiv.data('message-type');
            alert(message_type);
            console.log("Message ID:", messageId);

            if (!messageId) {
                alert("Message ID is missing.");
                return;
            }
            if (message_type == "sent") {
                messageDiv.html(`
            <input type="text" class="edit-message-input" value="${messageText}" />
            <button class="save-edit">Save</button>
            <button class="cancel-edit">Cancel</button>
            <button class="delete-edit">Delete</button>
                `);
            }
            else {
                messageDiv.html(`<p>${messageText}</p>`);
            }

            messageDiv.on('click', '.save-edit', function () {
                const updatedMessage = messageDiv.find('.edit-message-input').val().trim();
                if (!updatedMessage) {
                    alert('Message cannot be empty.');
                    return;
                }

                $.ajax({
                    type: 'POST',
                    url: '/Administrator/Chats/UpdateMessage',
                    data: { messageId: messageId, message: updatedMessage },
                    success: function (response) {
                        if (response.success) {
                            messageDiv.html(`
                            ${updatedMessage}
                        `);
                        } else {
                            alert(`Failed to update message: ${response.error}`);
                            messageDiv.html(messageText);
                        }
                    },
                    error: function (xhr, status, error) {
                        console.error('Error updating message:', error);
                        alert('An error occurred while updating the message.');
                        messageDiv.html(messageText);
                    }
                });
            });

            messageDiv.on('click', '.cancel-edit', function () {
                messageDiv.html(messageText);
            });

            messageDiv.on('click', '.delete-edit', function () {
                if (confirm("Are you sure you want to delete this message?")) {
                    $.ajax({
                        type: 'POST',
                        url: '/Administrator/Chats/DeleteMessage',
                        data: { messageId: messageId },
                        success: function (response) {
                            if (response.success) {
                                messageDiv.remove();
                            } else {
                                alert(`Failed to delete message: ${response.error}`);
                            }
                        },
                        error: function (xhr, status, error) {
                            console.error('Error deleting message:', error);
                            alert('An error occurred while deleting the message.');
                        }
                    });
                }
            });
        });
    });

});