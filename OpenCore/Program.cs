using System;
using System.IO;

namespace OpenCore {
   class Program {
      static MCU m = new MCU();
      static void DrawCPU() {
         Console.WriteLine("|========================================REGISTERS========================================|");
         int index = 0;
         string ch = "R";
         for (int j = 0; j < 4; j++) {
            Console.Write("|");
            for (int i = 0; i < 4; i++) {
               switch (index) {
                  case 14:
                     ch = "SP";
                     break;
                  case 15:
                     ch = "PC";
                     break;
                  default:
                     ch = $"R{index}";
                     break;
               }
               if (i == 3) {
                  Console.Write($"{ch}\t0x{m.R[index++]:X8}|\n");
               }
               else {
                  Console.Write($"{ch}\t0x{m.R[index++]:X8}\t");
               }
            }
         }
         Console.WriteLine("|========================================INSTRUCTION======================================|");
         Console.WriteLine($"-> {m.CurrentInstruction()}");
         Console.WriteLine("|===========================================SRAM==========================================|");
         Console.Write("|");
         for (int i = 0; i < 30; i++) {
            Console.Write($"{i:X2} ");
         }
         Console.WriteLine("");
         Console.Write("|");
         for (int i = 0; i < 30; i++) {
            Console.Write($"{m.Memory.ToArray()[i]:X2} ");
         }
         Console.WriteLine("");
         Console.WriteLine("|=========================================================================================|");

      }

      static void Main(string[] args) {
         m.Reset();
         DrawCPU();

         m.Code.Seek(0, SeekOrigin.Begin);
         InstructionContainer i = new InstructionContainer();
         i.Op = ALU.add;
         i.Mux = MUX.im4;
         i.Rd = 0;
         i.Src = 1;
         i.ToStream(m.Code);
         i.ToStream(m.Code);
         i.ToStream(m.Code);
         i.ToStream(m.Code);
         m.Code.Seek(0, SeekOrigin.Begin);


         while (Console.ReadKey().KeyChar != 'q') {
            m.Clock();
            Console.Clear();
            DrawCPU();
         }

      }
   }
}
