import * as signalR from "@microsoft/signalr";

const apiBaseUrl = "https://localhost:5001";
const userId = "00000000-0000-0000-0000-000000000001"; // replace with current user id

const connection = new signalR.HubConnectionBuilder()
    .withUrl(`${apiBaseUrl}/hubs/notification?userId=${userId}`)
    .withAutomaticReconnect()
    .build();

connection.on("ReceiveMessage", (message) => {
    console.log("Realtime message:", message);
});

async function startRealtime() {
    try {
        await connection.start();
        console.log("Connected to notification hub.");

        await connection.invoke("JoinGroup", "review-owners");
        console.log("Joined group: review-owners");
    } catch (error) {
        console.error("SignalR connection failed:", error);
    }
}

async function sendToAll(message) {
    await fetch(`${apiBaseUrl}/api/notifications/send-all`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({ message })
    });
}

async function sendToUser(targetUserId, message) {
    await fetch(`${apiBaseUrl}/api/notifications/send-user`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({ userId: targetUserId, message })
    });
}

async function sendToGroup(groupName, message) {
    await fetch(`${apiBaseUrl}/api/notifications/send-group`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({ groupName, message })
    });
}

startRealtime();

export { sendToAll, sendToUser, sendToGroup };
