import {
    defineConfig
} from 'vite'
import react
    from '@vitejs/plugin-react'
import tailwindcss
    from "@tailwindcss/vite";
import path
    from 'path'

// https://vite.dev/config/
export default defineConfig({
    plugins: [
        tailwindcss(),
        react({
            babel: {
                plugins: [['babel-plugin-react-compiler']],
            },
        }),

    ],
    server: {
        proxy: {
            '/api': {
                target: process.env.CLOCKTOWER_SERVER_URI || 'http://localhost:5120',
                changeOrigin: true,
                secure: false
            }
        }
    },
    resolve: {
        alias: {
            '@': path.resolve(__dirname, './src'),
            '@/components': path.resolve(__dirname, './src/components'),
            '@/types': path.resolve(__dirname, './src/types'),
            '@/services': path.resolve(__dirname, './src/services'),
            '@/utils': path.resolve(__dirname, './src/utils'),
            '@/store': path.resolve(__dirname, './src/store'),
        }
    }
})
