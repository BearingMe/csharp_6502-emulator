import type { DefaultTheme } from 'vitepress'

export const sidebar: DefaultTheme.Sidebar = {
  '/tutorials/': [
    {
      text: 'Modular 6502 Emulator Tutorial',
      items: [
        { text: 'Overview', link: '/tutorials/' },
        { text: '1. Architecture & Memory Bus', link: '/tutorials/getting-started' },
        { text: '2. Register File & Status Flags', link: '/tutorials/registers-and-flags' },
        { text: '3. Addressing Mode Resolvers', link: '/tutorials/addressing-modes' },
        { text: '4. Bitwise Math & Arithmetic (ALU)', link: '/tutorials/bitwise-math-and-alu' },
        { text: '5. Instruction Dispatcher & Stepper', link: '/tutorials/instruction-dispatcher' },
        { text: '6. Interrupts & System Vectors', link: '/tutorials/interrupts-and-reset' },
      ],
    },
  ],
  '/how-to/': [
    {
      text: 'Practical How-To Guides',
      items: [
        { text: 'Overview', link: '/how-to/' },
        { text: 'Implement Two\'s Complement ADC / SBC', link: '/how-to/implement-adc-sbc' },
        { text: 'Implement Bitwise Shifts & Rotates', link: '/how-to/implement-shifts-and-rotates' },
        { text: 'Resolve Indexed Indirect Modes (IZX/IZY)', link: '/how-to/resolve-indexed-indirect' },
        { text: 'Emulate 6502 Page Boundary Penalties', link: '/how-to/handle-page-boundary-cycles' },
        { text: 'Replicate the Indirect JMP Hardware Bug', link: '/how-to/replicate-jmp-indirect-bug' },
        { text: 'Build a Step-by-Step CPU Disassembler', link: '/how-to/build-6502-disassembler' },
      ],
    },
  ],
  '/reference/': [
    {
      text: 'Technical Reference',
      items: [
        { text: 'Overview', link: '/reference/' },
        { text: 'Complete Instruction Set Matrix', link: '/reference/instruction-matrix' },
        { text: 'Status Register & Flags', link: '/reference/status-flags' },
        { text: 'Addressing Modes Specification', link: '/reference/addressing-modes' },
        { text: 'Arithmetic & Logical Opcodes', link: '/reference/opcodes-arithmetic-logic' },
        { text: 'Branch & Jump Opcodes', link: '/reference/opcodes-branch-jump' },
        { text: 'Memory & Register Transfer Opcodes', link: '/reference/opcodes-memory-transfers' },
        { text: 'Stack & System Control Opcodes', link: '/reference/opcodes-stack-system' },
      ],
    },
  ],
  '/explanation/': [
    {
      text: 'Architecture & Theory',
      items: [
        { text: 'Overview', link: '/explanation/' },
        { text: '6502 Hardware Architecture', link: '/explanation/hardware-architecture' },
        { text: 'Overflow & Signed Binary Arithmetic', link: '/explanation/signed-overflow-mechanics' },
        { text: 'Cycle-Accurate vs Step Execution', link: '/explanation/cycle-execution-model' },
        { text: 'Memory Map & Page Structure', link: '/explanation/memory-organization-and-stack' },
      ],
    },
  ],
}