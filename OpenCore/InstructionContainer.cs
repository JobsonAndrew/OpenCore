using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OpenCore {

   public enum ALU : byte {
      sleep,
      mov,
      add,
      adc,
      sub,
      mul,
      div,
      and,
      or,
      xor,
      not,
      shr,
      shl,
      ldb,
      lds,
      ldw,
      sdb,
      sds,
      sdw,
      sifl,
      sifle,
      sifne,
      sife
   }

   public enum MUX : byte {
      reg = 0x00,
      im4 = 0x01,
      reg_im16 = 0x02,
      im20 = 0x03
   }

   public class InstructionContainer {
      public ALU Op { get; set; }
      public MUX Mux { get; set; }
      public int Rd { get; set; }
      public int Src { get; set; }
      public int Imm { get; set; }

      public uint Size { get; set; }

      public static InstructionContainer GetInstruction(string str) {

         return null;
      }
      public static InstructionContainer GetInstruction(Stream stream) {
         if (stream != null) {
            if (stream.CanRead) {
               InstructionContainer i = new InstructionContainer();
               BinaryReader br = null;
               byte b = (byte)stream.ReadByte();
               i.Op = (ALU)(b >> 2);
               i.Mux = (MUX)(b & 0x03);
               b = (byte)stream.ReadByte();
               i.Rd = (b >> 4);
               i.Src = (b & 0x0F);
               switch (i.Mux) {
                  case MUX.reg:
                  case MUX.im4:
                     i.Size = 2;
                     break;
                  case MUX.reg_im16:
                     br = new BinaryReader(stream);
                     i.Imm = br.ReadUInt16();
                     i.Size = 4;
                     break;
                  case MUX.im20:
                     br = new BinaryReader(stream);
                     i.Imm = br.ReadUInt16();
                     i.Size = 4;
                     break;
                  default:
                     break;
               }
               return i;
            }
            else {
               throw new Exception("Can't read stream.");
            }
         }
         else {
            throw new NullReferenceException();
         }
      }
      public void ToStream(Stream stream) {
         BinaryWriter bw = null;
         byte b = (byte)((byte)Op << 2);
         b |= (byte)Mux;
         stream.WriteByte(b);
         b = (byte)(Rd << 4);
         b |= (byte)(Src & 0x0F);
         stream.WriteByte(b);
         switch (Mux) {
            case MUX.reg:
               break;
            case MUX.im4:
               break;
            case MUX.reg_im16:
            case MUX.im20:
               bw = new BinaryWriter(stream);
               bw.Write((ushort)Imm);
               break;
            default:
               break;
         }

      }
      public override string ToString() {
         string OpA = $"R{Rd}";
         string OpB = $"R{Src}";

         if (Rd == 14) OpA = "SP";
         if (Rd == 15) OpA = "PC";

         if (Src == 14) OpB = "SP";
         if (Src == 15) OpB = "PC";

         switch (Mux) {
            case MUX.reg:
               return $"{Op} {OpA}, {OpB}".ToUpper();
            case MUX.im4:
               return $"{Op} {OpA}, {Src}".ToUpper();
            case MUX.reg_im16:
               return $"{Op} {OpA}, {OpB} + {Imm}".ToUpper();
            case MUX.im20:
               return $"{Op} {OpA}, {(Src<<16) | Imm}".ToUpper();
            default:
               return "Error";
         }
      }

   }
}
