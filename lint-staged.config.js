export default {
	"*.{js,ts,jsx,tsx}": ["biome check --fix"],
	"*.{json,yaml,yml}": ["prettier --write"],
	"*.{md,mdx}": ["prettier --write", "markdownlint-cli2 --fix"],
	"*.sql": ["prettier --write"],
	"*.{cs,csproj}": (stagedFiles) => [
		`dotnet format ./train-ticket-booking-system.slnx style --include ${stagedFiles.join(" ")}`,
		`jb cleanupcode ${stagedFiles.join(" ")}`
	]
};
