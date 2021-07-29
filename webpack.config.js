var path = require("path");

var isProduction = !hasArg(/webpack-dev-server/);

var HtmlWebpackPlugin = require('html-webpack-plugin');
var CopyWebpackPlugin = require('copy-webpack-plugin');

module.exports = [
    {
        mode: isProduction ? 'production' : 'development',
        devtool: isProduction ? 'source-map' : 'eval-source-map',
        entry: "./src/App.fs.js",
        output: {
            path: isProduction ? path.join(__dirname, "./deploy") : path.join(__dirname, "./public"),
            filename: "[contenthash].source.js",
        },
        devServer: {
            publicPath: "/",
            contentBase: "./public"
        },
        plugins: [
            new HtmlWebpackPlugin({
                filename: 'index.html',
                template: 'public/index.html'
            })
        ].concat(isProduction ? [
            new CopyWebpackPlugin({
                patterns: [{
                    from: "./public" }]
            })
        ] : []),
        optimization: {
            splitChunks: {
                chunks: 'all'
            },
        },
    }
]

function hasArg(arg) {
    return arg instanceof RegExp
        ? process.argv.some(x => arg.test(x))
        : process.argv.indexOf(arg) !== -1;
}
