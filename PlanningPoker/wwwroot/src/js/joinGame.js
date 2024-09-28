import * as signalR from "@microsoft/signalr";

// Get gameLink from data attribute
const body = document.querySelector("body");
const gameLink = body.dataset.gameLink;

// Retrieve playerName from localStorage or initialize to empty string
let playerName = localStorage.getItem("playerName") || "";

// Create SignalR connection
const connection = new signalR.HubConnectionBuilder().withUrl("/gamehub").withAutomaticReconnect().build();

function startConnection() {
	connection
		.start()
		.then(function () {
			console.log("Connected to game hub");

			connection.invoke("GetConnectionId").then(function (connectionId) {
				document.cookie = "ConnectionId=" + connectionId + "; path=/; max-age=3600";

				if (playerName) {
					joinGame();
				} else {
					document.getElementById("joinForm").style.display = "block";
				}
			});
		})
		.catch(function (err) {
			console.error(err.toString());
			setTimeout(startConnection, 5000);
		});
}

startConnection();

// SignalR event handlers
connection.on("RoundStarted", function (roundName) {
	alert("A new round has started: " + roundName);
	document.getElementById("roundName").textContent = roundName;
	document.getElementById("votingSection").style.display = "block";
	document.getElementById("waitingSection").style.display = "none";

	// Clear votes and voted players list
	document.getElementById("votesList").innerHTML = "";
	document.getElementById("votedPlayersList").innerHTML = "";

	// Enable voting buttons
	document.querySelectorAll(".card-btn").forEach(function (btn) {
		btn.disabled = false;
	});
});

connection.on("PlayerVoted", function (playerName) {
	const list = document.getElementById("votedPlayersList");
	const li = document.createElement("li");
	li.textContent = playerName;
	list.appendChild(li);
});

connection.on("VotesRevealed", function (votes) {
	// Hide the list of players who have voted
	document.getElementById("votedPlayersSection").style.display = "none";

	// Display votes next to player names
	const votesList = document.getElementById("votesList");
	votesList.innerHTML = "";
	votes.forEach(function (vote) {
		const li = document.createElement("li");
		li.textContent = vote.playerName + ": " + vote.card;
		votesList.appendChild(li);
	});

	// Disable voting buttons
	document.querySelectorAll(".card-btn").forEach(function (btn) {
		btn.disabled = true;
	});
});

// Handle player updates (if needed)
connection.on("UpdatePlayerList", function (players) {
	// Update player list if needed
});

// Handle errors
connection.on("Error", function (message) {
	alert("Error: " + message);
});

// Join game function
function joinGame() {
	connection.invoke("JoinGame", gameLink, playerName);
	document.getElementById("joinForm").style.display = "none";
	document.getElementById("waitingSection").style.display = "block";
}

// Event listeners
document.getElementById("joinForm").addEventListener("submit", function (event) {
	event.preventDefault();
	playerName = document.getElementById("playerName").value;
	localStorage.setItem("playerName", playerName);
	joinGame();
});

document.querySelectorAll(".card-btn").forEach(function (button) {
	button.addEventListener("click", function () {
		const cardValue = this.getAttribute("data-card");
		connection.invoke("SubmitVote", gameLink, cardValue).catch(function (err) {
			console.error(err.toString());
		});
		alert("Vote submitted!");

		// Disable buttons after voting
		document.querySelectorAll(".card-btn").forEach(function (btn) {
			btn.disabled = true;
		});
	});
});
