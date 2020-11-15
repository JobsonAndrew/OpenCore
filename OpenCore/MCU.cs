using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OpenCore {
   public class MCU {
      public uint[] R { get; set; } = new uint[16];
      public uint PC {
         get {
            return R[15];
         }
         set {
            R[15] = value;
         }
      }
      public uint SP {
         get {
            return R[14];
         }
         set {
            R[14] = value;
         }
      }
      public MemoryStream Memory { get; set; }
      public MemoryStream Code { get; set; }
      public MCU(int MemorySize = 8192, int CodeSize = 32768) {
         Memory = new MemoryStream(new byte[MemorySize]);
         Code = new MemoryStream(new byte[CodeSize]);
      }
      public void Reset() {
         R = new uint[16];
         Memory.Position = 0;
         Memory.Write(new byte[Memory.Length], 0, (int)Memory.Length);
      }
      public InstructionContainer CurrentInstruction() {
         Code.Seek(PC, SeekOrigin.Begin);
         return InstructionContainer.GetInstruction(Code);
      }
      public void Clock() {
         Code.Seek(PC, SeekOrigin.Begin);
         InstructionContainer ins = InstructionContainer.GetInstruction(Code);
         if (ins.Op != ALU.sleep) {
            uint OpA = R[ins.Rd], OpB = 0;
            switch (ins.Mux) {
               case MUX.reg:
                  OpB = R[ins.Src];
                  break;
               case MUX.im4:
                  OpB = (uint)ins.Src;
                  break;
               case MUX.reg_im16:
                  OpB = (uint)(R[ins.Src] + ins.Imm);
                  break;
               case MUX.im20:
                  OpB = (uint)((ins.Src << 16) | ins.Imm);
                  break;
               default:
                  break;
            }
            switch (ins.Op) {
               case ALU.sleep:
                  break;
               case ALU.mov:
                  R[ins.Rd] = OpB;
                  break;
               case ALU.add:
                  R[ins.Rd] += OpB;
                  break;
               case ALU.adc:
                  R[ins.Rd] += OpB;
                  break;
               case ALU.sub:
                  R[ins.Rd] -= OpB;
                  break;
               case ALU.mul:
                  R[ins.Rd] *= OpB;
                  break;
               case ALU.div:
                  R[ins.Rd] /= OpB;
                  break;
               case ALU.and:
                  R[ins.Rd] &= OpB;
                  break;
               case ALU.or:
                  R[ins.Rd] |= OpB;
                  break;
               case ALU.xor:
                  R[ins.Rd] ^= OpB;
                  break;
               case ALU.not:
                  R[ins.Rd] = ~OpB;
                  break;
               case ALU.shr:
                  R[ins.Rd] >>= (int)(OpB & 0x1F);
                  break;
               case ALU.shl:
                  R[ins.Rd] <<= (int)(OpB & 0x1F);
                  break;
               case ALU.ldb:
                  R[ins.Rd] = ldb(OpB);
                  break;
               case ALU.lds:
                  R[ins.Rd] = lds(OpB);
                  break;
               case ALU.ldw:
                  R[ins.Rd] = ldw(OpB);
                  break;
               case ALU.sdb:
                  sdb(R[ins.Rd], OpB);
                  break;
               case ALU.sds:
                  sds(R[ins.Rd], OpB);
                  break;
               case ALU.sdw:
                  sdw(R[ins.Rd], OpB);
                  break;
               case ALU.sifl:
                  if (R[ins.Rd] < OpB) PC += 2;
                  break;
               case ALU.sifle:
                  if (R[ins.Rd] <= OpB) PC += 2;
                  break;
               case ALU.sifne:
                  if (R[ins.Rd] != OpB) PC += 2;
                  break;
               case ALU.sife:
                  if (R[ins.Rd] == OpB) PC += 2;
                  break;
               default:
                  break;
            }
            if (ins.Rd != 15) PC += ins.Size;
         }
      }
      private void sdb(uint address, uint data) {
         BinaryWriter bw = new BinaryWriter(Memory);
         bw.BaseStream.Seek(address, SeekOrigin.Begin);
         bw.Write((byte)data);
         bw.Flush();
      }
      private void sds(uint address, uint data) {
         BinaryWriter bw = new BinaryWriter(Memory);
         bw.BaseStream.Seek(address, SeekOrigin.Begin);
         bw.Write((short)data);
         bw.Flush();
      }
      private void sdw(uint address, uint data) {
         BinaryWriter bw = new BinaryWriter(Memory);
         bw.BaseStream.Seek(address, SeekOrigin.Begin);
         bw.Write((uint)data);
         bw.Flush();
      }
      private uint ldb(uint address) {
         BinaryReader br = new BinaryReader(Memory);
         br.BaseStream.Seek(address, SeekOrigin.Begin);
         return br.ReadByte();
      }
      private uint lds(uint address) {
         BinaryReader br = new BinaryReader(Memory);
         br.BaseStream.Seek(address, SeekOrigin.Begin);
         return br.ReadUInt16();
      }
      private uint ldw(uint address) {
         BinaryReader br = new BinaryReader(Memory);
         br.BaseStream.Seek(address, SeekOrigin.Begin);
         return br.ReadUInt32();
      }
   }
}
