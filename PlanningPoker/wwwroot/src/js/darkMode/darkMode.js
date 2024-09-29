document.addEventListener("DOMContentLoaded", () => {
	// Function to set the theme based on the user's preference or local storage
	const setTheme = (theme) => {
		const darkModeLogo = document.getElementById("darkModeLogo");
		const lightModeLogo = document.getElementById("lightModeLogo");

		if (theme === "light") {
			darkModeLogo.style.display = "none";
			lightModeLogo.style.display = "block";
		} else {
			darkModeLogo.style.display = "block";
			lightModeLogo.style.display = "none";
		}

		document.documentElement.setAttribute("data-theme", theme);
		localStorage.setItem("theme", theme); // Store the user's preference in local storage
	};

	// Get the theme from local storage or use system preference
	const storedTheme = localStorage.getItem("theme");

	// Check if the user's device has a dark mode preference
	const userPrefersDark = window.matchMedia && window.matchMedia("(prefers-color-scheme: dark)").matches;

	// Determine the initial theme
	const initialTheme = storedTheme || (userPrefersDark ? "dark" : "light");

	// Apply the initial theme
	setTheme(initialTheme);

	// Listen for changes in the user's preference and update the theme dynamically
	window.matchMedia("(prefers-color-scheme: dark)").addEventListener("change", (event) => {
		const newTheme = event.matches ? "dark" : "light";
		setTheme(newTheme);
	});
});
