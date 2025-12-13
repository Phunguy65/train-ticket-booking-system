export default {
	"*.{js,ts,jsx,tsx}": ["pnpm exec biome check --fix"],
	"*.{json,yaml,yml}": ["pnpm exec prettier --write"],
	"*.{md,mdx}": [
		"pnpm exec prettier --write",
		"pnpm exec markdownlint-cli2 --fix"
	],
	"*.sql": ["pnpm exec prettier --write"],
	"*.{cs,csproj,axaml}": (stagedFiles) => {
		const jbCommand = `jb cleanupcode ${stagedFiles.join(" ")}`;
		// Skip client project on non-windows platforms
		if (process.platform !== "win32") {
			const filteredFiles = stagedFiles.filter(
				(file) => !file.includes("frontend/client")
			);

			const subProjetcs = [
				"backend/backend.csproj",
				"frontend/sdk-client/sdk-client.csproj",
				"frontend/admin/admin.csproj"
			];
			const dotnetFormatCommands = subProjetcs.map(
				(proj) =>
					`dotnet format ${proj} style --include ${filteredFiles.join(" ")}`
			);

			return [...dotnetFormatCommands, jbCommand];
		}
		// On windows, format all files
		return [
			`dotnet format train-ticket-booking-system.slnx style --include ${stagedFiles.join(" ")}`,
			jbCommand
		];
	}
};
