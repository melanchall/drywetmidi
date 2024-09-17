using Melanchall.DryWetMidi.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Composing
{
    /// <summary>
    /// Settings to control handling of a piano roll be the
    /// <see cref="PatternBuilder.PianoRoll(string, PianoRollSettings)"/> method. More info in the
    /// <see href="xref:a_composing_pattern#customization">Pattern: Piano roll: Customization</see> article.
    /// </summary>
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

        private IEnumerable<PianoRollAction> _customActions;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a symbol which means a single-cell note. The default value
        /// is <c>'|'</c>.
        /// </summary>
        /// <exception cref="ArgumentException">Space (' ') is the prohibted character.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// One of the following errors occurred:
        /// <list type="bullet">
        /// <item>
        /// <description>The same symbol defined by the <see cref="MultiCellNoteStartSymbol"/> property.</description>
        /// </item>
        /// <item>
        /// <description>The same symbol defined by the <see cref="MultiCellNoteEndSymbol"/> property.</description>
        /// </item>
        /// <item>
        /// <description>The symbol used for a custom action wuthin the <see cref="CustomActions"/>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public char SingleCellNoteSymbol
        {
            get { return _singleCellNoteSymbol; }
            set
            {
                ThrowIfArgument.IsProhibitedValue(nameof(value), value, ProhibitedSymbol);

                ThrowIfArgument.DoesntSatisfyCondition(
                    nameof(value),
                    value,
                    v => v != MultiCellNoteStartSymbol,
                    $"The same symbol defined by the {nameof(MultiCellNoteStartSymbol)} property.");

                ThrowIfArgument.DoesntSatisfyCondition(
                    nameof(value),
                    value,
                    v => v != MultiCellNoteEndSymbol,
                    $"The same symbol defined by the {nameof(MultiCellNoteEndSymbol)} property.");

                ThrowIfArgument.DoesntSatisfyCondition(
                    nameof(value),
                    value,
                    v => CustomActions?.Any(a => a.StartSymbol == v || a.EndSymbol == v) != true,
                    $"The symbol used for a custom action within the {nameof(CustomActions)}.");

                _singleCellNoteSymbol = value;
            }
        }

        /// <summary>
        /// Gets or sets a symbol which means the start of a multi-cell note. The default value
        /// is <c>'['</c>.
        /// </summary>
        /// <exception cref="ArgumentException">Space (' ') is the prohibted character.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// One of the following errors occurred:
        /// <list type="bullet">
        /// <item>
        /// <description>The same symbol defined by the <see cref="SingleCellNoteSymbol"/> property.</description>
        /// </item>
        /// <item>
        /// <description>The same symbol defined by the <see cref="MultiCellNoteEndSymbol"/> property.</description>
        /// </item>
        /// <item>
        /// <description>The symbol used for a custom action wuthin the <see cref="CustomActions"/>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public char MultiCellNoteStartSymbol
        {
            get { return _multiCellNoteStartSymbol; }
            set
            {
                ThrowIfArgument.IsProhibitedValue(nameof(value), value, ProhibitedSymbol);

                ThrowIfArgument.DoesntSatisfyCondition(
                    nameof(value),
                    value,
                    v => v != SingleCellNoteSymbol,
                    $"The same symbol defined by the {nameof(SingleCellNoteSymbol)} property.");

                ThrowIfArgument.DoesntSatisfyCondition(
                    nameof(value),
                    value,
                    v => v != MultiCellNoteEndSymbol,
                    $"The same symbol defined by the {nameof(MultiCellNoteEndSymbol)} property.");

                ThrowIfArgument.DoesntSatisfyCondition(
                    nameof(value),
                    value,
                    v => CustomActions?.Any(a => a.StartSymbol == v || a.EndSymbol == v) != true,
                    $"The symbol used for a custom action within the {nameof(CustomActions)}.");

                _multiCellNoteStartSymbol = value;
            }
        }

        /// <summary>
        /// Gets or sets a symbol which means the end of a multi-cell note. The default value
        /// is <c>']'</c>.
        /// </summary>
        /// <exception cref="ArgumentException">Space (' ') is the prohibted character.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// One of the following errors occurred:
        /// <list type="bullet">
        /// <item>
        /// <description>The same symbol defined by the <see cref="SingleCellNoteSymbol"/> property.</description>
        /// </item>
        /// <item>
        /// <description>The same symbol defined by the <see cref="MultiCellNoteStartSymbol"/> property.</description>
        /// </item>
        /// <item>
        /// <description>The symbol used for a custom action wuthin the <see cref="CustomActions"/>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public char MultiCellNoteEndSymbol
        {
            get { return _multiCellNoteEndSymbol; }
            set
            {
                ThrowIfArgument.IsProhibitedValue(nameof(value), value, ProhibitedSymbol);

                ThrowIfArgument.DoesntSatisfyCondition(
                    nameof(value),
                    value,
                    v => v != SingleCellNoteSymbol,
                    $"The same symbol defined by the {nameof(SingleCellNoteSymbol)} property.");

                ThrowIfArgument.DoesntSatisfyCondition(
                    nameof(value),
                    value,
                    v => v != MultiCellNoteStartSymbol,
                    $"The same symbol defined by the {nameof(MultiCellNoteStartSymbol)} property.");

                ThrowIfArgument.DoesntSatisfyCondition(
                    nameof(value),
                    value,
                    v => CustomActions?.Any(a => a.StartSymbol == v || a.EndSymbol == v) != true,
                    $"The symbol used for a custom action within the {nameof(CustomActions)}.");

                _multiCellNoteEndSymbol = value;
            }
        }

        /// <summary>
        /// Gets or sets a dictionary which maps specified symbols to custom actions.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// One of the following errors occurred:
        /// <list type="bullet">
        /// <item>
        /// <description>Actions keys contain the space (' ') symbol which is prohibited.</description>
        /// </item>
        /// <item>
        /// <description>Actions keys contain the symbol defined by the <see cref="SingleCellNoteSymbol"/>
        /// property which is prohibited.</description>
        /// </item>
        /// <item>
        /// <description>Actions keys contain the symbol defined by the <see cref="MultiCellNoteStartSymbol"/>
        /// property which is prohibited.</description>
        /// </item>
        /// <item>
        /// <description>Actions keys contain the symbol defined by the <see cref="MultiCellNoteEndSymbol"/>
        /// property which is prohibited.</description>
        /// </item>
        /// </list>
        /// </exception>
        public IEnumerable<PianoRollAction> CustomActions
        {
            get { return _customActions; }
            set
            {
                if (value != null)
                {
                    ThrowIfArgument.DoesntSatisfyCondition(
                        nameof(value),
                        value,
                        v => v?.Any(a => a.StartSymbol == ProhibitedSymbol) != true,
                        $"Actions symbols contain the space (' ') symbol which is prohibited.");

                    ThrowIfArgument.DoesntSatisfyCondition(
                        nameof(value),
                        value,
                        v => v?.Any(a => a.StartSymbol == SingleCellNoteSymbol) != true,
                        $"Actions symbols contain the symbol defined by the {nameof(SingleCellNoteSymbol)} ('{SingleCellNoteSymbol}') property which is prohibited.");

                    ThrowIfArgument.DoesntSatisfyCondition(
                        nameof(value),
                        value,
                        v => v?.Any(a => a.StartSymbol == MultiCellNoteStartSymbol) != true,
                        $"Actions symbols contain the symbol defined by the {nameof(MultiCellNoteStartSymbol)} ('{MultiCellNoteStartSymbol}') property which is prohibited.");

                    ThrowIfArgument.DoesntSatisfyCondition(
                        nameof(value),
                        value,
                        v => v?.Any(a => a.StartSymbol == MultiCellNoteEndSymbol) != true,
                        $"Actions symbols contain the symbol defined by the {nameof(MultiCellNoteEndSymbol)} ('{MultiCellNoteEndSymbol}') property which is prohibited.");
                }

                _customActions = value;
            }
        }

        #endregion
    }
}
