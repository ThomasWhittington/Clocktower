import {
    defineConfig
} from 'vitest/config';
import react
    from '@vitejs/plugin-react';
import path
    from 'node:path';
import {
    fileURLToPath
} from 'node:url';

const dirname = typeof __dirname !== 'undefined' ? __dirname : path.dirname(fileURLToPath(import.meta.url));

export default defineConfig({
  plugins: [react()],
    test: {
        globals: true,
        environment: 'jsdom',
        exclude: [
            '**/node_modules/**',
            '**/dist/**',
            '**/.{idea,git,cache,output,temp}/**',
            '**/{karma,rollup,webpack,vite,vitest,jest,ava,babel,nyc,cypress,tsup,build}.config.*',
        ]
    },
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src')
    }
  }
});