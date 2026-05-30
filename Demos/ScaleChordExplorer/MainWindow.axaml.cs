using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Composing;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.MusicTheory;
using Note = Melanchall.DryWetMidi.MusicTheory.Note;

namespace ScaleChordExplorer;

public partial class MainWindow : Window
{
    // ── Piano layout constants ──────────────────────────────────────────────
    private const int KeyboardStartNote = 48;   // C3
    private const int KeyboardEndNote   = 71;   // B4  (2 octaves)
    private const int WhiteKeyWidth     = 44;
    private const int WhiteKeyHeight    = 120;
    private const int BlackKeyWidth     = 28;
    private const int BlackKeyHeight    = 76;
    private const int OctaveWidth       = WhiteKeyWidth * 7; // 308

    // Semitone-in-octave → is black key?
    private static readonly bool[] IsBlack =
    {
        false, true, false, true, false, false,
        true,  false, true,  false, true,  false
    };

    // White-key position within octave (semitone → white-key index 0..6)
    private static readonly int[] SemitoneToWhiteIndex = { 0, -1, 1, -1, 2, 3, -1, 4, -1, 5, -1, 6 };

    // Pixel offset of each black key within an octave (from octave x start)
    private static readonly int[] BlackKeyX = { 30, 74, 162, 206, 250 };

    // Black keys by semitone within octave (C#=1,D#=3,F#=6,G#=8,A#=10)
    private static readonly int[] BlackSemitones = { 1, 3, 6, 8, 10 };

    // ── Scale data ──────────────────────────────────────────────────────────
    private static readonly (string Name, IEnumerable<Interval> Intervals)[] Scales =
    {
        ("Major (Ionian)",           ScaleIntervals.Ionian),
        ("Natural Minor (Aeolian)",  ScaleIntervals.Aeolian),
        ("Harmonic Minor",           ScaleIntervals.HarmonicMinor),
        ("Melodic Minor",            ScaleIntervals.MelodicMinor),
        ("Dorian",                   ScaleIntervals.Dorian),
        ("Phrygian",                 ScaleIntervals.Phrygian),
        ("Lydian",                   ScaleIntervals.Lydian),
        ("Mixolydian",               ScaleIntervals.Mixolydian),
        ("Major Pentatonic",         ScaleIntervals.MajorPentatonic),
        ("Minor Pentatonic",         ScaleIntervals.MinorPentatonic),
        ("Blues",                    ScaleIntervals.Blues),
        ("Chromatic",                ScaleIntervals.Chromatic),
    };

    // ── State ───────────────────────────────────────────────────────────────
    private Scale _currentScale = new Scale(ScaleIntervals.Ionian, NoteName.C);
    private readonly Dictionary<int, Border> _keyBorders = new();   // midiNote → border
    private readonly HashSet<int> _inScaleKeys    = new();
    private readonly HashSet<int> _playingKeys    = new();
    private OutputDevice? _outputDevice;
    private Playback? _activePlayback;
    private readonly object _playbackLock = new();

    // ── Colors ──────────────────────────────────────────────────────────────
    private static readonly IBrush BrushWhiteDefault  = new SolidColorBrush(Color.Parse("#F0F0F0"));
    private static readonly IBrush BrushWhiteInScale  = new SolidColorBrush(Color.Parse("#ADE6FF"));
    private static readonly IBrush BrushWhitePlaying  = new SolidColorBrush(Color.Parse("#FFD600"));
    private static readonly IBrush BrushBlackDefault  = new SolidColorBrush(Color.Parse("#1A1A2E"));
    private static readonly IBrush BrushBlackInScale  = new SolidColorBrush(Color.Parse("#0D47A1"));
    private static readonly IBrush BrushBlackPlaying  = new SolidColorBrush(Color.Parse("#FF9800"));

    // ── Constructor ─────────────────────────────────────────────────────────
    public MainWindow()
    {
        InitializeComponent();
        Loaded    += OnLoaded;
        Closed    += OnClosed;
    }

    // ── Initialisation ──────────────────────────────────────────────────────
    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        PopulateRootComboBox();
        PopulateScaleComboBox();
        RefreshMidiDevices();
        BuildPianoKeyboard();
        UpdateScaleHighlight();
        WireSliders();

