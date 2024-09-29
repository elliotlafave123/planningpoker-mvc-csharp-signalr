import * as signalR from "@microsoft/signalr";

// Function to extract a query parameter from the URL
function getQueryParameter(name) {
	const urlParams = new URLSearchParams(window.location.search);
	return urlParams.get(name);
}

const gameLink = getQueryParameter("gameLink");

let playerName = localStorage.getItem("playerName") || "";

// Variables to store game state and player data
let currentPlayers = [];
let playerVotes = {}; // Stores votes; value can be actual vote, "?", or "-"
let isRoundActive = false; // Track if the round is active
let votesRevealed = false; // Track if votes have been revealed

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
					// Show the form to enter player name if not already set
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

// SignalR event handler for updating player list
connection.on("UpdatePlayerList", function (players) {
	console.log("Received updated player list:", players);
	currentPlayers = players;

	// Render the player list using the renderPlayerList function
	renderPlayerList(players, playerVotes, isRoundActive, votesRevealed);

	const playerListSection = document.getElementById("playerListSection");
	if (players.length > 0) {
		playerListSection.style.display = "block";

		const main = document.querySelector(".l-main");

		if (main.classList.contains("l-main--centered")) {
			main.classList.remove("l-main--centered");
		}
	}
});

// SignalR event handler for round started
connection.on("RoundStarted", function (roundName) {
	console.log("Round started:", roundName);
	document.getElementById("roundName").textContent = roundName;
	document.getElementById("votingSection").style.display = "block";
	document.getElementById("waitingSection").style.display = "none";

	isRoundActive = true;
	votesRevealed = false;
	playerVotes = {}; // Reset player votes

	// Enable voting buttons
	document.querySelectorAll(".c-card-button").forEach(function (btn) {
		btn.disabled = false;
	});

	// Re-render the player list
	renderPlayerList(currentPlayers, playerVotes, isRoundActive, votesRevealed);
});

// SignalR event handler for player voted
connection.on("PlayerVoted", function (playerName) {
	console.log("Player voted:", playerName);

	// Mark that the player has voted by setting their vote to "?"
	playerVotes[playerName] = "?";

	// Re-render the player list to show "?" next to the player's name
	renderPlayerList(currentPlayers, playerVotes, isRoundActive, votesRevealed);
});

// SignalR event handler for votes revealed
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
	renderPlayerList(currentPlayers, playerVotes, isRoundActive, votesRevealed);

	// Disable voting buttons
	document.querySelectorAll(".c-card-button").forEach(function (btn) {
		btn.disabled = true;
		btn.classList.remove("c-card-button--selected");
	});
});

// Handle receiving player game state
connection.on("ReceivePlayerGameState", function (gameState) {
	console.log("Received player game state:", gameState);

	const votingSection = document.getElementById("votingSection");
	const waitingSection = document.getElementById("waitingSection");
	const roundNameElement = document.getElementById("roundName");

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

	// Re-render the player list
	renderPlayerList(currentPlayers, playerVotes, isRoundActive, votesRevealed);

	if (isRoundActive) {
		// Round is active
		waitingSection.style.display = "none";
		votingSection.style.display = "block";
		roundNameElement.textContent = gameState.roundName || "Current Round";

		// If the player has already voted, disable voting buttons
		if (gameState.hasVoted) {
			document.querySelectorAll(".c-card-button").forEach(function (btn) {
				btn.disabled = true;

				if (playerVotes[playerName] === btn.getAttribute("data-card")) {
					btn.classList.add("c-card-button--selected");
				}
			});
		} else {
			// Enable voting buttons
			document.querySelectorAll(".c-card-button").forEach(function (btn) {
				btn.disabled = false;
				btn.classList.remove("c-card-button--selected");
			});
		}
	} else {
		// Round is not active
		votingSection.style.display = "none";
		waitingSection.style.display = "block";
	}
});

// Handle errors
connection.on("Error", function (message) {
	console.error("Error:", message);
});

// Join game function
function joinGame() {
	if (gameLink && playerName) {
		connection
			.invoke("JoinGame", gameLink, playerName)
			.then(() => {
				console.log("Successfully joined game.");
				document.getElementById("joinForm").style.display = "none";

				// Request the current game state
				connection.invoke("GetPlayerGameState", gameLink).catch(function (err) {
					console.error("Error invoking GetPlayerGameState:", err);
				});
			})
			.catch((err) => {
				console.error("Failed to join game:", err);
			});
	} else {
		console.error("Game Link or Player Name is missing.");
	}
}

// Event listeners
document.getElementById("joinForm").addEventListener("submit", function (event) {
	event.preventDefault();
	playerName = document.getElementById("playerName").value;
	localStorage.setItem("playerName", playerName);
	joinGame();
});

document.querySelectorAll(".c-card-button").forEach(function (button) {
	button.addEventListener("click", function () {
		const cardValue = this.getAttribute("data-card");
		this.classList.add("c-card-button--selected");
		connection
			.invoke("SubmitVote", gameLink, cardValue)
			.then(() => {
				console.log("Vote submitted!");
				// Disable buttons after voting
				document.querySelectorAll(".c-card-button").forEach(function (btn) {
					btn.disabled = true;
				});
			})
			.catch(function (err) {
				console.error(err.toString());
			});
	});
});

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
