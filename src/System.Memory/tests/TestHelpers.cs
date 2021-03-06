// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Xunit;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

using static System.Buffers.Binary.BinaryPrimitives;

namespace System
{
    public static class TestHelpers
    {
        public static void Validate<T>(this Span<T> span, params T[] expected) where T : struct, IEquatable<T>
        {
            Assert.True(span.SequenceEqual(expected));
        }

        public static void ValidateReferenceType<T>(this Span<T> span, params T[] expected)
        {
            Assert.Equal(span.Length, expected.Length);
            for (int i = 0; i < expected.Length; i++)
            {
                T actual = span[i];
                Assert.Same(expected[i], actual);
            }

            T ignore;
            AssertThrows<IndexOutOfRangeException, T>(span, (_span) => ignore = _span[expected.Length]);
        }

        public delegate void AssertThrowsAction<T>(Span<T> span);

        // Cannot use standard Assert.Throws() when testing Span - Span and closures don't get along.
        public static void AssertThrows<E, T>(Span<T> span, AssertThrowsAction<T> action) where E:Exception
        {
            try
            {
                action(span);
                Assert.False(true, "Expected exception: " + typeof(E).GetType());
            }
            catch (E)
            {
            }
            catch (Exception wrongException)
            {
                Assert.False(true, "Wrong exception thrown: Expected " + typeof(E).GetType() + ": Actual: " + wrongException.GetType());
            }
        }

        // 
        // The innocent looking construct:
        //
        //    Assert.Throws<E>( () => new Span() );
        //
        // generates a hidden box of the Span as the return value of the lambda. This makes the IL illegal and unloadable on 
        // runtimes that enforce the actual Span rules (never mind that we expect never to reach the box instruction...)
        //
        // The workaround is to code it like this:
        //
        //    Assert.Throws<E>( () => new Span().DontBox() );
        // 
        // which turns the lambda return type back to "void" and eliminates the troublesome box instruction.
        //
        public static void DontBox<T>(this Span<T> span)
        {
            // This space intentionally left blank.
        }

        public static void Validate<T>(this ReadOnlySpan<T> span, params T[] expected) where T : struct, IEquatable<T>
        {
            Assert.True(span.SequenceEqual(expected));
        }

        public static void ValidateReferenceType<T>(this ReadOnlySpan<T> span, params T[] expected)
        {
            Assert.Equal(span.Length, expected.Length);
            for (int i = 0; i < expected.Length; i++)
            {
                T actual = span[i];
                Assert.Same(expected[i], actual);
            }

            T ignore;
            AssertThrows<IndexOutOfRangeException, T>(span, (_span) => ignore = _span[expected.Length]);
        }

        public delegate void AssertThrowsActionReadOnly<T>(ReadOnlySpan<T> span);

        // Cannot use standard Assert.Throws() when testing Span - Span and closures don't get along.
        public static void AssertThrows<E, T>(ReadOnlySpan<T> span, AssertThrowsActionReadOnly<T> action) where E:Exception
        {
            try
            {
                action(span);
                Assert.False(true, "Expected exception: " + typeof(E).GetType());
            }
            catch (E)
            {
            }
            catch (Exception wrongException)
            {
                Assert.False(true, "Wrong exception thrown: Expected " + typeof(E).GetType() + ": Actual: " + wrongException.GetType());
            }
        }

        // 
        // The innocent looking construct:
        //
        //    Assert.Throws<E>( () => new Span() );
        //
        // generates a hidden box of the Span as the return value of the lambda. This makes the IL illegal and unloadable on 
        // runtimes that enforce the actual Span rules (never mind that we expect never to reach the box instruction...)
        //
        // The workaround is to code it like this:
        //
        //    Assert.Throws<E>( () => new Span().DontBox() );
        // 
        // which turns the lambda return type back to "void" and eliminates the troublesome box instruction.
        //
        public static void DontBox<T>(this ReadOnlySpan<T> span)
        {
            // This space intentionally left blank.
        }

        public static void Validate<T>(this Memory<T> memory, params T[] expected) where T : struct, IEquatable<T>
        {
            Assert.True(memory.Span.SequenceEqual(expected));
        }

        public static void ValidateReferenceType<T>(this Memory<T> memory, params T[] expected)
        {
            T[] bufferArray = memory.ToArray();
            Assert.Equal(memory.Length, expected.Length);
            for (int i = 0; i < expected.Length; i++)
            {
                T actual = bufferArray[i];
                Assert.Same(expected[i], actual);
            }
        }

        public static void Validate<T>(this ReadOnlyMemory<T> memory, params T[] expected) where T : struct, IEquatable<T>
        {
            Assert.True(memory.Span.SequenceEqual(expected));
        }

