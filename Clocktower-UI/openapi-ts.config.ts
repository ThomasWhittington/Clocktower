// openapi-ts.config.ts
import {
    defineConfig
} from '@hey-api/openapi-ts';

export default defineConfig({
    input: 'http://localhost:5120/swagger/v1/swagger.json', // or URL to your API spec
    output: 'src/openApi',
    plugins: [
        '@hey-api/typescript',
        '@hey-api/transformers',
        '@hey-api/sdk'
    ],
});