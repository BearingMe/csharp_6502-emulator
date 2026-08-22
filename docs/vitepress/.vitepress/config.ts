import { defineConfig } from 'vitepress'
import { nav } from './config/nav'
import { sidebar } from './config/sidebar'

export default defineConfig({
  base: '/csharp_6502-emulator/',
  title: 'MOS 6502 Architecture & Emulation Guide',
  description: 'Modular, step-by-step engineering documentation for the MOS 6502 microprocessor',
  cleanUrls: true,
  markdown: {
    math: true
  },
  themeConfig: {
    logo: undefined,
    siteTitle: 'MOS 6502 Guide',
    nav,
    sidebar,
    outline: {
      level: [2, 3],
      label: 'On this page',
    },
    search: {
      provider: 'local',
    },
    socialLinks: [
      { icon: 'github', link: 'https://github.com/OneLoneCoder/olcNES' },
    ],
    footer: {
      message: 'MOS 6502 Architecture & Emulation Reference.',
      copyright: 'Documentation reconstructed with Diátaxis framework.',
    },
  },
})