        // Show library version
        var asm = typeof(Scale).Assembly;
        var v   = asm.GetName().Version;
        TxtVersion.Text = v is null ? "DryWetMidi" : $"DryWetMidi v{v.Major}.{v.Minor}.{v.Build}";
    }

    private void PopulateRootComboBox()
    {
        foreach (NoteName n in Enum.GetValues<NoteName>())
            CmbRoot.Items.Add(n.ToString().Replace("Sharp", "♯"));
        CmbRoot.SelectedIndex = 0;
        CmbRoot.SelectionChanged += (_, _) => OnScaleChanged();
    }

    private void PopulateScaleComboBox()
    {
        foreach (var s in Scales)
            CmbScale.Items.Add(s.Name);
        CmbScale.SelectedIndex = 0;
        CmbScale.SelectionChanged += (_, _) => OnScaleChanged();
    }

    private void WireSliders()
    {
        SliderVelocity.ValueChanged += (_, e) => TxtVelocity.Text = ((int)e.NewValue).ToString();
        SliderBpm.ValueChanged      += (_, e) => TxtBpm.Text      = ((int)e.NewValue).ToString();
        SliderNoteMs.ValueChanged   += (_, e) => TxtNoteMs.Text   = ((int)e.NewValue).ToString();
    }

    private void RefreshMidiDevices()
    {
        var prev = (string?)CmbMidiOut.SelectedItem;
        CmbMidiOut.Items.Clear();
        CmbMidiOut.Items.Add("(No output — silent)");

        try
        {
            foreach (var ep in OutputDevice.GetAll())
                CmbMidiOut.Items.Add(ep.Name);
        }
        catch
        {
            // MIDI not available on this system
        }

        // Try restore previous selection
        int idx = 0;
        if (prev is not null)
        {
            for (int i = 1; i < CmbMidiOut.Items.Count; i++)
                if ((string?)CmbMidiOut.Items[i] == prev) { idx = i; break; }
        }
        else if (CmbMidiOut.Items.Count > 1)
        {
            idx = 1; // auto-select first real device
        }
        CmbMidiOut.SelectedIndex = idx;
    }

    // ── Piano keyboard building ──────────────────────────────────────────────
    private void BuildPianoKeyboard()
    {
        PianoCanvas.Children.Clear();
        _keyBorders.Clear();

        var whiteKeys = new List<(int midiNote, double x)>();
        var blackKeys = new List<(int midiNote, double x)>();

        // Compute position for every key in range [KeyboardStartNote, KeyboardEndNote]
        for (int midi = KeyboardStartNote; midi <= KeyboardEndNote; midi++)
        {
            int octave  = midi / 12;
            int semi    = midi % 12;
            double octX = (octave - (KeyboardStartNote / 12)) * OctaveWidth;

            if (IsBlack[semi])
            {
                int bIdx = Array.IndexOf(BlackSemitones, semi);
                double x = octX + BlackKeyX[bIdx];
                blackKeys.Add((midi, x));
            }
            else
            {
                int wIdx = SemitoneToWhiteIndex[semi];
                double x = octX + wIdx * WhiteKeyWidth;
                whiteKeys.Add((midi, x));
            }
        }

        double totalWidth = ((KeyboardEndNote / 12) - (KeyboardStartNote / 12) + 1) * OctaveWidth;
        PianoCanvas.Width = totalWidth;

        // Add white keys first (z-order below black)
        foreach (var (midi, x) in whiteKeys)
            AddKey(midi, x, WhiteKeyWidth, WhiteKeyHeight, false);

        // Add black keys on top
        foreach (var (midi, x) in blackKeys)
            AddKey(midi, x, BlackKeyWidth, BlackKeyHeight, true);
    }

    private void AddKey(int midi, double x, double w, double h, bool isBlack)
    {
        var octave = midi / 12 - 1;   // MIDI 60 = C4 in standard notation
        var semi   = midi % 12;
        var noteName = ((NoteName)semi).ToString().Replace("Sharp", "♯");

        // Label only C notes on white keys
        TextBlock? label = null;
        if (!isBlack && semi == 0)
        {
            label = new TextBlock
            {
                Text       = $"C{octave}",
                FontSize   = 9,
                Foreground = Brushes.Gray,
                HorizontalAlignment = HorizontalAlignment.Center,
            };
        }

        var border = new Border
        {
            Width           = w,
            Height          = h,
            Background      = isBlack ? BrushBlackDefault : BrushWhiteDefault,
            BorderBrush     = new SolidColorBrush(Color.Parse("#444466")),
            BorderThickness = new Thickness(1),
            CornerRadius    = new CornerRadius(0, 0, 4, 4),
            Cursor          = new Cursor(StandardCursorType.Hand),
            Child           = label,
        };

        // Align label to bottom of key
        if (label is not null)
        {
            border.Child = new Panel
            {
                Children =
                {
                    new Border { VerticalAlignment = VerticalAlignment.Bottom, Padding = new Thickness(0,0,0,4),
                                 Child = label }
                }
            };
        }

        Canvas.SetLeft(border, x);
        Canvas.SetTop(border, 0);

        if (isBlack)
            border.ZIndex = 2;

        border.PointerPressed += (_, pe) => OnKeyPressed(midi, pe);

        PianoCanvas.Children.Add(border);
        _keyBorders[midi] = border;
    }

    // ── Scale update ────────────────────────────────────────────────────────
    private void OnScaleChanged()
    {
        StopPlayback();
        var root  = (NoteName)CmbRoot.SelectedIndex;
        var idx   = CmbScale.SelectedIndex;
        if (idx < 0 || idx >= Scales.Length) return;
        _currentScale = new Scale(Scales[idx].Intervals, root);
        UpdateScaleHighlight();
    }

    private void UpdateScaleHighlight()
    {
        _inScaleKeys.Clear();

        // Collect which MIDI notes in our keyboard range are in the scale
        var scaleNotes = _currentScale
            .GetNotes()
            .SkipWhile(n => n.NoteNumber < KeyboardStartNote)
            .TakeWhile(n => n.NoteNumber <= KeyboardEndNote)
            .ToList();

        foreach (var n in scaleNotes)
            _inScaleKeys.Add((int)(byte)n.NoteNumber);

        // Repaint all keys
        foreach (var (midi, border) in _keyBorders)
            SetKeyColor(midi, border);

        // Update text
        var names = scaleNotes
            .Select(n => n.NoteName.ToString().Replace("Sharp", "♯") + n.Octave)
            .ToList();
        TxtScaleNotes.Text = names.Count == 0 ? "(none in range)" : string.Join("  ", names);
        SetStatus($"Scale updated: {_currentScale}  ({names.Count} notes in keyboard range).");
    }

    // ── Key color helper ────────────────────────────────────────────────────
    private void SetKeyColor(int midi, Border border)
    {
        bool black    = IsBlack[midi % 12];
        bool inScale  = _inScaleKeys.Contains(midi);
        bool playing  = _playingKeys.Contains(midi);

        border.Background = (black, inScale, playing) switch
        {
            (_, _, true)       => black ? BrushBlackPlaying : BrushWhitePlaying,
            (true, true, _)    => BrushBlackInScale,
            (true, false, _)   => BrushBlackDefault,
            (false, true, _)   => BrushWhiteInScale,
            (false, false, _)  => BrushWhiteDefault,
        };
    }

    // Simplified: playing beats everything else
    private void SetKeyColorSimple(int midi)
    {
        if (!_keyBorders.TryGetValue(midi, out var border)) return;
        bool black   = IsBlack[midi % 12];
        bool inScale = _inScaleKeys.Contains(midi);
        bool playing = _playingKeys.Contains(midi);

        border.Background = playing
            ? (black ? BrushBlackPlaying : BrushWhitePlaying)
            : inScale
                ? (black ? BrushBlackInScale : BrushWhiteInScale)
                : (black ? BrushBlackDefault : BrushWhiteDefault);
    }

    // ── Interactive key press ────────────────────────────────────────────────
    private async void OnKeyPressed(int midi, PointerPressedEventArgs e)
    {
        e.Handled = true;
        var device = EnsureOutputDevice();
        if (device is null)
        {
            SetStatus("No MIDI output selected — key presses are silent.");
            return;
        }

        int velocity = (int)SliderVelocity.Value;
        int noteMs   = (int)SliderNoteMs.Value;
        int semi     = midi % 12;
        int octave   = midi / 12 - 1;
        string name  = ((NoteName)semi).ToString().Replace("Sharp", "♯");
        SetStatus($"Pressed: {name}{octave}  (MIDI {midi})");

        // Light up key
        _playingKeys.Add(midi);
        SetKeyColorSimple(midi);
        TxtPlayingNote.Text = $"{name}{octave}";

        try
        {
            device.SendEvent(new NoteOnEvent((SevenBitNumber)midi, (SevenBitNumber)velocity));
            await Task.Delay(noteMs);
            device.SendEvent(new NoteOffEvent((SevenBitNumber)midi, SevenBitNumber.MinValue));
        }
        catch (Exception ex)
        {
            SetStatus($"MIDI error: {ex.Message}");
        }
        finally
        {
            _playingKeys.Remove(midi);
            SetKeyColorSimple(midi);
            TxtPlayingNote.Text = "—";
        }
    }

    // ── Playback buttons ─────────────────────────────────────────────────────
    private void BtnPlayAscending_Click(object? sender, RoutedEventArgs e)   => StartScalePlayback(descending: false);
    private void BtnPlayBoth_Click(object? sender, RoutedEventArgs e)        => StartScalePlayback(descending: true);
    private void BtnPlayProgression_Click(object? sender, RoutedEventArgs e) => StartProgressionPlayback();
    private void BtnStop_Click(object? sender, RoutedEventArgs e)            => StopPlayback();
    private void BtnRefreshDevices_Click(object? sender, RoutedEventArgs e)  => RefreshMidiDevices();

    // ── Scale Playback ───────────────────────────────────────────────────────
    private void StartScalePlayback(bool descending)
    {
        StopPlayback();
        var device = EnsureOutputDevice();
        if (device is null)
        {
            SetStatus("No MIDI output selected — cannot play back.");
            return;
        }

        int bpm      = (int)SliderBpm.Value;
        int velocity = (int)SliderVelocity.Value;

        // Collect scale notes in keyboard range
        var notes = _currentScale
            .GetNotes()
            .SkipWhile(n => n.NoteNumber < KeyboardStartNote)
            .TakeWhile(n => n.NoteNumber <= KeyboardEndNote)
            .ToList();

        if (notes.Count == 0)
        {
            SetStatus("No scale notes in the keyboard range for this scale.");
            return;
        }

        var allNotes = descending
            ? notes.Concat(Enumerable.Reverse(notes).Skip(1)).ToList()
            : notes;

        // Build pattern: one note per beat
        var pb = new PatternBuilder().SetVelocity((SevenBitNumber)velocity);
        foreach (var n in allNotes)
            pb.Note(n, MusicalTimeSpan.Quarter);
        var pattern  = pb.Build();
        var tempoMap = TempoMap.Create(Tempo.FromBeatsPerMinute(bpm));

        Playback playback;
        try
        {
            playback = pattern.GetPlayback(tempoMap, FourBitNumber.MinValue, device);
        }
        catch (Exception ex)
        {
            SetStatus($"Playback error: {ex.Message}");
            return;
        }

        AttachPlaybackEvents(playback);

        lock (_playbackLock) { _activePlayback = playback; }
        playback.Start();
        SetStatus($"Playing scale {_currentScale}{(descending ? " ascending & descending" : " ascending")} at {bpm} BPM…");
    }

    // ── Chord Progression Playback ───────────────────────────────────────────
    private void StartProgressionPlayback()
    {
        StopPlayback();
        var device = EnsureOutputDevice();
        if (device is null)
        {
            SetStatus("No MIDI output selected — cannot play back.");
            return;
        }

        int bpm      = (int)SliderBpm.Value;
        int velocity = (int)SliderVelocity.Value;

        // Get enough scale notes for three chords (need indices 0–8)
        var scaleNotes = _currentScale
            .GetNotes()
            .SkipWhile(n => n.NoteNumber < KeyboardStartNote)
            .TakeWhile(n => n.NoteNumber <= KeyboardEndNote + 12) // allow a little above visible range
            .Take(12)
            .ToArray();

        if (scaleNotes.Length < 5)
        {
            SetStatus("Not enough scale notes in range to build a chord progression.");
            return;
        }

        // Helper: safely pick a note by index (clamps if not enough notes)
        Note SafeNote(int i) => scaleNotes[Math.Min(i, scaleNotes.Length - 1)];

        // I – IV – V – I  (root position triads using scale degrees)
        var chordI  = new[] { SafeNote(0), SafeNote(2), SafeNote(4) };
        var chordIV = new[] { SafeNote(3), SafeNote(5), SafeNote(7) };
        // For pentatonic scales (only 5/6 notes) just build dyads
        var chordV  = scaleNotes.Length >= 9
            ? new[] { SafeNote(4), SafeNote(6), SafeNote(8) }
            : new[] { SafeNote(4), SafeNote(Math.Min(6, scaleNotes.Length - 1)) };

        // Name the chords for the status bar
        string NameChord(Note[] ch) =>
            string.Join("-", ch.Select(n => n.NoteName.ToString().Replace("Sharp", "♯") + n.Octave));

        var pattern = new PatternBuilder()
            .SetVelocity((SevenBitNumber)velocity)
            .SetNoteLength(MusicalTimeSpan.Whole)
            .Chord(chordI)
            .Chord(chordIV)
            .Chord(chordV)
            .Chord(chordI)
            .Build();

        var tempoMap = TempoMap.Create(Tempo.FromBeatsPerMinute(bpm));

        Playback playback;
        try
        {
            playback = pattern.GetPlayback(tempoMap, FourBitNumber.MinValue, device);
        }
        catch (Exception ex)
        {
            SetStatus($"Playback error: {ex.Message}");
            return;
        }

        AttachPlaybackEvents(playback);

        lock (_playbackLock) { _activePlayback = playback; }
        playback.Start();

        SetStatus($"Playing I-IV-V-I progression in {_currentScale}:  " +
                  $"I={NameChord(chordI)}  IV={NameChord(chordIV)}  V={NameChord(chordV)}  at {bpm} BPM…");
    }

    // ── Playback helpers ─────────────────────────────────────────────────────
    private void AttachPlaybackEvents(Playback pb)
    {
        pb.NotesPlaybackStarted += (_, args) =>
        {
            var midiNotes = args.Notes
                .Select(n => (int)(byte)n.NoteNumber)
                .Where(m => m >= KeyboardStartNote && m <= KeyboardEndNote)
                .ToList();

            Dispatcher.UIThread.Post(() =>
            {
                foreach (var m in midiNotes)
                {
                    _playingKeys.Add(m);
                    SetKeyColorSimple(m);
                }
                TxtPlayingNote.Text = midiNotes.Count == 0 ? "—"
                    : string.Join(" ", midiNotes.Select(m =>
                        ((NoteName)(m % 12)).ToString().Replace("Sharp", "♯") + (m / 12 - 1)));
            });
        };

        pb.NotesPlaybackFinished += (_, args) =>
        {
            var midiNotes = args.Notes
                .Select(n => (int)(byte)n.NoteNumber)
                .Where(m => m >= KeyboardStartNote && m <= KeyboardEndNote)
                .ToList();

            Dispatcher.UIThread.Post(() =>
            {
                foreach (var m in midiNotes)
                {
                    _playingKeys.Remove(m);
                    SetKeyColorSimple(m);
                }
                TxtPlayingNote.Text = "—";
            });
        };

        pb.Finished += (_, _) =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                _playingKeys.Clear();
                foreach (var (midi, border) in _keyBorders)
                    SetKeyColorSimple(midi);
                TxtPlayingNote.Text = "—";
                SetStatus("Playback finished.");
            });
            lock (_playbackLock)
            {
                if (ReferenceEquals(_activePlayback, pb))
                    _activePlayback = null;
            }
        };
    }

    private void StopPlayback()
    {
        Playback? pb;
        lock (_playbackLock)
        {
            pb = _activePlayback;
            _activePlayback = null;
        }

        if (pb is null) return;

        try { pb.Stop(); } catch { /* ignore */ }

        // Flush any visually "playing" keys back to their resting colour
        Dispatcher.UIThread.Post(() =>
        {
            _playingKeys.Clear();
            foreach (var (midi, _) in _keyBorders)
                SetKeyColorSimple(midi);
            TxtPlayingNote.Text = "—";
        });

        try { pb.Dispose(); } catch { /* ignore */ }
    }

    // ── MIDI device management ───────────────────────────────────────────────
    private OutputDevice? EnsureOutputDevice()
    {
        int idx = CmbMidiOut.SelectedIndex;
        if (idx <= 0) return null;  // index 0 == "(No output — silent)"

        string? name = CmbMidiOut.Items[idx] as string;
        if (name is null) return null;

        if (_outputDevice?.Name == name && _outputDevice is not null)
            return _outputDevice;

        _outputDevice?.Dispose();
        _outputDevice = null;

        try
        {
            _outputDevice = OutputDevice.GetAll().FirstOrDefault(ep => ep.Name == name);
        }
        catch (Exception ex)
        {
            SetStatus($"Could not open MIDI device \"{name}\": {ex.Message}");
        }

        return _outputDevice;
    }

    // ── Helpers ──────────────────────────────────────────────────────────────
    private void SetStatus(string msg) =>
        TxtStatus.Text = msg;

    private void OnClosed(object? sender, EventArgs e)
    {
        StopPlayback();
        _outputDevice?.Dispose();
    }
}
