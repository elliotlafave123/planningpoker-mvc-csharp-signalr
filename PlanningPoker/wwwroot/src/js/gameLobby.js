// wwwroot/src/js/gameLobby.js

import * as signalR from "@microsoft/signalr";

// Get gameLink and isHost from data attributes
const body = document.querySelector("body");
const gameLink = body.dataset.gameLink;
const isHost = body.dataset.isHost === "true";

// Create SignalR connection
const connection = new signalR.HubConnectionBuilder().withUrl("/gamehub").withAutomaticReconnect().build();

function startConnection() {
	connection
		.start()
		.then(function () {
			console.log("Connected to game hub");

			connection.invoke("GetConnectionId").then(function (connectionId) {
				document.cookie = "ConnectionId=" + connectionId + "; path=/; max-age=3600";

				// Join the game as host
				connection.invoke("JoinGameAsHost", gameLink).then(function () {
					document.getElementById("hostControls").style.display = "block";
					document.getElementById("startRoundForm").style.display = "block";
				});
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

	document.getElementById("startRoundForm").style.display = "none";
	document.getElementById("endRoundButton").style.display = "block";

	// Clear votes and voted players list
	document.getElementById("votesList").innerHTML = "";
	document.getElementById("votedPlayersList").innerHTML = "";
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

	// Show start round button again
	document.getElementById("startRoundForm").style.display = "block";
	document.getElementById("endRoundButton").style.display = "none";
});

// Handle player updates
connection.on("UpdatePlayerList", function (players) {
	const list = document.getElementById("playersList");
	list.innerHTML = "";
	players.forEach(function (player) {
		const li = document.createElement("li");
		li.textContent = player.name;
		list.appendChild(li);
	});
});

// Handle errors
connection.on("Error", function (message) {
	alert("Error: " + message);
});

// Host controls
document.getElementById("startRoundForm").addEventListener("submit", function (event) {
	event.preventDefault();
	const roundName = document.getElementById("roundNameInput").value;
	connection.invoke("StartRound", gameLink, roundName).catch(function (err) {
		console.error(err.toString());
	});
});

document.getElementById("endRoundButton").addEventListener("click", function () {
	connection.invoke("EndRound", gameLink).catch(function (err) {
		console.error(err.toString());
	});
});
