import { init } from '@github/markdownlint-github';

const overrides = init({
	'line-length': false,
	'no-duplicate-heading': {
		allow_different_nesting: true,
		siblings_only: true
	},
	'list-marker-space': {
		ul_multi: 3,
		ul_single: 3
	},
	'ul-indent': {
		indent: 4
	}
});
const options = {
	config: overrides,
	customRules: ['@github/markdownlint-github'],
	outputFormatters: [
		[
			'markdownlint-cli2-formatter-pretty',
			{
				appendLink: true
			}
		]
	],
	gitignore: true,
	globs: ['**/*.{md,mdx}']
};

export default options;