        public static void ValidateReferenceType<T>(this ReadOnlyMemory<T> memory, params T[] expected)
        {
            T[] bufferArray = memory.ToArray();
            Assert.Equal(memory.Length, expected.Length);
            for (int i = 0; i < expected.Length; i++)
            {
                T actual = bufferArray[i];
                Assert.Same(expected[i], actual);
            }
        }

        public static void Validate<T>(Span<byte> span, T value) where T : struct
        {
            T read = ReadMachineEndian<T>(span);
            Assert.Equal(value, read);
            span.Clear();
        }

        public static TestStructExplicit testExplicitStruct = new TestStructExplicit
        {
            S0 = short.MaxValue,
            I0 = int.MaxValue,
            L0 = long.MaxValue,
            US0 = ushort.MaxValue,
            UI0 = uint.MaxValue,
            UL0 = ulong.MaxValue,
            S1 = short.MinValue,
            I1 = int.MinValue,
            L1 = long.MinValue,
            US1 = ushort.MinValue,
            UI1 = uint.MinValue,
            UL1 = ulong.MinValue
        };

        public static Span<byte> GetSpanBE()
        {
            Span<byte> spanBE = new byte[Unsafe.SizeOf<TestStructExplicit>()];

            WriteInt16BigEndian(spanBE, testExplicitStruct.S0);
            WriteInt32BigEndian(spanBE.Slice(2), testExplicitStruct.I0);
            WriteInt64BigEndian(spanBE.Slice(6), testExplicitStruct.L0);
            WriteUInt16BigEndian(spanBE.Slice(14), testExplicitStruct.US0);
            WriteUInt32BigEndian(spanBE.Slice(16), testExplicitStruct.UI0);
            WriteUInt64BigEndian(spanBE.Slice(20), testExplicitStruct.UL0);
            WriteInt16BigEndian(spanBE.Slice(28), testExplicitStruct.S1);
            WriteInt32BigEndian(spanBE.Slice(30), testExplicitStruct.I1);
            WriteInt64BigEndian(spanBE.Slice(34), testExplicitStruct.L1);
            WriteUInt16BigEndian(spanBE.Slice(42), testExplicitStruct.US1);
            WriteUInt32BigEndian(spanBE.Slice(44), testExplicitStruct.UI1);
            WriteUInt64BigEndian(spanBE.Slice(48), testExplicitStruct.UL1);

            Assert.Equal(56, spanBE.Length);
            return spanBE;
        }

        public static Span<byte> GetSpanLE()
        {
            Span<byte> spanLE = new byte[Unsafe.SizeOf<TestStructExplicit>()];

            WriteInt16LittleEndian(spanLE, testExplicitStruct.S0);
            WriteInt32LittleEndian(spanLE.Slice(2), testExplicitStruct.I0);
            WriteInt64LittleEndian( spanLE.Slice(6), testExplicitStruct.L0);
            WriteUInt16LittleEndian(spanLE.Slice(14), testExplicitStruct.US0);
            WriteUInt32LittleEndian(spanLE.Slice(16), testExplicitStruct.UI0);
            WriteUInt64LittleEndian(spanLE.Slice(20), testExplicitStruct.UL0);
            WriteInt16LittleEndian(spanLE.Slice(28), testExplicitStruct.S1);
            WriteInt32LittleEndian(spanLE.Slice(30), testExplicitStruct.I1);
            WriteInt64LittleEndian(spanLE.Slice(34), testExplicitStruct.L1);
            WriteUInt16LittleEndian(spanLE.Slice(42), testExplicitStruct.US1);
            WriteUInt32LittleEndian(spanLE.Slice(44), testExplicitStruct.UI1);
            WriteUInt64LittleEndian(spanLE.Slice(48), testExplicitStruct.UL1);

            Assert.Equal(56, spanLE.Length);
            return spanLE;
        }
        
        [StructLayout(LayoutKind.Explicit)]
        public struct TestStructExplicit
        {
            [FieldOffset(0)]
            public short S0;
            [FieldOffset(2)]
            public int I0;
            [FieldOffset(6)]
            public long L0;
            [FieldOffset(14)]
            public ushort US0;
            [FieldOffset(16)]
            public uint UI0;
            [FieldOffset(20)]
            public ulong UL0;
            [FieldOffset(28)]
            public short S1;
            [FieldOffset(30)]
            public int I1;
            [FieldOffset(34)]
            public long L1;
            [FieldOffset(42)]
            public ushort US1;
            [FieldOffset(44)]
            public uint UI1;
            [FieldOffset(48)]
            public ulong UL1;
        }

        [StructLayout(LayoutKind.Sequential)]
        public sealed class TestClass
        {
            private double _d;
            public char C0;
            public char C1;
            public char C2;
            public char C3;
            public char C4;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct TestValueTypeWithReference
        {
            public int I;
            public string S;
        }

        public enum TestEnum
        {
            e0,
            e1,
            e2,
            e3,
            e4,
        }
    }
}

