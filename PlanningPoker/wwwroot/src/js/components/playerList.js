export function renderPlayerList(players) {
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
