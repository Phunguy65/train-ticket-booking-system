export default {
	"*.{js,ts,jsx,tsx}": ["biome check --fix"],
	"*.{json,yaml,yml}": ["prettier --write"],
	"*.{md,mdx}": ["markdownlint-cli2 --fix", "prettier --write"],
	"*.{cs,csproj}": [
		"dotnet format ./train-ticket-booking-system.slnx style --include $STAGED_FILES"
	]
};
