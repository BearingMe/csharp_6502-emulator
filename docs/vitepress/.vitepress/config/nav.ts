import type { DefaultTheme } from 'vitepress'

export const nav: DefaultTheme.NavItem[] = [
  { text: 'Tutorials', link: '/tutorials/getting-started' },
  { text: 'How-To', link: '/how-to/' },
  { text: 'Reference', link: '/reference/' },
  { text: 'Explanation', link: '/explanation/' },
  {
    text: 'References',
    items: [
      { text: 'Masswerk 6502 Guide', link: 'https://www.masswerk.at/6502/6502_instruction_set.html' },
      { text: 'NESDev CPU Reference', link: 'https://www.nesdev.org/wiki/Instruction_reference' },
      { text: 'OneLoneCoder 6502 Source', link: 'https://github.com/OneLoneCoder/olcNES' },
    ],
  },
]