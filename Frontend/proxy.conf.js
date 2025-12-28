const PROXY_CONFIG = [
    {
        context: [
            "/api",
        ],
        target: "https://localhost:7287",
        secure: false,
        changeOrigin: true,
        headers: {
            Connection: 'Keep-Alive'
        }
    }
]

module.exports = PROXY_CONFIG;
