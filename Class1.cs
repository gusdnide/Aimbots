using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Assault_Cube_Aimbot_2._0
{
    class Memory
    {

        public static Int32 ReadInt32(Process TargetProcess, IntPtr MemoryAddress)
        {
            int BytesRead;
            return BitConverter.ToInt32(Read(TargetProcess, MemoryAddress, sizeof(Int32), out BytesRead), 0);
        }

        public static float ReadFloat(Process TargetProcess, IntPtr MemoryAddress)
        {
            int BytesRead;
            return BitConverter.ToSingle(Read(TargetProcess, MemoryAddress, sizeof(float), out BytesRead), 0);
        }

        private static Byte[] Read(Process TargetProcess, IntPtr MemoryAddress, uint BytesToRead, out int BytesRead)
        {
            byte[] ByteBuffer = new byte[BytesToRead];
            IntPtr BytesReadPTR;
            ReadProcessMemory(TargetProcess.Handle, MemoryAddress, ByteBuffer,
                BytesToRead, out BytesReadPTR);
            BytesRead = BytesReadPTR.ToInt32();
            return ByteBuffer;
        }

        public static void WriteInt32(Process TargetProcess, IntPtr MemoryAddress, Int32 Value)
        {
            int BytesWritten;
            Write(TargetProcess, MemoryAddress, BitConverter.GetBytes(Value), out BytesWritten);
        }

        public static void WriteFloat(Process TargetProcess, IntPtr MemoryAddress, float Value)
        {
            int BytesWritten;
            Write(TargetProcess, MemoryAddress, BitConverter.GetBytes(Value), out BytesWritten);
        }

        private static void Write(Process TargetProcess, IntPtr MemoryAddress, byte[] BytesToWrite, out int BytesWritten)
        {
            IntPtr BytesWrittenPTR;
            WriteProcessMemory(TargetProcess.Handle, MemoryAddress, BytesToWrite, (uint)BytesToWrite.Length, out BytesWrittenPTR);

            BytesWritten = BytesWrittenPTR.ToInt32();
        }

        public static int CalculatePointer(Process TargetProcess, int BaseAddress, int[] Offsets)
        {
            int PTRCount = Offsets.Length - 1;
            byte[] ByteBuffer = new byte[4];
            int TemporaryAddress = 0;
            IntPtr BytesReadPTR;

            BaseAddress += (Int32)TargetProcess.MainModule.BaseAddress;

            if (PTRCount == 0)
                TemporaryAddress = BaseAddress;

            for (int i = 0; i <= PTRCount; i++)
            {
                if (i == PTRCount) 
                {
                    ReadProcessMemory(TargetProcess.Handle, (IntPtr)TemporaryAddress, ByteBuffer, 4, out BytesReadPTR);
                    TemporaryAddress = BitConverter.ToInt32(ByteBuffer, 0) + Offsets[i];
                }
                else if (i == 0) 
                {
                    ReadProcessMemory(TargetProcess.Handle, (IntPtr)BaseAddress, ByteBuffer, 4, out BytesReadPTR);
                    TemporaryAddress = BitConverter.ToInt32(ByteBuffer, 0) + Offsets[0];
                }
                else 
                {
                    ReadProcessMemory(TargetProcess.Handle, (IntPtr)TemporaryAddress, ByteBuffer, 4, out BytesReadPTR);
                    TemporaryAddress = BitConverter.ToInt32(ByteBuffer, 0) + Offsets[i];
                }
            }
            return TemporaryAddress;
        }

        [DllImport("kernel32.dll")]
        private static extern Int32 ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress,
            [In, Out] byte[] buffer, UInt32 size, out IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll")]
        private static extern Int32 WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress,
            [In, Out] byte[] buffer, UInt32 size, out IntPtr lpNumberOfBytesWritten);

    }
}