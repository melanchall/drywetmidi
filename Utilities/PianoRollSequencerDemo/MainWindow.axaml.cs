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
    private const string SilentOutputOption = "Silent (no output)";

    private static readonly GridStepOption[] GridStepOptions =
    [
        new("1/4", TicksPerBeat),
        new("1/8", TicksPerBeat / 2),
        new("1/16", TicksPerBeat / 4),
        new("1/32", TicksPerBeat / 8)
    ];

    private static readonly GridStepOption[] MetricGridStepOptions =
    [
        new("100 ms", TimeConverter.ConvertFrom((MetricTimeSpan)TimeSpan.FromMilliseconds(100), TempoMap.Default)),
        new("500 ms", TimeConverter.ConvertFrom((MetricTimeSpan)TimeSpan.FromMilliseconds(500), TempoMap.Default)),
        new("1 sec", TimeConverter.ConvertFrom((MetricTimeSpan)TimeSpan.FromSeconds(1), TempoMap.Default)),
        new("2 sec", TimeConverter.ConvertFrom((MetricTimeSpan)TimeSpan.FromSeconds(2), TempoMap.Default)),
        new("5 sec", TimeConverter.ConvertFrom((MetricTimeSpan)TimeSpan.FromSeconds(5), TempoMap.Default))
    ];

    private static readonly GridStepOption[] MusicalGridStepOptions =
    [
        new("Whole", TimeConverter.ConvertFrom(MusicalTimeSpan.Whole, TempoMap.Default)),
        new("Half", TimeConverter.ConvertFrom(MusicalTimeSpan.Half, TempoMap.Default)),
        new("Quarter", TimeConverter.ConvertFrom(MusicalTimeSpan.Quarter, TempoMap.Default)),
        new("Eighth", TimeConverter.ConvertFrom(MusicalTimeSpan.Eighth, TempoMap.Default)),
        new("Sixteenth", TimeConverter.ConvertFrom(MusicalTimeSpan.Sixteenth, TempoMap.Default))
    ];

    private static readonly GridStepOption[] MidiGridStepOptions =
    [
        new("120 ticks", 120),
        new("240 ticks", 240),
        new("480 ticks", 480),
        new("960 ticks", 960)
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

    private static readonly ToolOption[] ToolOptions =
    [
        new("Draw notes", ToolMode.Draw),
        new("Cut notes", ToolMode.Cut)
    ];

    private readonly ObservableTimedObjectsCollection _collection = [];
    private readonly Dictionary<Note, Border> _noteViews = [];
    private readonly List<string> _outputEndpointNames = [];
    private readonly DispatcherTimer _uiTimer;

    private long _gridStepTicks = TicksPerBeat / 4;
    private GridStepOption[] _activeGridStepOptions = GridStepOptions;
    private string _selectedGridStepName = "1/16";
    private TimeFormatOption _selectedTimeFormat = TimeFormatOptions[2];
    private TimeSignatureOption _selectedTimeSignature = TimeSignatureOptions[0];
    private ToolMode _selectedTool = ToolMode.Draw;
    private bool _isSnappingEnabled = true;
    private string _selectedOutputOption = SilentOutputOption;
    private TempoMap _tempoMap;

    private Playback? _playback;
    private OutputEndpoint? _outputEndpoint;

    private Canvas _keysCanvas = null!;
    private Canvas _gridCanvas = null!;
    private Canvas _notesCanvas = null!;
    private TextBlock _statusText = null!;
    private TextBlock _currentTimeText = null!;
    private ComboBox _gridStepComboBox = null!;
    private ComboBox _timeSignatureComboBox = null!;
    private ComboBox _outputEndpointComboBox = null!;
    private ComboBox _toolComboBox = null!;
    private CheckBox _snapCheckBox = null!;

    private Line _playhead = null!;
    private Line _cursorLine = null!;

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
        PopulateOutputEndpointOptions();
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

        _gridStepComboBox = this.FindControl<ComboBox>("GridStepComboBox")
            ?? throw new InvalidOperationException("GridStepComboBox is not found.");
        var timeFormatComboBox = this.FindControl<ComboBox>("TimeFormatComboBox")
            ?? throw new InvalidOperationException("TimeFormatComboBox is not found.");
        _timeSignatureComboBox = this.FindControl<ComboBox>("TimeSignatureComboBox")
            ?? throw new InvalidOperationException("TimeSignatureComboBox is not found.");
        _outputEndpointComboBox = this.FindControl<ComboBox>("OutputEndpointComboBox")
            ?? throw new InvalidOperationException("OutputEndpointComboBox is not found.");
        _toolComboBox = this.FindControl<ComboBox>("ToolComboBox")
            ?? throw new InvalidOperationException("ToolComboBox is not found.");
        _snapCheckBox = this.FindControl<CheckBox>("SnapCheckBox")
            ?? throw new InvalidOperationException("SnapCheckBox is not found.");

        playButton.Click += (_, _) => StartPlayback();
        stopButton.Click += (_, _) => StopPlayback();
        resetButton.Click += (_, _) => ResetPlaybackPosition();
        clearButton.Click += (_, _) => ClearNotes();

        _gridStepComboBox.SelectionChanged += (_, _) => OnGridStepChanged(_gridStepComboBox.SelectedIndex);
        RefreshGridStepOptions();

        timeFormatComboBox.ItemsSource = TimeFormatOptions.Select(option => option.Name).ToArray();
        timeFormatComboBox.SelectedIndex = Array.FindIndex(TimeFormatOptions, option => option.Type == _selectedTimeFormat.Type);
        timeFormatComboBox.SelectionChanged += (_, _) => OnTimeFormatChanged(timeFormatComboBox.SelectedIndex);

        _timeSignatureComboBox.ItemsSource = TimeSignatureOptions.Select(option => option.Name).ToArray();
        _timeSignatureComboBox.SelectedIndex = Array.FindIndex(TimeSignatureOptions, option => option.Name == _selectedTimeSignature.Name);
        _timeSignatureComboBox.SelectionChanged += (_, _) => OnTimeSignatureChanged(_timeSignatureComboBox.SelectedIndex);

        _toolComboBox.ItemsSource = ToolOptions.Select(option => option.Name).ToArray();
        _toolComboBox.SelectedIndex = 0;
        _toolComboBox.SelectionChanged += (_, _) => OnToolChanged(_toolComboBox.SelectedIndex);

        _snapCheckBox.IsChecked = _isSnappingEnabled;
        _snapCheckBox.IsCheckedChanged += (_, _) =>
        {
            _isSnappingEnabled = _snapCheckBox.IsChecked ?? false;
            UpdatePlaybackVisuals();
            UpdateStatus();
        };

        _notesCanvas.PointerPressed += NotesCanvasOnPointerPressed;
        _notesCanvas.PointerMoved += NotesCanvasOnPointerMoved;
        _notesCanvas.PointerReleased += NotesCanvasOnPointerReleased;

        UpdateTimeSignatureAvailability();
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
            Stroke = new SolidColorBrush(Color.Parse("#EF4444")),
            StrokeThickness = 2,
            IsHitTestVisible = false,
            ZIndex = 1000
        };

        _cursorLine = new Line
        {
            StartPoint = new Point(0, 0),
            EndPoint = new Point(0, height),
            Stroke = new SolidColorBrush(Color.Parse("#60A5FA")),
            StrokeThickness = 1,
            Opacity = 0.75,
            IsHitTestVisible = false,
            ZIndex = 900
        };

        _notesCanvas.Children.Add(_cursorLine);
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
                Fill = new SolidColorBrush(Color.Parse(isBlack ? "#13213A" : "#172A45")),
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
                Stroke = new SolidColorBrush(Color.Parse("#243C61")),
                StrokeThickness = 1,
                IsHitTestVisible = false
            });
        }

        var totalTicks = VisibleBeats * TicksPerBeat;
        DrawGridStepLines(totalRows, totalTicks, Color.Parse("#2E62AE"), 0.7, 1);

        if (UsesBarBeatFormat(_selectedTimeFormat.Type))
            DrawBarBeatGridLines(totalRows, totalTicks);
        else if (_selectedTimeFormat.Type == TimeSpanType.Metric)
            DrawMetricSecondLines(totalRows, totalTicks);
    }

    private void DrawGridStepLines(int totalRows, long totalTicks, Color color, double opacity, double thickness)
    {
        if (_gridStepTicks <= 0)
            return;

        for (var ticks = 0L; ticks <= totalTicks; ticks += _gridStepTicks)
        {
            AddVerticalGridLine(totalRows, ticks, color, opacity, thickness);
        }
    }

    private void DrawBarBeatGridLines(int totalRows, long totalTicks)
    {
        var barLengthTicks = GetBarLengthTicks();
        var beatLengthTicks = GetBeatLengthTicks();
        if (barLengthTicks <= 0 || beatLengthTicks <= 0)
            return;

        for (var ticks = 0L; ticks <= totalTicks; ticks += beatLengthTicks)
        {
            var isBar = ticks % barLengthTicks == 0;
            AddVerticalGridLine(
                totalRows,
                ticks,
                Color.Parse(isBar ? "#EF4444" : "#93C5FD"),
                isBar ? 0.95 : 0.8,
                isBar ? 2 : 1.3);
        }
    }

    private void DrawMetricSecondLines(int totalRows, long totalTicks)
    {
        var totalMetricTime = TimeConverter.ConvertTo<MetricTimeSpan>(totalTicks, _tempoMap);
        var seconds = (int)Math.Ceiling(totalMetricTime.TotalSeconds);

        for (var second = 0; second <= seconds; second++)
        {
            var ticks = TimeConverter.ConvertFrom(new MetricTimeSpan(0, 0, second), _tempoMap);
            if (ticks > totalTicks)
                break;

            AddVerticalGridLine(totalRows, ticks, Color.Parse("#EF4444"), 0.9, 1.8);
        }
    }

    private void AddVerticalGridLine(int totalRows, long ticks, Color color, double opacity, double thickness)
    {
        var x = ticks / (double)TicksPerBeat * PixelsPerBeat;
        var line = new Line
        {
            StartPoint = new Point(x, 0),
            EndPoint = new Point(x, totalRows * RowHeight),
            Stroke = new SolidColorBrush(color),
            StrokeThickness = thickness,
            IsHitTestVisible = false,
            Opacity = opacity
        };

        _gridCanvas.Children.Add(line);
    }

    private void InitializePlayback(long? currentTickTime = null)
    {
        _playback?.Stop();
        _playback?.Dispose();

        _playback = _outputEndpoint != null
            ? new Playback(_collection, _tempoMap, _outputEndpoint)
            : new Playback(_collection, _tempoMap);

        _playback.Loop = true;

        if (currentTickTime.HasValue)
            _playback.MoveToTime(new MidiTimeSpan(currentTickTime.Value));
    }

    private void PopulateOutputEndpointOptions()
    {
        _outputEndpointNames.Clear();
        _outputEndpointNames.Add(SilentOutputOption);

        try
        {
            var discoveredEndpoints = OutputEndpoint.GetAll().ToList();
            foreach (var endpoint in discoveredEndpoints)
            {
                if (!_outputEndpointNames.Contains(endpoint.Name))
                    _outputEndpointNames.Add(endpoint.Name);

                endpoint.Dispose();
            }
        }
        catch
        {
            _outputEndpointNames.Clear();
            _outputEndpointNames.Add(SilentOutputOption);
        }

        _outputEndpointComboBox.ItemsSource = _outputEndpointNames.ToArray();

        if (_selectedOutputOption == SilentOutputOption || !_outputEndpointNames.Contains(_selectedOutputOption))
            _selectedOutputOption = _outputEndpointNames.FirstOrDefault(name => name != SilentOutputOption) ?? SilentOutputOption;

        _outputEndpointComboBox.SelectedIndex = Math.Max(0, _outputEndpointNames.IndexOf(_selectedOutputOption));
        _outputEndpointComboBox.SelectionChanged += (_, _) => OnOutputEndpointChanged(_outputEndpointComboBox.SelectedIndex);
        ReopenSelectedOutputEndpoint();
    }

    private void OnOutputEndpointChanged(int selectedIndex)
    {
        if (selectedIndex < 0 || selectedIndex >= _outputEndpointNames.Count)
            return;

        _selectedOutputOption = _outputEndpointNames[selectedIndex];
        var currentTicks = _playback?.GetCurrentTime<MidiTimeSpan>().TimeSpan;
        var wasRunning = _playback?.IsRunning == true;

        ReopenSelectedOutputEndpoint();
        InitializePlayback(currentTicks);

        if (wasRunning)
        {
            _playback?.Start();
            _uiTimer.Start();
        }

        UpdateStatus();
    }

    private void ReopenSelectedOutputEndpoint()
    {
        _outputEndpoint?.Dispose();
        _outputEndpoint = null;

        if (_selectedOutputOption == SilentOutputOption)
            return;

        try
        {
            var discoveredEndpoints = OutputEndpoint.GetAll().ToList();
            foreach (var endpoint in discoveredEndpoints)
            {
                if (_outputEndpoint == null && endpoint.Name == _selectedOutputOption)
                {
                    _outputEndpoint = endpoint;
                    continue;
                }

                endpoint.Dispose();
            }
        }
        catch
        {
            _outputEndpoint?.Dispose();
            _outputEndpoint = null;
            _selectedOutputOption = SilentOutputOption;
            _outputEndpointComboBox.SelectedIndex = 0;
        }
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
        if (selectedIndex < 0 || selectedIndex >= _activeGridStepOptions.Length)
            return;

        _gridStepTicks = _activeGridStepOptions[selectedIndex].Ticks;
        _selectedGridStepName = _activeGridStepOptions[selectedIndex].Name;
        RedrawGrid();
        UpdatePlaybackVisuals();
        UpdateStatus();
    }

    private void OnTimeFormatChanged(int selectedIndex)
    {
        if (selectedIndex < 0 || selectedIndex >= TimeFormatOptions.Length)
            return;

        _selectedTimeFormat = TimeFormatOptions[selectedIndex];
        UpdateTimeSignatureAvailability();
        RefreshGridStepOptions();
        RedrawGrid();
        UpdatePlaybackVisuals();
        UpdateStatus();
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
        RefreshGridStepOptions();
        RedrawGrid();
        UpdatePlaybackVisuals();
        UpdateStatus();

        if (wasRunning)
        {
            _playback?.Start();
            _uiTimer.Start();
        }
    }

    private void RefreshGridStepOptions()
    {
        _activeGridStepOptions = GetGridStepOptionsForCurrentTimeFormat();
        _gridStepComboBox.ItemsSource = _activeGridStepOptions.Select(option => option.Name).ToArray();

        var selectedIndex = Array.FindIndex(_activeGridStepOptions, option => option.Name == _selectedGridStepName);
        if (selectedIndex < 0)
        {
            selectedIndex = Array.FindIndex(_activeGridStepOptions, option => option.Ticks == _gridStepTicks);
        }

        if (selectedIndex < 0)
            selectedIndex = GetDefaultGridStepIndex(_selectedTimeFormat.Type);

        selectedIndex = Math.Clamp(selectedIndex, 0, _activeGridStepOptions.Length - 1);
        _gridStepComboBox.SelectedIndex = selectedIndex;
        _gridStepTicks = _activeGridStepOptions[selectedIndex].Ticks;
        _selectedGridStepName = _activeGridStepOptions[selectedIndex].Name;
    }

    private GridStepOption[] GetGridStepOptionsForCurrentTimeFormat()
    {
        return _selectedTimeFormat.Type switch
        {
            TimeSpanType.Metric => MetricGridStepOptions,
            TimeSpanType.Musical => MusicalGridStepOptions,
            TimeSpanType.Midi => MidiGridStepOptions,
            _ => GridStepOptions
        };
    }

    private static int GetDefaultGridStepIndex(TimeSpanType timeSpanType)
    {
        return timeSpanType switch
        {
            TimeSpanType.Metric => 1,
            TimeSpanType.Musical => 2,
            TimeSpanType.Midi => 1,
            _ => 2
        };
    }

    private void UpdateTimeSignatureAvailability()
    {
        _timeSignatureComboBox.IsEnabled = UsesBarBeatFormat(_selectedTimeFormat.Type);
    }

    private static bool UsesBarBeatFormat(TimeSpanType timeSpanType)
    {
        return timeSpanType == TimeSpanType.BarBeatTicks || timeSpanType == TimeSpanType.BarBeatFraction;
    }

    private void OnToolChanged(int selectedIndex)
    {
        if (selectedIndex < 0 || selectedIndex >= ToolOptions.Length)
            return;

        _selectedTool = ToolOptions[selectedIndex].Mode;
        UpdateStatus();
    }

    private void NotesCanvasOnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var point = e.GetPosition(_notesCanvas);
        UpdateCursorLine(point.X);

        if (e.Source is Border { Tag: Note note })
        {
            if (_selectedTool == ToolMode.Cut && e.GetCurrentPoint(_notesCanvas).Properties.IsLeftButtonPressed)
            {
                SplitNote(note, point.X);
                return;
            }

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
            var startTicks = NormalizeInteractionTicks(PositionToTicks(point.X));
            AddNote(startTicks, _gridStepTicks, noteNumber);
            return;
        }

        if (_selectedTool == ToolMode.Cut)
            return;

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
        UpdateCursorLine(point.X);

        if (_draggedNote != null)
        {
            var deltaX = point.X - _dragStartPoint.X;
            var deltaY = point.Y - _dragStartPoint.Y;

            var newTime = NormalizeInteractionTicks((long)Math.Round((_dragOriginalTime / (double)TicksPerBeat * PixelsPerBeat + deltaX) / PixelsPerBeat * TicksPerBeat));
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

        var startTicks = NormalizeInteractionTicks(PositionToTicks(_drawStartPoint.X));
        var endTicks = NormalizeInteractionTicks(PositionToTicks(point.X));

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
        _cursorLine.ZIndex = 900;
        _playhead.ZIndex = 1000;
    }

    private static Border CreateNoteView(Note note)
    {
        return new Border
        {
            Tag = note,
            Height = RowHeight - 3,
            CornerRadius = new CornerRadius(2),
            BorderBrush = new SolidColorBrush(Color.Parse("#1E3A8A")),
            BorderThickness = new Thickness(1),
            Background = new SolidColorBrush(Color.Parse("#3B82F6"))
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
            BorderBrush = new SolidColorBrush(Color.Parse("#EF4444")),
            Background = new SolidColorBrush(Color.Parse("#66EF4444")),
            IsHitTestVisible = false
        };

        _notesCanvas.Children.Add(_drawPreview);
    }

    private void UpdateDrawPreview(Point point)
    {
        if (_drawPreview == null)
            return;

        _drawPreview.SetValue(IsVisibleProperty, true);

        var startTicks = NormalizeInteractionTicks(PositionToTicks(_drawStartPoint.X));
        var endTicks = NormalizeInteractionTicks(PositionToTicks(point.X));
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
        var signatureText = _timeSignatureComboBox.IsEnabled ? _selectedTimeSignature.Name : "N/A";
        var selectedToolOption = ToolOptions.FirstOrDefault(option => option.Mode == _selectedTool);
        var toolName = string.IsNullOrEmpty(selectedToolOption.Name) ? "Unknown" : selectedToolOption.Name;
        var snapText = _isSnappingEnabled ? "On" : "Off";

        _statusText.Text = $"Notes: {_noteViews.Count} | Grid: {GetGridStepName()} | Signature: {signatureText} | Tool: {toolName} | Snap: {snapText} | Output: {outputEndpointName}";
    }

    private string GetGridStepName()
    {
        return _selectedGridStepName;
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

    private long NormalizeInteractionTicks(long ticks)
    {
        var clampedTicks = Math.Max(0, ticks);
        return _isSnappingEnabled ? SnapTicks(clampedTicks) : clampedTicks;
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

    private void UpdateCursorLine(double positionX)
    {
        var ticks = NormalizeInteractionTicks(PositionToTicks(positionX));
        var x = ticks / (double)TicksPerBeat * PixelsPerBeat;

        _cursorLine.StartPoint = new Point(x, 0);
        _cursorLine.EndPoint = new Point(x, _notesCanvas.Height);
    }

    private void SplitNote(Note note, double positionX)
    {
        var splitTicks = NormalizeInteractionTicks(PositionToTicks(positionX));
        var minSplit = note.Time + _gridStepTicks;
        var maxSplit = note.EndTime - _gridStepTicks;
        if (splitTicks <= minSplit || splitTicks >= maxSplit)
            return;

        var noteNumber = (int)note.NoteNumber;
        var firstLength = splitTicks - note.Time;
        var secondLength = note.EndTime - splitTicks;

        RemoveNote(note);
        AddNote(note.Time, firstLength, noteNumber);
        AddNote(splitTicks, secondLength, noteNumber);
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

    private readonly record struct ToolOption(string Name, ToolMode Mode);

    private enum ToolMode
    {
        Draw,
        Cut
    }
}
