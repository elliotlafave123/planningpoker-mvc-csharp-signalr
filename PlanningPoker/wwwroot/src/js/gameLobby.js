import * as signalR from "@microsoft/signalr";

// Function to extract a query parameter from the URL
function getQueryParameter(name) {
	const urlParams = new URLSearchParams(window.location.search);
	return urlParams.get(name);
}

const gameLink = getQueryParameter("gameLink");

console.log("Game Link:", gameLink);

const connection = new signalR.HubConnectionBuilder().withUrl("/gamehub").withAutomaticReconnect().build();

// Variables to store game state and player data
let currentPlayers = [];
let playerVotes = {}; // Stores votes; value can be actual vote, "?", or "-"
let isRoundActive = false; // Track if the round is active
let votesRevealed = false; // Track if votes have been revealed

function startConnection() {
	connection
		.start()
		.then(function () {
			console.log("Connected to game hub");

			connection.invoke("GetConnectionId").then(function (connectionId) {
				console.log("Connection ID:", connectionId);
				document.cookie = "ConnectionId=" + connectionId + "; path=/; max-age=3600";

				console.log("Joining game as host with Game Link:", gameLink);

				connection
					.invoke("JoinGameAsHost", gameLink)
					.then(() => {
						// After joining, request the current game state
						connection.invoke("GetGameState", gameLink).catch(function (err) {
							console.error("Error invoking GetGameState:", err);
						});
					})
					.catch(function (err) {
						console.error("Error invoking JoinGameAsHost:", err);
					});
			});
		})
		.catch(function (err) {
			console.error("Error starting SignalR connection:", err);
			setTimeout(startConnection, 5000);
		});
}

startConnection();

// Function to render the player list
function renderPlayerList(players) {
	const playerListSection = document.getElementById("playerListSection");
	const list = document.getElementById("playersList");

	if (players.length > 0) {
		playerListSection.style.display = "block";
		list.innerHTML = "";

		players.forEach(function (player) {
			const li = document.createElement("li");
			li.classList.add("c-players-list-item");

			// Create a span for the player's name
			const nameSpan = document.createElement("span");
			nameSpan.classList.add("c-players-list-item__name");
			nameSpan.textContent = player.name;

			// Create a div for the player's vote
			const voteDiv = document.createElement("div");
			voteDiv.classList.add("c-players-list-item__vote");

			// Determine what to display in the voteDiv
			let voteDisplay = "";
			if (votesRevealed) {
				// Votes have been revealed; display actual vote
				voteDisplay = playerVotes[player.name] || "-";
			} else if (isRoundActive) {
				// Round is active
				if (playerVotes[player.name] === "?") {
					// Player has voted but votes are not revealed
					voteDisplay = "?";
				} else {
					// Player has not yet voted
					voteDisplay = "-";
				}
			} else {
				// No active round
				voteDisplay = "";
			}

			voteDiv.textContent = voteDisplay;

			// Append the name span and vote div to the list item
			li.appendChild(nameSpan);
			li.appendChild(voteDiv);

			// Append the list item to the list
			list.appendChild(li);
		});
	} else {
		playerListSection.style.display = "none";
	}
}

// Event handlers

// Update the player list when players join or leave
connection.on("UpdatePlayerList", function (players) {
	console.log("Received updated player list:", players);
	currentPlayers = players;
	renderPlayerList(players);

	const waitingMessage = document.getElementById("waitingMessage");
	const hostControls = document.getElementById("hostControls");

	if (players.length > 0) {
		waitingMessage.style.display = "none";

		if (players.length >= 2) {
			hostControls.style.display = "block";
		} else {
			hostControls.style.display = "none";
		}

		const main = document.querySelector(".l-main");
		if (main.classList.contains("l-main--centered")) {
			main.classList.remove("l-main--centered");
		}
	} else {
		waitingMessage.style.display = "block";
		hostControls.style.display = "none";
	}
});

