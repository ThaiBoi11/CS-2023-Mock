# CS-2023-Mock
 My annotated code for the AS 2023 Skeleton Program
#### AssemblerInstruction Struct
A struct(ure), in its simplest form, is a way of "packaging" different types to create a custom type. In the case of AssemblerInstruction, it "packages" 2 strings called opCode and operandString, and an integer called operandValue. This struct is then used to create an array of AssemblerInstructions, resulting in the memory that is referenced throughout the program. 
#### Instruction Format
If a scenario comes up in which you are asked to write a custom line of assembly code, or an entire program, it is useful to understand how the program reads lines and how your code should be formatted. For an instruction to be "valid" (i.e., it does not cause any errors when assembled and run), it must have the following qualities:
1. The instruction itself must start 7 characters from the beginning of the line, and must be written in all caps. This ensures that the different parts of an instruction are identified correctly.
2. The opcode instruction must be a member of the opCodeValues array. This ensures that the opcode is a valid instruction that is in the instruction set. The instruction set consists of  "LDA", "STA", "LDA#", "HLT", "ADD", "JMP", "SUB", "CMP#", "BEQ", "SKP", "JSR", "RTN" and "   ". A hashtag (#) appended to the end of an opcode indicates that the value should be treated as a numerical value, as opposed to a memory location
3. Any comments to a line of code should be appended using an asterisk (*). Any text beyond this asterisk and before the next line will not be read by the program.
4. Variables must be declared either at the beginning, or at the end of the code. (I am unsure if they can be declared midway through the instruction program but this should not be done anyways.)
