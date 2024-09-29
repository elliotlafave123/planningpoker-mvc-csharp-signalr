document.addEventListener("DOMContentLoaded", function () {
	const copyButtons = document.querySelectorAll(".c-copy-box__button");

	copyButtons.forEach((button) => {
		button.addEventListener("click", function () {
			const input = button.parentElement.querySelector(".c-copy-box__input");
			const copyIcon = button.querySelector(".c-copy-box__button__icon");
			const copyButtonText = button.querySelector("span");

			input.select();
			document.execCommand("copy");

			copyButtonText.textContent = "Copied!";
			copyIcon.style.display = "none";
			button.classList.add("c-copy-box__button--copied");

			setTimeout(() => {
				copyButtonText.textContent = "Copy";
				copyIcon.style.display = "inline-block";
				button.classList.remove("c-copy-box__button--copied");
			}, 2000); // 2 seconds
		});
	});
});
