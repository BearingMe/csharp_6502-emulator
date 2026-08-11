docs/specs/addressing_modes.md
docs/specs/index.md
docs/specs/registers.md

# 6502 — Índice

Referência técnica do MPU 6502 dividida em 4 arquivos, cada um cobrindo uma camada distinta (registradores → modos de endereçamento → instruções → opcodes). Carregue só o(s) arquivo(s) relevante(s) para a pergunta — não é necessário ler tudo.

| Arquivo | Conteúdo | Abrir quando... |
|---|---|---|
| [6502-registers.md](registers.md) | PC, SP, A, X, Y e as 7 flags do status register (N,V,B,D,I,Z,C) — tamanho, propósito, quando cada flag é setada. | A pergunta é sobre estado interno da CPU: o que uma flag significa, o que um registrador guarda, ou por que uma operação afetou (ou não) uma flag específica. |
| [6502-addressing-modes.md](addressing-modes.md) | As 13 formas de especificar um operando (Immediate, Zero Page, Absolute, Indirect Indexed, etc.), com sintaxe e regras de wrap-around/carry. | A pergunta envolve como um operando é resolvido — diferença entre `(nn,X)` e `(nn),Y`, se um modo dá wrap na zero page, ou qual sintaxe usar para indexação. |
| [6502-instruction-set.md](instruction-set.md) | As 56 instruções agrupadas por função (Load/Store, Stack, Lógica, Aritmética, Shifts, Branches, etc.) com descrição e flags afetadas — sem hex/ciclos. | A pergunta é sobre o que uma instrução *faz* ou qual instrução usar para uma tarefa (ex.: "como comparo dois valores?", "qual a diferença entre ASL e ROL?"). |
| [6502-opcodes.md](opcodes.md) | Tabela cruzada mnemônico × modo → opcode hex, bytes, ciclos, com notas de penalidade de page-crossing e o bug do `JMP (abs)`. | A pergunta precisa do byte exato, contagem de ciclos, tamanho em bytes, ou está fazendo assembly/disassembly/otimização de timing. |

## Notas

- Os arquivos são independentes mas se referenciam: `opcodes.md` usa os mesmos nomes de modo definidos em `addressing-modes.md`; `instruction-set.md` lista as flags nos mesmos termos de `registers.md`.
- Cobertura é do conjunto oficial NMOS 6502 (56 instruções, 151 opcodes legais). Opcodes ilegais/undocumented e extensões 65C02/Rockwell não estão incluídos nesses 4 arquivos.