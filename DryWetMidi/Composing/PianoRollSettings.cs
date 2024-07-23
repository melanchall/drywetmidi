using Melanchall.DryWetMidi.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Composing
{
    public sealed class PianoRollSettings
    {
        #region Constants

        private const char ProhibitedSymbol = ' ';

        private const char DefaultSingleCellNoteSymbol = '|';
        private const char DefaultMultiCellNoteStartSymbol = '[';
        private const char DefaultMultiCellNoteEndSymbol = ']';

        #endregion

        #region Fields

        private char _singleCellNoteSymbol = DefaultSingleCellNoteSymbol;
        private char _multiCellNoteStartSymbol = DefaultMultiCellNoteStartSymbol;
        private char _multiCellNoteEndSymbol = DefaultMultiCellNoteEndSymbol;

        private Dictionary<char, Action<MusicTheory.Note, PatternBuilder>> _customActions;

        #endregion

        #region Properties

        public char SingleCellNoteSymbol
        {
            get { return _singleCellNoteSymbol; }
            set
            {
                ThrowIfArgument.IsProhibitedValue(nameof(value), value, ProhibitedSymbol);

                _singleCellNoteSymbol = value;
            }
        }

        public char MultiCellNoteStartSymbol
        {
            get { return _multiCellNoteStartSymbol; }
            set
            {
                ThrowIfArgument.IsProhibitedValue(nameof(value), value, ProhibitedSymbol);

                _multiCellNoteStartSymbol = value;
            }
        }

        public char MultiCellNoteEndSymbol
        {
            get { return _multiCellNoteEndSymbol; }
            set
            {
                ThrowIfArgument.IsProhibitedValue(nameof(value), value, ProhibitedSymbol);

                _multiCellNoteEndSymbol = value;
            }
        }

        public Dictionary<char, Action<MusicTheory.Note, PatternBuilder>> CustomActions
        {
            get { return _customActions; }
            set
            {
                if (value != null)
                {
                    ThrowIfArgument.DoesntSatisfyCondition(
                        nameof(value),
                        value,
                        v => !v.Keys.Contains(SingleCellNoteSymbol),
                        $"Actions keys contain the symbol defined by the {nameof(SingleCellNoteSymbol)} ('{SingleCellNoteSymbol}') property which is prohibited.");

                    ThrowIfArgument.DoesntSatisfyCondition(
                        nameof(value),
                        value,
                        v => !v.Keys.Contains(MultiCellNoteStartSymbol),
                        $"Actions keys contain the symbol defined by the {nameof(MultiCellNoteStartSymbol)} ('{MultiCellNoteStartSymbol}') property which is prohibited.");

                    ThrowIfArgument.DoesntSatisfyCondition(
                        nameof(value),
                        value,
                        v => !v.Keys.Contains(MultiCellNoteEndSymbol),
                        $"Actions keys contain the symbol defined by the {nameof(MultiCellNoteEndSymbol)} ('{MultiCellNoteEndSymbol}') property which is prohibited.");
                }

                _customActions = value;
            }
        }

        #endregion
    }
}
