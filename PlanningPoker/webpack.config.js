// webpack.config.js

const path = require("path");
const MiniCssExtractPlugin = require("mini-css-extract-plugin");

module.exports = {
	mode: "development",
	entry: {
		app: "./wwwroot/src/js/index.js",
		joinGame: "./wwwroot/src/js/joinGame.js",
		gameLobby: "./wwwroot/src/js/gameLobby.js",
		styles: "./wwwroot/src/scss/main.scss",
	},
	output: {
		path: path.resolve(__dirname, "wwwroot/dist"),
		filename: "[name].bundle.js",
		publicPath: "/dist/",
	},
	module: {
		rules: [
			{
				test: /\.js$/,
				exclude: /node_modules/,
				use: {
					loader: "babel-loader",
					options: {
						presets: ["@babel/preset-env"],
					},
				},
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
	devtool: "source-map",
	devServer: {
		static: path.join(__dirname, "wwwroot"),
		compress: true,
		port: 9000,
		hot: true,
	},
};
