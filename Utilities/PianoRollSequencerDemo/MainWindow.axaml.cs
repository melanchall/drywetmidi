using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Multimedia;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.PianoRollSequencerDemo;

public partial class MainWindow : Window
{
    private const int TicksPerBeat = 480;
    private const double PixelsPerBeat = 64;
    private const double RowHeight = 18;
    private const int LowestNoteNumber = 48;
    private const int HighestNoteNumber = 84;
    private const int VisibleBeats = 48;
    private const long DefaultNoteLength = TicksPerBeat;

    private static readonly GridStepOption[] GridStepOptions =
    [
        new("1/4", TicksPerBeat),
        new("1/8", TicksPerBeat / 2),
        new("1/16", TicksPerBeat / 4),
        new("1/32", TicksPerBeat / 8)
    ];

    private static readonly TimeFormatOption[] TimeFormatOptions =
    [
        new("Metric", TimeSpanType.Metric),
        new("Musical", TimeSpanType.Musical),
        new("Bar/Beat/Ticks", TimeSpanType.BarBeatTicks),
        new("Bar/Beat/Fraction", TimeSpanType.BarBeatFraction),
        new("MIDI", TimeSpanType.Midi)
    ];

    private static readonly TimeSignatureOption[] TimeSignatureOptions =
    [
        new("4/4", 4, 4),
        new("3/4", 3, 4),
        new("5/8", 5, 8),
        new("6/8", 6, 8)
    ];

    private readonly ObservableTimedObjectsCollection _collection = [];
    private readonly Dictionary<Note, Border> _noteViews = [];
    private readonly DispatcherTimer _uiTimer;

    private long _gridStepTicks = TicksPerBeat / 4;
    private TimeFormatOption _selectedTimeFormat = TimeFormatOptions[2];
    private TimeSignatureOption _selectedTimeSignature = TimeSignatureOptions[0];
    private TempoMap _tempoMap;

    private Playback? _playback;
    private OutputEndpoint? _outputEndpoint;

    private Canvas _keysCanvas = null!;
    private Canvas _gridCanvas = null!;
    private Canvas _notesCanvas = null!;
    private TextBlock _statusText = null!;
    private TextBlock _currentTimeText = null!;

    private Line _playhead = null!;

    private Note? _draggedNote;
    private Point _dragStartPoint;
    private long _dragOriginalTime;
    private int _dragOriginalNoteNumber;

    private bool _isDrawing;
    private Point _drawStartPoint;
    private Border? _drawPreview;

    public MainWindow()
    {
        InitializeComponent();

        _tempoMap = TempoMap.Create(
            new TicksPerQuarterNoteTimeDivision(TicksPerBeat),
            new TimeSignature(_selectedTimeSignature.Numerator, _selectedTimeSignature.Denominator));

        _uiTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(33)
        };

        _uiTimer.Tick += (_, _) => UpdatePlaybackVisuals();

