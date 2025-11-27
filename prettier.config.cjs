/** @type {import('prettier-plugin-sql').SqlBaseOptions} */
const prettierPluginSqlConfig = {
  language: 'tsql',
  keywordCase: 'upper',
}
/**
 * @filename prettier.config.cjs
 * @type import('prettier').Config
 */
module.exports = {
	trailingComma: 'none',
	tabWidth: 4,
	useTabs: true,
	singleQuote: true,
	jsxSingleQuote: true,
	bracketSpacing: true,
	arrowParens: 'always',
	printWidth: 80,
	proseWrap: 'always',
	endOfLine: 'lf',
	semi: true,
	plugins: [require.resolve('prettier-plugin-sql'), require.resolve('@prettier/plugin-xml')],
	...prettierPluginSqlConfig,
};