// Handle round started event
connection.on("RoundStarted", function (roundName) {
	console.log("Round started:", roundName);

	isRoundActive = true;
	votesRevealed = false;
	playerVotes = {};

	// Update UI elements
	document.getElementById("startRoundForm").style.display = "none";
	document.getElementById("endRoundSection").style.display = "block";

	// Clear previous votes and re-render player list
	renderPlayerList(currentPlayers);
});

// Handle player voted event
connection.on("PlayerVoted", function (playerName) {
	console.log("Player voted:", playerName);

	// Mark that the player has voted by setting their vote to "?"
	playerVotes[playerName] = "?";

	// Re-render the player list to show "?" next to the player's name
	renderPlayerList(currentPlayers);
});

// Handle votes revealed event
connection.on("VotesRevealed", function (votes) {
	console.log("Votes revealed:", votes);

	isRoundActive = false;
	votesRevealed = true;

	// Update the playerVotes object with actual votes
	playerVotes = {};
	votes.forEach(function (vote) {
		playerVotes[vote.playerName] = vote.card;
	});

	// Re-render the player list with vote values
	renderPlayerList(currentPlayers);

	// Show start round form and hide end round button
	document.getElementById("startRoundForm").style.display = "block";
	document.getElementById("endRoundSection").style.display = "none";
});

// Handle receiving game state
connection.on("ReceiveGameState", function (gameState) {
	console.log("Received game state:", gameState);

	isRoundActive = gameState.isRoundActive;
	votesRevealed = !gameState.isRoundActive && gameState.votesRevealed;

	if (votesRevealed) {
		// Set playerVotes to the revealed votes
		playerVotes = {};
		gameState.votes.forEach(function (vote) {
			playerVotes[vote.playerName] = vote.card;
		});
	} else if (isRoundActive) {
		// Reset playerVotes; we'll update as players vote
		playerVotes = {};
		// Optionally, you can request who has already voted if you track that on the server
	} else {
		// No active round
		playerVotes = {};
	}

	// Update UI elements
	document.getElementById("startRoundForm").style.display = isRoundActive ? "none" : "block";
	document.getElementById("endRoundSection").style.display = isRoundActive ? "block" : "none";

	// Re-render the player list
	renderPlayerList(currentPlayers);
});

// Handle reconnection events
connection.onreconnecting((error) => {
	console.assert(connection.state === signalR.HubConnectionState.Reconnecting);
	console.log("Connection lost due to error. Reconnecting...", error);
});

connection.onreconnected((connectionId) => {
	console.assert(connection.state === signalR.HubConnectionState.Connected);
	console.log("Connection reestablished. Connected with connectionId", connectionId);

	// After reconnection, rejoin the group and request the game state
	connection
		.invoke("JoinGameAsHost", gameLink)
		.then(() => {
			connection.invoke("GetGameState", gameLink).catch(function (err) {
				console.error("Error invoking GetGameState after reconnection:", err);
			});
		})
		.catch(function (err) {
			console.error("Error invoking JoinGameAsHost after reconnection:", err);
		});
});

// Handle errors
connection.on("Error", function (message) {
	console.error("Error:", message);
});

// Start Round form submission
document.getElementById("startRoundForm").addEventListener("submit", function (event) {
	event.preventDefault();
	const roundName = document.getElementById("roundNameInput").value;
	console.log("Starting round with name:", roundName);
	connection.invoke("StartRound", gameLink, roundName).catch(function (err) {
		console.error("Error starting round:", err);
	});
});

// End Round button click
document.getElementById("endRoundButton").addEventListener("click", function () {
	console.log("Ending round with game link:", gameLink);
	connection.invoke("EndRound", gameLink).catch(function (err) {
		console.error("Error ending round:", err);
	});
});

// DOMContentLoaded event
document.addEventListener("DOMContentLoaded", function () {
	console.log("DOM fully loaded and parsed");
	if (document.getElementById("playersList").children.length === 0) {
		console.log("No players in list. Showing waiting message.");
		document.getElementById("waitingMessage").style.display = "block";
	}
});