        InitializeControls();
        InitializeGrid();
        InitializePlayback();
        SeedNotes();
        UpdatePlaybackVisuals();
        UpdateStatus();
    }

    private void InitializeControls()
    {
        _keysCanvas = this.FindControl<Canvas>("KeysCanvas")
            ?? throw new InvalidOperationException("KeysCanvas is not found.");
        _gridCanvas = this.FindControl<Canvas>("GridCanvas")
            ?? throw new InvalidOperationException("GridCanvas is not found.");
        _notesCanvas = this.FindControl<Canvas>("NotesCanvas")
            ?? throw new InvalidOperationException("NotesCanvas is not found.");
        _statusText = this.FindControl<TextBlock>("StatusText")
            ?? throw new InvalidOperationException("StatusText is not found.");
        _currentTimeText = this.FindControl<TextBlock>("CurrentTimeText")
            ?? throw new InvalidOperationException("CurrentTimeText is not found.");

        var playButton = this.FindControl<Button>("PlayButton")
            ?? throw new InvalidOperationException("PlayButton is not found.");
        var stopButton = this.FindControl<Button>("StopButton")
            ?? throw new InvalidOperationException("StopButton is not found.");
        var resetButton = this.FindControl<Button>("ResetButton")
            ?? throw new InvalidOperationException("ResetButton is not found.");
        var clearButton = this.FindControl<Button>("ClearButton")
            ?? throw new InvalidOperationException("ClearButton is not found.");

        var gridStepComboBox = this.FindControl<ComboBox>("GridStepComboBox")
            ?? throw new InvalidOperationException("GridStepComboBox is not found.");
        var timeFormatComboBox = this.FindControl<ComboBox>("TimeFormatComboBox")
            ?? throw new InvalidOperationException("TimeFormatComboBox is not found.");
        var timeSignatureComboBox = this.FindControl<ComboBox>("TimeSignatureComboBox")
            ?? throw new InvalidOperationException("TimeSignatureComboBox is not found.");

        playButton.Click += (_, _) => StartPlayback();
        stopButton.Click += (_, _) => StopPlayback();
        resetButton.Click += (_, _) => ResetPlaybackPosition();
        clearButton.Click += (_, _) => ClearNotes();

        gridStepComboBox.ItemsSource = GridStepOptions.Select(option => option.Name).ToArray();
        gridStepComboBox.SelectedIndex = Array.FindIndex(GridStepOptions, option => option.Ticks == _gridStepTicks);
        gridStepComboBox.SelectionChanged += (_, _) => OnGridStepChanged(gridStepComboBox.SelectedIndex);

        timeFormatComboBox.ItemsSource = TimeFormatOptions.Select(option => option.Name).ToArray();
        timeFormatComboBox.SelectedIndex = Array.FindIndex(TimeFormatOptions, option => option.Type == _selectedTimeFormat.Type);
        timeFormatComboBox.SelectionChanged += (_, _) => OnTimeFormatChanged(timeFormatComboBox.SelectedIndex);

        timeSignatureComboBox.ItemsSource = TimeSignatureOptions.Select(option => option.Name).ToArray();
        timeSignatureComboBox.SelectedIndex = Array.FindIndex(TimeSignatureOptions, option => option.Name == _selectedTimeSignature.Name);
        timeSignatureComboBox.SelectionChanged += (_, _) => OnTimeSignatureChanged(timeSignatureComboBox.SelectedIndex);

        _notesCanvas.PointerPressed += NotesCanvasOnPointerPressed;
        _notesCanvas.PointerMoved += NotesCanvasOnPointerMoved;
        _notesCanvas.PointerReleased += NotesCanvasOnPointerReleased;
    }

    private void InitializeGrid()
    {
        var totalRows = HighestNoteNumber - LowestNoteNumber + 1;
        var width = VisibleBeats * PixelsPerBeat;
        var height = totalRows * RowHeight;

        _keysCanvas.Width = 96;
        _keysCanvas.Height = height;
        _gridCanvas.Width = width;
        _gridCanvas.Height = height;
        _notesCanvas.Width = width;
        _notesCanvas.Height = height;

        RedrawGrid();

        _playhead = new Line
        {
            StartPoint = new Point(0, 0),
            EndPoint = new Point(0, height),
            Stroke = new SolidColorBrush(Color.Parse("#7FEA4E")),
            StrokeThickness = 2,
            IsHitTestVisible = false,
            ZIndex = 1000
        };

        _notesCanvas.Children.Add(_playhead);
    }

    private void RedrawGrid()
    {
        var totalRows = HighestNoteNumber - LowestNoteNumber + 1;
        var width = VisibleBeats * PixelsPerBeat;

        _keysCanvas.Children.Clear();
        _gridCanvas.Children.Clear();

        DrawPianoKeys(totalRows);
        DrawBackground(totalRows, width);
        DrawGridLines(totalRows, width);
    }

    private void DrawPianoKeys(int totalRows)
    {
        for (var row = 0; row < totalRows; row++)
        {
            var noteNumber = HighestNoteNumber - row;
            var isBlack = IsBlackKey(noteNumber);

            _keysCanvas.Children.Add(new Rectangle
            {
                Width = 96,
                Height = RowHeight,
                Fill = new SolidColorBrush(Color.Parse(isBlack ? "#1B1B1B" : "#F2F2F2")),
                IsHitTestVisible = false
            });
            Canvas.SetTop(_keysCanvas.Children[^1], row * RowHeight);

            var noteLabel = GetNoteLabel(noteNumber);
            _keysCanvas.Children.Add(new TextBlock
            {
                Text = noteLabel,
                Foreground = new SolidColorBrush(Color.Parse(isBlack ? "#C5C5C5" : "#111111")),
                FontSize = 10,
                IsHitTestVisible = false
            });
            Canvas.SetTop(_keysCanvas.Children[^1], row * RowHeight + 2);
            Canvas.SetLeft(_keysCanvas.Children[^1], 6);
        }

        _keysCanvas.Children.Add(new Line
        {
            StartPoint = new Point(95, 0),
            EndPoint = new Point(95, totalRows * RowHeight),
            Stroke = new SolidColorBrush(Color.Parse("#3B3B3B")),
            StrokeThickness = 1,
            IsHitTestVisible = false
        });
    }

    private void DrawBackground(int totalRows, double width)
    {
        for (var row = 0; row < totalRows; row++)
        {
            var noteNumber = HighestNoteNumber - row;
            var isBlack = IsBlackKey(noteNumber);

            _gridCanvas.Children.Add(new Rectangle
            {
                Width = width,
                Height = RowHeight,
                Fill = new SolidColorBrush(Color.Parse(isBlack ? "#202020" : "#262626")),
                IsHitTestVisible = false
            });

            Canvas.SetTop(_gridCanvas.Children[^1], row * RowHeight);
        }
    }

    private void DrawGridLines(int totalRows, double width)
    {
        for (var row = 0; row <= totalRows; row++)
        {
            _gridCanvas.Children.Add(new Line
            {
                StartPoint = new Point(0, row * RowHeight),
                EndPoint = new Point(width, row * RowHeight),
                Stroke = new SolidColorBrush(Color.Parse("#313131")),
                StrokeThickness = 1,
                IsHitTestVisible = false
            });
        }

        var totalTicks = VisibleBeats * TicksPerBeat;
        var barLengthTicks = GetBarLengthTicks();
        var beatLengthTicks = GetBeatLengthTicks();

        for (var ticks = 0L; ticks <= totalTicks; ticks += _gridStepTicks)
        {
            var x = ticks / (double)TicksPerBeat * PixelsPerBeat;
            var isBar = ticks % barLengthTicks == 0;
            var isBeat = ticks % beatLengthTicks == 0;

            _gridCanvas.Children.Add(new Line
            {
                StartPoint = new Point(x, 0),
                EndPoint = new Point(x, totalRows * RowHeight),
                Stroke = new SolidColorBrush(Color.Parse(isBar ? "#5A5A5A" : isBeat ? "#4A4A4A" : "#3A3A3A")),
                StrokeThickness = isBar ? 1.5 : 1,
                IsHitTestVisible = false
            });
        }
    }

    private void InitializePlayback(long? currentTickTime = null)
    {
        _playback?.Stop();
        _playback?.Dispose();

        try
        {
            _outputEndpoint ??= OutputEndpoint.GetAll().FirstOrDefault();
        }
        catch
        {
            _outputEndpoint = null;
        }

        _playback = _outputEndpoint != null
            ? new Playback(_collection, _tempoMap, _outputEndpoint)
            : new Playback(_collection, _tempoMap);

        _playback.Loop = true;

        if (currentTickTime.HasValue)
            _playback.MoveToTime(new MidiTimeSpan(currentTickTime.Value));
    }

    private void SeedNotes()
    {
        var seed = new[] { 60, 62, 64, 65, 67, 69, 71, 72 };

        for (var i = 0; i < seed.Length; i++)
        {
            AddNote(i * TicksPerBeat, DefaultNoteLength, seed[i]);
        }
    }

    private void StartPlayback()
    {
        _playback?.Start();
        _uiTimer.Start();
        UpdatePlaybackVisuals();
        UpdateStatus();
    }

    private void StopPlayback()
    {
        _playback?.Stop();
        _uiTimer.Stop();
        UpdatePlaybackVisuals();
        UpdateStatus();
    }

    private void ResetPlaybackPosition()
    {
        _playback?.MoveToStart();
        UpdatePlaybackVisuals();
        UpdateStatus();
    }

    private void ClearNotes()
    {
        foreach (var note in _noteViews.Keys.ToList())
        {
            _collection.Remove(note);
            RemoveNoteView(note);
        }

        UpdateStatus();
    }

    private void OnGridStepChanged(int selectedIndex)
    {
        if (selectedIndex < 0 || selectedIndex >= GridStepOptions.Length)
            return;

        _gridStepTicks = GridStepOptions[selectedIndex].Ticks;
        RedrawGrid();
        UpdatePlaybackVisuals();
        UpdateStatus();
    }

    private void OnTimeFormatChanged(int selectedIndex)
    {
        if (selectedIndex < 0 || selectedIndex >= TimeFormatOptions.Length)
            return;

        _selectedTimeFormat = TimeFormatOptions[selectedIndex];
        UpdatePlaybackVisuals();
    }

    private void OnTimeSignatureChanged(int selectedIndex)
    {
        if (selectedIndex < 0 || selectedIndex >= TimeSignatureOptions.Length)
            return;

        _selectedTimeSignature = TimeSignatureOptions[selectedIndex];

        var currentTicks = _playback?.GetCurrentTime<MidiTimeSpan>().TimeSpan;
        var wasRunning = _playback?.IsRunning == true;

        _tempoMap = TempoMap.Create(
            new TicksPerQuarterNoteTimeDivision(TicksPerBeat),
            new TimeSignature(_selectedTimeSignature.Numerator, _selectedTimeSignature.Denominator));

        InitializePlayback(currentTicks);
        RedrawGrid();
        UpdatePlaybackVisuals();
        UpdateStatus();

        if (wasRunning)
        {
            _playback?.Start();
            _uiTimer.Start();
        }
    }

    private void NotesCanvasOnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var point = e.GetPosition(_notesCanvas);

        if (e.Source is Border { Tag: Note note })
        {
            if (e.GetCurrentPoint(_notesCanvas).Properties.IsRightButtonPressed)
            {
                RemoveNote(note);
                return;
            }

            if (e.GetCurrentPoint(_notesCanvas).Properties.IsLeftButtonPressed)
            {
                _draggedNote = note;
                _dragStartPoint = point;
                _dragOriginalTime = note.Time;
                _dragOriginalNoteNumber = note.NoteNumber;
                e.Pointer.Capture(_notesCanvas);
            }

            return;
        }

        if (e.GetCurrentPoint(_notesCanvas).Properties.IsLeftButtonPressed && e.ClickCount == 2)
        {
            var noteNumber = PositionToNoteNumber(point.Y);
            var startTicks = SnapTicks(PositionToTicks(point.X));
            AddNote(startTicks, _gridStepTicks, noteNumber);
            return;
        }

        if (e.GetCurrentPoint(_notesCanvas).Properties.IsLeftButtonPressed)
        {
            _isDrawing = true;
            _drawStartPoint = point;
            EnsureDrawPreview();
            UpdateDrawPreview(point);
            e.Pointer.Capture(_notesCanvas);
        }
    }

    private void NotesCanvasOnPointerMoved(object? sender, PointerEventArgs e)
    {
        var point = e.GetPosition(_notesCanvas);

        if (_draggedNote != null)
        {
            var deltaX = point.X - _dragStartPoint.X;
            var deltaY = point.Y - _dragStartPoint.Y;

            var newTime = SnapTicks((long)Math.Round((_dragOriginalTime / (double)TicksPerBeat * PixelsPerBeat + deltaX) / PixelsPerBeat * TicksPerBeat));
            var noteNumberDelta = (int)Math.Round(deltaY / RowHeight);
            var newNoteNumber = ClampNoteNumber(_dragOriginalNoteNumber - noteNumberDelta);

            ChangeNote(_draggedNote, newTime, _draggedNote.Length, newNoteNumber);
            return;
        }

        if (_isDrawing)
            UpdateDrawPreview(point);
    }

    private void NotesCanvasOnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_draggedNote != null)
        {
            _draggedNote = null;
            e.Pointer.Capture(null);
            return;
        }

        if (!_isDrawing)
            return;

        var point = e.GetPosition(_notesCanvas);

        var startTicks = SnapTicks(PositionToTicks(_drawStartPoint.X));
        var endTicks = SnapTicks(PositionToTicks(point.X));

        if (endTicks < startTicks)
            (startTicks, endTicks) = (endTicks, startTicks);

        var length = Math.Max(_gridStepTicks, endTicks - startTicks);
        var noteNumber = PositionToNoteNumber(_drawStartPoint.Y);

        AddNote(startTicks, length, noteNumber);

        _drawPreview?.SetValue(IsVisibleProperty, false);
        _isDrawing = false;
        e.Pointer.Capture(null);
    }

    private void AddNote(long time, long length, int noteNumber)
    {
        var note = new Note((SevenBitNumber)noteNumber)
        {
            Time = Math.Max(0, time),
            Length = Math.Max(_gridStepTicks, length),
            Velocity = (SevenBitNumber)90
        };

        _collection.Add(note);
        AddNoteView(note);
        UpdateStatus();
    }

    private void RemoveNote(Note note)
    {
        _collection.Remove(note);
        RemoveNoteView(note);
        UpdateStatus();
    }

    private void ChangeNote(Note note, long time, long length, int noteNumber)
    {
        var updatedTime = Math.Max(0, time);
        var updatedLength = Math.Max(_gridStepTicks, length);
        var updatedNoteNumber = ClampNoteNumber(noteNumber);

        _collection.ChangeObject(
            note,
            _ =>
            {
                note.Time = updatedTime;
                note.Length = updatedLength;
                note.NoteNumber = (SevenBitNumber)updatedNoteNumber;
            });

        UpdateNoteView(note);
    }

    private void AddNoteView(Note note)
    {
        var view = CreateNoteView(note);
        _noteViews[note] = view;
        _notesCanvas.Children.Add(view);
        UpdateNoteView(note);
        _playhead.ZIndex = 1000;
    }

    private static Border CreateNoteView(Note note)
    {
        return new Border
        {
            Tag = note,
            Height = RowHeight - 3,
            CornerRadius = new CornerRadius(2),
            BorderBrush = new SolidColorBrush(Color.Parse("#1D6A93")),
            BorderThickness = new Thickness(1),
            Background = new SolidColorBrush(Color.Parse("#2BA6E8"))
        };
    }

    private void UpdateNoteView(Note note)
    {
        if (!_noteViews.TryGetValue(note, out var view))
            return;

        view.Width = Math.Max(10, note.Length / (double)TicksPerBeat * PixelsPerBeat - 2);
        Canvas.SetLeft(view, note.Time / (double)TicksPerBeat * PixelsPerBeat + 1);
        Canvas.SetTop(view, NoteNumberToRow(note.NoteNumber) * RowHeight + 1.5);
    }

    private void RemoveNoteView(Note note)
    {
        if (!_noteViews.Remove(note, out var view))
            return;

        _notesCanvas.Children.Remove(view);
    }

    private void EnsureDrawPreview()
    {
        if (_drawPreview != null)
            return;

        _drawPreview = new Border
        {
            Height = RowHeight - 3,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(2),
            BorderBrush = new SolidColorBrush(Color.Parse("#D7BA7D")),
            Background = new SolidColorBrush(Color.Parse("#55D7BA7D")),
            IsHitTestVisible = false
        };

        _notesCanvas.Children.Add(_drawPreview);
    }

    private void UpdateDrawPreview(Point point)
    {
        if (_drawPreview == null)
            return;

        _drawPreview.SetValue(IsVisibleProperty, true);

        var startTicks = SnapTicks(PositionToTicks(_drawStartPoint.X));
        var endTicks = SnapTicks(PositionToTicks(point.X));
        var startX = Math.Min(startTicks, endTicks) / (double)TicksPerBeat * PixelsPerBeat;
        var width = Math.Max(_gridStepTicks, Math.Abs(endTicks - startTicks)) / (double)TicksPerBeat * PixelsPerBeat;

        var noteNumber = PositionToNoteNumber(_drawStartPoint.Y);

        _drawPreview.Width = Math.Max(10, width - 2);
        Canvas.SetLeft(_drawPreview, startX + 1);
        Canvas.SetTop(_drawPreview, NoteNumberToRow(noteNumber) * RowHeight + 1.5);
    }

    private void UpdatePlaybackVisuals()
    {
        if (_playback == null)
            return;

        var currentTickTime = _playback.GetCurrentTime<MidiTimeSpan>().TimeSpan;
        var x = currentTickTime / (double)TicksPerBeat * PixelsPerBeat;

        _playhead.StartPoint = new Point(x, 0);
        _playhead.EndPoint = new Point(x, _notesCanvas.Height);

        var currentTime = _playback.GetCurrentTime(_selectedTimeFormat.Type);
        _currentTimeText.Text = currentTime.ToString();
    }

    private void UpdateStatus()
    {
        var outputEndpointName = _outputEndpoint?.Name ?? "No output endpoint (silent playback)";

        _statusText.Text = $"Notes: {_noteViews.Count} | Grid: {GetGridStepName()} | Signature: {_selectedTimeSignature.Name} | Output: {outputEndpointName}";
    }

    private string GetGridStepName()
    {
        var index = Array.FindIndex(GridStepOptions, option => option.Ticks == _gridStepTicks);
        return index >= 0 ? GridStepOptions[index].Name : _gridStepTicks.ToString();
    }

    private long GetBeatLengthTicks()
    {
        return (long)Math.Round(TicksPerBeat * 4.0 / _selectedTimeSignature.Denominator);
    }

    private long GetBarLengthTicks()
    {
        return GetBeatLengthTicks() * _selectedTimeSignature.Numerator;
    }

    private static string GetNoteLabel(int noteNumber)
    {
        string[] noteNames = ["C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B"];
        var octave = noteNumber / 12 - 1;
        return $"{noteNames[noteNumber % 12]}{octave}";
    }

    private static bool IsBlackKey(int noteNumber)
    {
        return noteNumber % 12 is 1 or 3 or 6 or 8 or 10;
    }

    private static int NoteNumberToRow(int noteNumber)
    {
        return HighestNoteNumber - noteNumber;
    }

    private static int ClampNoteNumber(int noteNumber)
    {
        return Math.Clamp(noteNumber, LowestNoteNumber, HighestNoteNumber);
    }

    private long SnapTicks(long ticks)
    {
        return Math.Max(0, (long)Math.Round(ticks / (double)_gridStepTicks) * _gridStepTicks);
    }

    private static long PositionToTicks(double x)
    {
        var clamped = Math.Clamp(x, 0, VisibleBeats * PixelsPerBeat);
        return (long)Math.Round(clamped / PixelsPerBeat * TicksPerBeat);
    }

    private static int PositionToNoteNumber(double y)
    {
        var row = (int)Math.Floor(Math.Clamp(y, 0, (HighestNoteNumber - LowestNoteNumber + 1) * RowHeight - 1) / RowHeight);
        return ClampNoteNumber(HighestNoteNumber - row);
    }

    protected override void OnClosed(EventArgs e)
    {
        _uiTimer.Stop();

        _playback?.Stop();
        _playback?.Dispose();
        _outputEndpoint?.Dispose();

        base.OnClosed(e);
    }

    private readonly record struct GridStepOption(string Name, long Ticks);

    private readonly record struct TimeFormatOption(string Name, TimeSpanType Type);

    private readonly record struct TimeSignatureOption(string Name, byte Numerator, byte Denominator);
}
