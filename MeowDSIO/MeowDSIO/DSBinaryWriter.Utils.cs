﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeowDSIO
{
    public partial class DSBinaryWriter : BinaryWriter
    {
        private static Encoding ShiftJISEncoding = Encoding.GetEncoding("shift_jis");

        // Now with 100% less 0DD0ADDE
        public static readonly byte[] PLACEHOLDER_32BIT = new byte[] { 0xDE, 0xAD, 0xD0, 0x0D };

        public string FileName { get; private set; }

        public long Position { get => BaseStream.Position; set => BaseStream.Position = value; }
        public long Length => BaseStream.Length;
        public void Goto(long absoluteOffset) => BaseStream.Seek(absoluteOffset, SeekOrigin.Begin);
        public void Jump(long relativeOffset) => BaseStream.Seek(relativeOffset, SeekOrigin.Current);

        private Stack<long> StepStack = new Stack<long>();
        private Stack<PaddedRegion> PaddedRegionStack = new Stack<PaddedRegion>();

        private Dictionary<string, long> MarkerDict = new Dictionary<string, long>();

        public bool BigEndian = false;

        public char StrEscapeChar = (char)0;

        public void StepIn(long offset)
        {
            StepStack.Push(Position);
            Goto(offset);
        }

        public void StepOut()
        {
            if (StepStack.Count == 0)
                throw new InvalidOperationException("You cannot step out unless StepIn() was previously called on an offset.");

            Goto(StepStack.Pop());
        }

        public void StepIntoPaddedRegion(long length, byte padding)
        {
            PaddedRegionStack.Push(new PaddedRegion(Position, length, padding));
        }

        public void StepOutOfPaddedRegion()
        {
            if (PaddedRegionStack.Count == 0)
                throw new InvalidOperationException("You cannot step out of padded region unless inside of one " +
                    $"as a result of previously calling {nameof(StepIntoPaddedRegion)}().");

            var deepestPaddedRegion = PaddedRegionStack.Pop();
            deepestPaddedRegion.AdvanceWriterToEnd(this);
        }

        public void DoAt(long offset, Action doAction)
        {
            StepIn(offset);
            doAction();
            StepOut();
        }

        public long Placeholder(string markerName = null)
        {
            var label = Label(markerName);
            Write(PLACEHOLDER_32BIT);
            return label;
        }

        public long Label(string markerName = null)
        {
            var labelOffset = Position;

            if (markerName != null)
            {
                if (MarkerDict.ContainsKey(markerName))
                    MarkerDict[markerName] = labelOffset;
                else
                    MarkerDict.Add(markerName, labelOffset);
            }

            return labelOffset;
        }

        public void Goto(string markerName)
        {
            if (markerName == null)
                throw new ArgumentNullException(nameof(markerName));

            if (MarkerDict.ContainsKey(markerName))
            {
                Position = MarkerDict[markerName];
            }
            else
            {
                throw new ArgumentException("No DSBinaryWriter Marker was registered " +
                    $"with the name '{markerName}'.", nameof(markerName));
            }
        }

        public void StepIn(string markerName)
        {
            if (markerName == null)
                throw new ArgumentNullException(nameof(markerName));

            if (MarkerDict.ContainsKey(markerName))
            {
                StepIn(MarkerDict[markerName]);
            }
            else
            {
                throw new ArgumentException("No DSBinaryWriter Marker was registered " +
                    $"with the name '{markerName}'.", nameof(markerName));
            }
        }

        public void Replace(string markerName, int replaceMarkerVal)
        {
            StepIn(markerName);
            Write(replaceMarkerVal);
            StepOut();
        }

        public void PointToHere(string markerName)
        {
            Replace(markerName, (int)Position);
        }
    }
}
