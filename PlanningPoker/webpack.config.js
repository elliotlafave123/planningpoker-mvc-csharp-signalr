const path = require("path");
const MiniCssExtractPlugin = require("mini-css-extract-plugin");

module.exports = {
	mode: "development",
	entry: {
		app: "./wwwroot/src/js/index.js",
		styles: "./wwwroot/src/scss/main.scss",
	},
	output: {
		path: path.resolve(__dirname, "wwwroot"),
		filename: "[name].bundle.js",
	},
	module: {
		rules: [
			{
				test: /\.js$/,
				exclude: /node_modules/,
				use: "babel-loader",
			},
			{
				test: /\.scss$/,
				use: [MiniCssExtractPlugin.loader, "css-loader", "sass-loader"],
			},
		],
	},
	plugins: [
		new MiniCssExtractPlugin({
			filename: "[name].bundle.css",
		}),
	],
	devServer: {
		static: path.join(__dirname, "wwwroot"),
		compress: true,
		port: 9000,
	},
};
