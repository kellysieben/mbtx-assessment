// vitest.config.ts
import { defineConfig } from 'vitest/config'

export default defineConfig({
  test: {
    // Specify the test environment (e.g., 'node' or 'jsdom' for React projects)
    environment: 'node', 
    globals: true, // Optional: allows using 'describe', 'it', etc. without imports
  },
})
