// Skeleton Program for the AQA AS Summer 2023 examination
// this code should be used in conjunction with the Preliminary Material
// written by the AQA Programmer Team
// developed in Visual Studio 2019

// Version number: 0.0.0 (?????????????????)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AssemblerSimulator
{
    class Program
    {
        const string EMPTY_STRING = "";
        const int HI_MEM = 20;
        const int MAX_INT = 127; // 8 bits available for operand (two's complement integer)
        const int PC = 0; // Program counter; counts the instruction the computer is on
        const int ACC = 1; //Accumulator; stores intermediate results for operations
        const int STATUS = 2; /* Status register; holds 3 flags.
                               * The "Z(ero)" flag is 1 if the value in the accumulator is 0.
                               * The "N(egative)" flag is 1 if the value in the accumulator is negative.
                               * The "(O)V(erflow)" flag is 1 if the value in the accumulator cannot be stored within 8 bits.
                               */
        const int TOS = 3; // Stack pointer
        const int ERR = 4; // Error register

        /// <summary>
        ///     A structure that defines what an instruction should consist of
        /// </summary>
        struct AssemblerInstruction
        {
            public string opCode;
            public string operandString;
            public int operandValue;
        }

        /// <summary>
        ///     Writes the main menu to the console
        /// </summary>
        private static void DisplayMenu()
        {
            Console.WriteLine();
            Console.WriteLine("Main Menu");
            Console.WriteLine("=========");
            Console.WriteLine("L - Load a program file");
            Console.WriteLine("D - Display source code");
            Console.WriteLine("E - Edit source code");
            Console.WriteLine("A - Assemble program");
            Console.WriteLine("R - Run the program");
            Console.WriteLine("X - Exit simulator");
            Console.WriteLine();
        }

        /// <summary> 
        ///     Gets the menu option as a single character string
        /// </summary>
        private static char GetMenuOption()
        {
            string choice = EMPTY_STRING;
            while (choice.Length != 1)
            {
                Console.Write("Enter your choice: ");
                choice = Console.ReadLine();
            }
            return choice[0];
        }

        /// <summary> 
        ///     Resets source code by writing "" to all string elements in the source code array
        /// </summary>
        /// <param name="sourceCode">The source code to be formatted </param>
        private static void ResetSourceCode(string[] sourceCode)
        {
            for (int lineNumber = 0; lineNumber < HI_MEM; lineNumber++)
            {
                sourceCode[lineNumber] = EMPTY_STRING;
            }
        }

        /// <summary>
        ///     Resets memory by writing "" to the opcode and operand strings, 
        ///     and writes 0 to all operand values
        /// </summary>
        /// <param name="memory">The memory to be reset</param>
        private static void ResetMemory(AssemblerInstruction[] memory)
        {
            for (int lineNumber = 0; lineNumber < HI_MEM; lineNumber++)
            {
                memory[lineNumber].opCode = EMPTY_STRING;
                memory[lineNumber].operandString = EMPTY_STRING;
                memory[lineNumber].operandValue = 0;
            }
        }

        /// <summary>
        ///    Displays the source code by looping through every character and writing it 
        /// </summary>
        /// <param name="sourceCode">The source code to be displayed</param>
        private static void DisplaySourceCode(string[] sourceCode)
        {
            Console.WriteLine();
            int numberOfLines = Convert.ToInt32(sourceCode[0]);
            for (int lineNumber = 0; lineNumber < numberOfLines + 1; lineNumber++)
            {
                Console.WriteLine($"{lineNumber,2} {sourceCode[lineNumber],-40}"); /* "2" decides how far away from the "margin" 
                                                                                    * the line number will be, and I don't know what "-40" 
                                                                                    * dictates
                                                                                    */
            }
            Console.WriteLine();
        }

        /// <summary>
        /// DO THIS LATER
        /// </summary>
        /// <param name="sourceCode"></param>
        private static void LoadFile(string[] sourceCode)
        {
            bool fileExists = false;
            int lineNumber = 0;
            string fileName;
            ResetSourceCode(sourceCode);
            Console.Write("Enter filename to load: ");
            fileName = Console.ReadLine();
            try
            {
                StreamReader fileIn = new StreamReader(fileName + ".txt");
                {
                    fileExists = true;
                    string instruction = fileIn.ReadLine();
                    while (instruction != null)
                    {
                        lineNumber++;
                        sourceCode[lineNumber] = instruction;
                        instruction = fileIn.ReadLine();
                    }
                    fileIn.Close();
                    sourceCode[0] = lineNumber.ToString();
                }
            }
            catch (Exception)
            {
                if (!fileExists)
                {
                    Console.WriteLine("Error Code 1"); // The error code for no file existing
                }
                else
                {
                    Console.WriteLine("Error Code 2"); // The error code for the file not being able to be read (maybe)
                    sourceCode[0] = (lineNumber - 1).ToString();
                }
            }
            if (lineNumber > 0)
            {
                DisplaySourceCode(sourceCode);
            }
        }

        /// <summary>
        ///     Adds the ability to edit the source code, by first selecting a line to edit, then rewriting 
        ///     it. NB: Once a line is selected, this is the only line you can edit; in order to edit
        ///     another line, you must exit and recall the method.
        /// </summary>
        /// <param name="sourceCode">The source code to be edited</param>
        private static void EditSourceCode(string[] sourceCode)
        {
            int lineNumber = 0;
            Console.Write("Enter line number of code to edit: ");
            lineNumber = Convert.ToInt32(Console.ReadLine());
            Console.WriteLine(sourceCode[lineNumber]);
            string choice = EMPTY_STRING;
            while (choice != "C")
            {
                choice = EMPTY_STRING;
                while (choice != "E" && choice != "C")
                {
                    Console.WriteLine("E - Edit this line");
                    Console.WriteLine("C - Cancel edit");
                    Console.Write("Enter your choice: ");
                    choice = Console.ReadLine();
                }
                if (choice == "E")
                {
                    Console.Write("Enter the new line: ");
                    sourceCode[lineNumber] = Console.ReadLine();
                }
                DisplaySourceCode(sourceCode);
            }
        }

        /// <summary>
        ///     Checks to see if a label is in the dictionary. If it is not, it is subsequently added as with the label as the key, and the line number as the value.
        ///     If the key is already in use, an error occurs.
        /// </summary>
        /// <param name="symbolTable">A dictionary containing the labels and subsequent line numbers</param>
        /// <param name="thisLabel">The label to be checked against the dictionary</param>
        /// <param name="lineNumber">The line number that the label references to</param>

        private static void UpdateSymbolTable(Dictionary<string, int> symbolTable, string thisLabel, int lineNumber)
        {
            if (symbolTable.ContainsKey(thisLabel))
            {
                Console.WriteLine("Error Code 3"); // The error code for an label already in use
            }
            else
            {
                symbolTable.Add(thisLabel, lineNumber);
            }
        }

        /// <summary>
        ///     Checks to see if a label is associated to the line being processed in PassOne. If
        ///     there is, add the label to the SymbolTable dictionary. If not, return an error
        ///     and write "ERR" to the opCode of that instruction.
        /// </summary>
        /// <param name="instruction">The instruction to be processed</param>
        /// <param name="lineNumber">The line number of the instructions</param>
        /// <param name="memory">The memory "tape"</param>
        /// <param name="symbolTable">The dictionary holding names of "objects"</param>
        private static void ExtractLabel(string instruction, int lineNumber, AssemblerInstruction[] memory, Dictionary<string, int> symbolTable)
        {
            if (instruction.Length > 0)
            {
                string thisLabel = instruction.Substring(0, 5);
                thisLabel = thisLabel.Trim();
                if (thisLabel != EMPTY_STRING)
                {
                    if (instruction[5] != ':')
                    {
                        Console.WriteLine("Error Code 4"); // An error code for a label not being formatted properly
                        memory[0].opCode = "ERR";
                    }
                    else
                    {
                        UpdateSymbolTable(symbolTable, thisLabel, lineNumber);
                    }
                }
            }
        }

        /// <summary>
        ///     "Extracts the opcode" from an AssemblerInstruction struct by taking two substrings off the instruction; one for the initial opcode, the other to check if the value 
        ///     should be treated as a memory address or a decimal numerical.
        /// </summary>
        /// <param name="instruction"></param>
        /// <param name="lineNumber"></param>
        /// <param name="memory"></param>
        private static void ExtractOpCode(string instruction, int lineNumber, AssemblerInstruction[] memory)
        {
            if (instruction.Length > 9)
            {
                string[] opCodeValues = new string[] { "LDA", "STA", "LDA#", "HLT", "ADD", "JMP", "SUB", "CMP#", "BEQ", "SKP", "JSR", "RTN", "   " };
                string operation = instruction.Substring(7, 3); //7 spaces away from left margin, reads 3 characters
                if (instruction.Length > 10)
                {
                    string addressMode = instruction.Substring(10, 1); //10 characters away from left margin, reads next character 

                    if (addressMode == "#") //Checks to see if the operand should be treated as a decimal numerical or a memory address
                    {
                        operation += addressMode;
                    }
                }
                if (opCodeValues.Contains(operation)) //Checks for valid instruction
                {
                    memory[lineNumber].opCode = operation;
                }
                else
                {
                    if (operation != EMPTY_STRING)
                    {
                        Console.WriteLine("Error Code 5"); // An error code for an opcode not being formatted properly
                        memory[0].opCode = "ERR";
                    }
                }
            }
        }

        /// <summary>
        ///     "Extracts the operand" by reading everything past the first 13 characters and then 
        ///     writing that to the operandString in the respective AssemblerInstruction struct.
        /// </summary>
        /// <param name="instruction"></param>
        /// <param name="lineNumber"></param>
        /// <param name="memory"></param>
        private static void ExtractOperand(string instruction, int lineNumber, AssemblerInstruction[] memory)
        {
            if (instruction.Length >= 13) // 7 spaces + 3/4 chars for opcode + 2 spaces + x chars for 
                                          // operand means any valid instruction will be at least 13 characters
            {
                string operand = instruction.Substring(12); // Starts reading from the 13th character (i.e., where the operand will be)
                int thisPosition = -1;
                for (int position = 0; position < operand.Length; position++)
                {
                    if (operand[position] == '*') // Checks for comment
                    {
                        thisPosition = position;
                    }
                }
                if (thisPosition >= 0)
                {
                    operand = operand.Substring(0, thisPosition);
                }
                operand = operand.Trim(); //the string.Trim method removes empty space at the beginning and end of a string
                memory[lineNumber].operandString = operand;
            }
        }

        /// <summary>
        ///     DO THIS LATER
        /// </summary>
        /// <param name="sourceCode"></param>
        /// <param name="memory"></param>
        /// <param name="symbolTable"></param>
        private static void PassOne(string[] sourceCode, AssemblerInstruction[] memory, Dictionary<string, int> symbolTable)
        {
            int numberOfLines = Convert.ToInt32(sourceCode[0]);
            for (int lineNumber = 1; lineNumber <= numberOfLines; lineNumber++)
            {
                string instruction = sourceCode[lineNumber];
                ExtractLabel(instruction, lineNumber, memory, symbolTable);
                ExtractOpCode(instruction, lineNumber, memory);
                ExtractOperand(instruction, lineNumber, memory);
            }
        }

        /// <summary>
        /// DO THIS LATER
        /// </summary>
        /// <param name="memory"></param>
        /// <param name="symbolTable"></param>
        /// <param name="numberOfLines"></param>
        private static void PassTwo(AssemblerInstruction[] memory, Dictionary<string, int> symbolTable, int numberOfLines)
        {
            for (int lineNumber = 1; lineNumber <= numberOfLines; lineNumber++)
            {
                string operand = memory[lineNumber].operandString;
                if (operand != EMPTY_STRING)
                {
                    if (symbolTable.ContainsKey(operand))
                    {
                        int operandValue = symbolTable[operand];
                        memory[lineNumber].operandValue = operandValue;
                    }
                    else
                    {
                        try
                        {
                            int operandValue = Convert.ToInt32(operand);
                            memory[lineNumber].operandValue = operandValue;
                        }
                        catch (Exception)
                        {
                            Console.WriteLine("Error Code 6");
                            memory[0].opCode = "ERR";
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     DO THIS LATER
        /// </summary>
        /// <param name="memory"></param>
        /// <param name="location"></param>
        private static void DisplayMemoryLocation(AssemblerInstruction[] memory, int location)
        {
            Console.Write($"*  {memory[location].opCode,-5}{memory[location].operandValue,-5} | ");
        }

        /// <summary>
        ///     DO THIS LATER
        /// </summary>
        /// <param name="sourceCode"></param>
        /// <param name="location"></param>
        private static void DisplaySourceCodeLine(string[] sourceCode, int location)
        {
            Console.WriteLine($"{location,3}  |  {sourceCode[location],-40}");
        }

        /// <summary>
        ///     DO THIS LATER
        /// </summary>
        /// <param name="sourceCode"></param>
        /// <param name="memory"></param>
        private static void DisplayCode(string[] sourceCode, AssemblerInstruction[] memory)
        {
            Console.WriteLine("*  Memory     Location  Label  Op   Operand Comment");
            Console.WriteLine("*  Contents                    Code");
            int numberOfLines = Convert.ToInt32(sourceCode[0]);
            DisplayMemoryLocation(memory, 0);
            Console.WriteLine("  0  |");
            for (int location = 1; location < numberOfLines + 1; location++)
            {
                DisplayMemoryLocation(memory, location);
                DisplaySourceCodeLine(sourceCode, location);
            }
        }

        /// <summary>
        ///     DO THIS LATER
        /// </summary>
        /// <param name="sourceCode"></param>
        /// <param name="memory"></param>
        private static void Assemble(string[] sourceCode, AssemblerInstruction[] memory)
        {
            ResetMemory(memory);
            int numberOfLines = Convert.ToInt32(sourceCode[0]);
            Dictionary<string, int> symbolTable = new Dictionary<string, int>();
            PassOne(sourceCode, memory, symbolTable);
            if (memory[0].opCode != "ERR")
            {
                memory[0].opCode = "JMP";
                if (symbolTable.ContainsKey("START"))
                {
                    memory[0].operandValue = symbolTable["START"];
                }
                else
                {
                    memory[0].operandValue = 1;
                }
                PassTwo(memory, symbolTable, numberOfLines);
            }
        }

        /// <summary>
        ///     Converts a decimal number to its binary equivalent by repeatedly dividing by 2, and recording the remainder
        /// </summary>
        /// <param name="decimalNumber">The decimal number to be converted</param>
        /// <returns>A string containing 0s and 1s that represents the original decimal number in binary</returns>
        private static string ConvertToBinary(int decimalNumber)
        {
            string binaryString = EMPTY_STRING, bit;
            int remainder;
            while (decimalNumber > 0)
            {
                remainder = decimalNumber % 2;
                bit = remainder.ToString();
                binaryString = bit + binaryString;
                decimalNumber = decimalNumber / 2;
            }
            while (binaryString.Length < 3)
            {
                binaryString = "0" + binaryString;
            }
            return binaryString;
        }

        /// <summary>
        ///     Converts a string of 0s and 1s into an integer number, using the Convert.Int32 method on each indivudual character
        ///     along with a "reverse modulo" operation
        /// </summary>
        /// <param name="binaryString">A string of 0s and 1s</param>
        /// <returns>An integer number representing the string that was passed in</returns>
        private static int ConvertToDecimal(string binaryString)
        {
            int decimalNumber = 0, bitValue;
            foreach (char bit in binaryString)
            {
                bitValue = Convert.ToInt32(bit.ToString());
                decimalNumber = decimalNumber * 2 + bitValue; // "Undoes" the modulo shenanigans in the ConvertToBinary method
            }
            return decimalNumber;
        }

        /// <summary>
        ///     Writes the frame number to the screen, along with a horizontal border
        /// </summary>
        /// <param name="frameNumber">The number signifying what frame the simulator is on</param>
        private static void DisplayFrameDelimiter(int frameNumber)
        {
            if (frameNumber == -1)
            {
                Console.WriteLine("***************************************************************");
            }
            else
            {
                Console.WriteLine($"****** Frame {frameNumber} ************************************************");
            }
        }

        /// <summary>
        ///     Displays information about the program including the registers, the memory and the source code
        /// </summary>
        /// <param name="sourceCode">The source code to be displayed</param>
        /// <param name="memory">The memory to be displayed</param>
        /// <param name="registers">The status of the registers</param>
        private static void DisplayCurrentState(string[] sourceCode, AssemblerInstruction[] memory, int[] registers)
        {
            Console.WriteLine("*");
            DisplayCode(sourceCode, memory);
            Console.WriteLine("*");
            Console.WriteLine($"*  PC:  {registers[PC]}  ACC:  {registers[ACC]}  TOS:  {registers[TOS]}");
            Console.WriteLine("*  Status Register: ZNV");
            Console.WriteLine($"*                   {ConvertToBinary(registers[STATUS])}");
            DisplayFrameDelimiter(-1);
        }

        /// <summary>
        ///     Checks the value in the accumulator for 0, negative and under/overflow cases, 
        ///     and writes the appropriate flags to the status register
        /// </summary>
        /// <param name="value">The value to be checked</param>
        /// <param name="registers">The status register</param>
        private static void SetFlags(int value, int[] registers)
        {
            if (value == 0)
            {
                registers[STATUS] = ConvertToDecimal("100");
            }
            else if (value < 0)
            {
                registers[STATUS] = ConvertToDecimal("010");
            }
            else if (value > MAX_INT || value < -(MAX_INT + 1))
            {
                registers[STATUS] = ConvertToDecimal("001");
            }
            else
            {
                registers[STATUS] = ConvertToDecimal("000");
            }
        }

        /// <summary>
        ///     Reports whenever there is an overflow error (i.e. the value in the error register is "1").
        /// </summary>
        /// <param name="errorMessage"></param>
        /// <param name="registers"></param>
        private static void ReportRunTimeError(string errorMessage, int[] registers)
        {
            Console.WriteLine($"Run time error: {errorMessage}");
            registers[ERR] = 1;
        }

        /// <summary>
        ///     "Loads" the value stored at the memory address to the accumulator
        /// </summary>
        /// <param name="memory">The memory location to be examined</param>
        /// <param name="registers">The registers to be written to</param>
        /// <param name="address">The address in memory to be checked</param>
        private static void ExecuteLDA(AssemblerInstruction[] memory, int[] registers, int address)
        {
            registers[ACC] = memory[address].operandValue;
            SetFlags(registers[ACC], registers);
        }

        /// <summary>
        ///     "Stores" the value in the accumulator to the specified memory address
        /// </summary>
        /// <param name="memory">The memory location to be examined</param>
        /// <param name="registers">The registers to be written to</param>
        /// <param name="address">The address in memory to be examined</param>
        private static void ExecuteSTA(AssemblerInstruction[] memory, int[] registers, int address)
        {
            memory[address].operandValue = registers[ACC];
        }

        /// <summary>
        ///     "Immediately loads" the value to the accumulator.
        /// </summary>
        /// <param name="registers">The array holding the registers</param>
        /// <param name="operand">The integer to be written to the accumulator</param>
        private static void ExecuteLDAimm(int[] registers, int operand)
        {
            registers[ACC] = operand;
            SetFlags(registers[ACC], registers);
        }

        /// <summary>
        ///     "Adds" the value stored at the memory address to the value stored in the accumulator,
        ///     then checks for flow errors. 
        /// </summary>
        /// <param name="memory">The memory to be examined</param>
        /// <param name="registers">The registers to be written to</param>
        /// <param name="address">The address to be examined</param>
        private static void ExecuteADD(AssemblerInstruction[] memory, int[] registers, int address)
        {

            registers[ACC] = registers[ACC] + memory[address].operandValue; // Updates accumulator
            SetFlags(registers[ACC], registers);
            if (registers[STATUS] == ConvertToDecimal("001"))
            {
                ReportRunTimeError("Overflow", registers);
            }
        }

        /// <summary>
        ///     "Subtracts" the value stored at the memory address to the value stored in the accumulator,
        ///     then checks for flow errors.
        /// </summary>
        /// <param name="memory"></param>
        /// <param name="registers"></param>
        /// <param name="address"></param>
        private static void ExecuteSUB(AssemblerInstruction[] memory, int[] registers, int address)
        {
            registers[ACC] = registers[ACC] - memory[address].operandValue;
            SetFlags(registers[ACC], registers);
            if (registers[STATUS] == ConvertToDecimal("001"))
            {
                ReportRunTimeError("Overflow", registers);
            }
        }

        /// <summary>
        ///     "Immediately compares" the value in the accumulator with the value specified by
        ///     the operand, and subtracts the operand from the accumulator value.
        /// </summary>
        /// <param name="registers"></param>
        /// <param name="operand"></param>
        private static void ExecuteCMPimm(int[] registers, int operand)
        {
            int value = registers[ACC] - operand;
            SetFlags(value, registers);
        }

        /// <summary>
        ///     Jumps to the specified address if the zero flag is flagged (i.e. if the value stored in the
        ///     accumulator is 0)
        /// </summary>
        /// <param name="registers"></param>
        /// <param name="address"></param>
        private static void ExecuteBEQ(int[] registers, int address)
        {
            string statusRegister = ConvertToBinary(registers[STATUS]);
            char flagZ = statusRegister[0];
            if (flagZ == '1')
            {
                registers[PC] = address;
            }
        }

        /// <summary>
        ///     "Jumps" to a specified line number by updating the program counter to 
        ///     the line number
        /// </summary>
        /// <param name="registers">The memory registers</param>
        /// <param name="address">The address to be jumped to</param>
        private static void ExecuteJMP(int[] registers, int address)
        {
            registers[PC] = address;
        }

        /// <summary>
        ///     "Skips" the line that is currently running. Not entirely sure why
        /// </summary>
        private static void ExecuteSKP()
        {
        }

        /// <summary>
        ///     Displays the contents of the stack
        /// </summary>
        /// <param name="memory">The memory holding the instructions</param>
        /// <param name="registers">The registers</param>
        private static void DisplayStack(AssemblerInstruction[] memory, int[] registers)
        {
            Console.WriteLine("Stack contents:");
            Console.WriteLine(" ----");
            for (int index = registers[TOS]; index < HI_MEM; index++)
            {
                Console.WriteLine($"| {memory[index].operandValue,2} |");
            }
            Console.WriteLine(" ----");
        }

        /// <summary>
        ///     "Jumps (to the) subroutine" by setting the 
        /// </summary>
        /// <param name="memory"></param>
        /// <param name="registers"></param>
        /// <param name="address"></param>
        private static void ExecuteJSR(AssemblerInstruction[] memory, int[] registers, int address)
        {
            int stackPointer = registers[TOS] - 1;
            memory[stackPointer].operandValue = registers[PC];
            registers[PC] = address;
            registers[TOS] = stackPointer;
            DisplayStack(memory, registers);
        }

        private static void ExecuteRTN(AssemblerInstruction[] memory, int[] registers)
        {
            int stackPointer = registers[TOS];
            registers[TOS] = registers[TOS] + 1;
            registers[PC] = memory[stackPointer].operandValue;
        }

        private static void Execute(string[] sourceCode, AssemblerInstruction[] memory)
        {
            int[] registers = new int[] { 0, 0, 0, 0, 0 };
            int frameNumber = 0, operand = 0;
            SetFlags(registers[ACC], registers);
            registers[PC] = 0;
            registers[TOS] = HI_MEM;
            DisplayFrameDelimiter(frameNumber);
            DisplayCurrentState(sourceCode, memory, registers);
            string opCode = memory[registers[PC]].opCode;
            while (opCode != "HLT")
            {
                frameNumber++;
                Console.WriteLine();
                DisplayFrameDelimiter(frameNumber);
                operand = memory[registers[PC]].operandValue;
                Console.WriteLine($"*  Current Instruction Register:  {opCode} {operand}");
                registers[PC] = registers[PC] + 1;
                switch (opCode)
                {
                    case "LDA":
                        ExecuteLDA(memory, registers, operand); break;
                    case "STA":
                        ExecuteSTA(memory, registers, operand); break;
                    case "LDA#":
                        ExecuteLDAimm(registers, operand); break;
                    case "ADD":
                        ExecuteADD(memory, registers, operand); break;
                    case "JMP":
                        ExecuteJMP(registers, operand); break;
                    case "JSR":
                        ExecuteJSR(memory, registers, operand); break;
                    case "CMP#":
                        ExecuteCMPimm(registers, operand); break;
                    case "BEQ":
                        ExecuteBEQ(registers, operand); break;
                    case "SUB":
                        ExecuteSUB(memory, registers, operand); break;
                    case "SKP":
                        ExecuteSKP(); break;
                    case "RTN":
                        ExecuteRTN(memory, registers); break;
                }
                if (registers[ERR] == 0)
                {
                    opCode = memory[registers[PC]].opCode;
                    DisplayCurrentState(sourceCode, memory, registers);
                }
                else
                {
                    opCode = "HLT";
                }
            }
            Console.WriteLine("Execution terminated");
        }

        private static void AssemblerSimulator()
        {
            string[] sourceCode = new string[HI_MEM];
            AssemblerInstruction[] memory = new AssemblerInstruction[HI_MEM];
            bool finished = false;
            char menuOption;
            ResetSourceCode(sourceCode);
            ResetMemory(memory);
            while (!finished)
            {
                DisplayMenu();
                menuOption = GetMenuOption();
                switch (menuOption) //Switch for figuring out what menu option the user picked
                {
                    case 'L':
                        LoadFile(sourceCode);
                        ResetMemory(memory);
                        break;
                    case 'D':
                        if (sourceCode[0] == EMPTY_STRING)
                        {
                            Console.WriteLine("Error Code 7");
                        }
                        else
                        {
                            DisplaySourceCode(sourceCode);
                        }
                        break;
                    case 'E':
                        if (sourceCode[0] == EMPTY_STRING)
                        {
                            Console.WriteLine("Error Code 8");
                        }
                        else
                        {
                            EditSourceCode(sourceCode);
                            ResetMemory(memory);
                        }
                        break;
                    case 'A':
                        if (sourceCode[0] == EMPTY_STRING)
                        {
                            Console.WriteLine("Error Code 9"); // Error code given when there is no file loaded to be assembled
                        }
                        else
                        {
                            Assemble(sourceCode, memory);
                        }
                        break;
                    case 'R':
                        if (memory[0].operandValue == 0)
                        {
                            Console.WriteLine("Error Code 10"); // Error code given when there is no code to be run
                        }
                        else if (memory[0].opCode == "ERR")
                        {
                            Console.WriteLine("Error Code 11");
                        }
                        else
                        {
                            Execute(sourceCode, memory);
                        }
                        break;
                    case 'X':
                        finished = true;
                        break;
                    default:
                        Console.WriteLine("You did not choose a valid menu option. Try again");
                        break;
                }
            }
            Console.WriteLine("You have chosen to exit the program");
            Console.ReadLine();
        }

        static void Main(string[] args)
        {
            AssemblerSimulator();
        }

    }
}
