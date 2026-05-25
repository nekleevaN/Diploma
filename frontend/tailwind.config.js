/** @type {import('tailwindcss').Config} */
export default {
  content: ['./index.html', './src/**/*.{vue,ts,tsx}'],
  theme: {
    extend: {
      colors: {
        // Primary brand: olive #708238
        teal: {
          50:  '#f5f7ed',
          100: '#e7ecce',
          150: '#DEE8C9',
          200: '#ceda9e',
          300: '#b3c66e',
          400: '#94a84a',
          500: '#708238',   // main olive — #708238
          600: '#5d6c2e',
          700: '#4a5624',
          800: '#37401b',
          900: '#242b12',
        },
        // Ivory / cream backgrounds
        ivory: {
          50:  '#ffffff',
          100: '#fdfaf7',
          200: '#f9f4ee',
          300: '#f4ede4',
          400: '#ede3d8',
        },
      },
      fontFamily: {
        sans: ['Inter', 'system-ui', 'sans-serif'],
      },
    }
  },
  plugins: []
}
