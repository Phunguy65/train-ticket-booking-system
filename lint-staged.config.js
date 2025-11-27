export default {
	"*.{js,ts,jsx,tsx}": ["biome check --fix"],
	"*.{json,yaml,yml}": ["prettier --write"],
	"*.{md,mdx}": ["markdownlint --fix", "prettier --write"],
	"**/*.{cs,csproj}": [
		"dotnet format ./train-ticket-booking-system.sln --check --include"
	]
};
