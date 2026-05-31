using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
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
    private const byte DefaultNoteVelocity = 100;
    private const double NoteVisualOffset = 1;
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
    private Button _playButton = null!;
    private Button _drawToolButton = null!;
    private Button _cutToolButton = null!;
    private CheckBox _snapCheckBox = null!;
    private Cursor? _drawCursor;
    private Cursor? _cutCursor;
    private Bitmap? _drawCursorBitmap;
    private Bitmap? _cutCursorBitmap;

    private Line _playhead = null!;
    private Line _cursorLine = null!;

    private Note? _draggedNote;
    private Point _dragStartPoint;
    private long _dragOriginalTime;
    private int _dragOriginalNoteNumber;
    private double _lastPointerX;

    private bool _isDrawing;
    private Point _drawStartPoint;
    private Border? _drawPreview;
    private TextBox? _velocityEditor;
    private Note? _velocityEditingNote;

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

        _playButton = this.FindControl<Button>("PlayButton")
            ?? throw new InvalidOperationException("PlayButton is not found.");
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
        _drawToolButton = this.FindControl<Button>("DrawToolButton")
            ?? throw new InvalidOperationException("DrawToolButton is not found.");
        _cutToolButton = this.FindControl<Button>("CutToolButton")
            ?? throw new InvalidOperationException("CutToolButton is not found.");
        _snapCheckBox = this.FindControl<CheckBox>("SnapCheckBox")
            ?? throw new InvalidOperationException("SnapCheckBox is not found.");

        _playButton.Click += (_, _) => TogglePlayback();
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

        _drawToolButton.Click += (_, _) => SetTool(ToolMode.Draw);
        _cutToolButton.Click += (_, _) => SetTool(ToolMode.Cut);
        UpdateToolButtons();
        InitializeToolCursors();

        _snapCheckBox.IsChecked = _isSnappingEnabled;
        _snapCheckBox.IsCheckedChanged += (_, _) =>
        {
            _isSnappingEnabled = _snapCheckBox.IsChecked ?? false;
            if (_cursorLine is { IsVisible: true })
                UpdateCursorLine(_lastPointerX);
            UpdatePlaybackVisuals();
            UpdateStatus();
        };

        _notesCanvas.PointerPressed += NotesCanvasOnPointerPressed;
        _notesCanvas.PointerMoved += NotesCanvasOnPointerMoved;
        _notesCanvas.PointerReleased += NotesCanvasOnPointerReleased;
        _notesCanvas.PointerEntered += NotesCanvasOnPointerEntered;
        _notesCanvas.PointerExited += NotesCanvasOnPointerExited;
        KeyDown += MainWindowOnKeyDown;

        UpdateTimeSignatureAvailability();
        UpdatePlayButtonState();
        UpdateToolCursor();
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
            StrokeThickness = 3.5,
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
            IsVisible = false,
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

            _keysCanvas.Children.Add(new Line
            {
                StartPoint = new Point(0, (row + 1) * RowHeight),
                EndPoint = new Point(95, (row + 1) * RowHeight),
                Stroke = new SolidColorBrush(Color.Parse("#6B7280")),
                StrokeThickness = 1,
                IsHitTestVisible = false
            });
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

        _playback.Loop = false;

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

    private void TogglePlayback()
    {
        if (_playback?.IsRunning == true)
            StopPlayback();
        else
            StartPlayback();
    }

    private void StartPlayback()
    {
        _playback?.Start();
        _uiTimer.Start();
        UpdatePlaybackVisuals();
        UpdatePlayButtonState();
        UpdateStatus();
    }

    private void StopPlayback()
    {
        _playback?.Stop();
        _uiTimer.Stop();
        UpdatePlaybackVisuals();
        UpdatePlayButtonState();
        UpdateStatus();
    }

    private void ResetPlaybackPosition()
    {
        _playback?.MoveToStart();
        UpdatePlaybackVisuals();
        UpdatePlayButtonState();
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
        if (_cursorLine is { IsVisible: true })
            UpdateCursorLine(_lastPointerX);
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
        if (_cursorLine is { IsVisible: true })
            UpdateCursorLine(_lastPointerX);
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
        if (_cursorLine is { IsVisible: true })
            UpdateCursorLine(_lastPointerX);
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

    private void SetTool(ToolMode toolMode)
    {
        _selectedTool = toolMode;
        UpdateToolButtons();
        UpdateToolCursor();
        UpdateStatus();
    }

    private void UpdateToolCursor()
    {
        _notesCanvas.Cursor = _selectedTool switch
        {
            ToolMode.Cut => _cutCursor ?? new Cursor(StandardCursorType.Cross),
            _ => _drawCursor ?? new Cursor(StandardCursorType.Cross)
        };
    }

    private void InitializeToolCursors()
    {
        _drawCursor = CreateCursorFromFile(
            "Assets/Cursors/draw-brush-cursor.png",
            new PixelPoint(3, 29),
            StandardCursorType.Cross,
            out _drawCursorBitmap);

        _cutCursor = CreateCursorFromFile(
            "Assets/Cursors/cut-knife-cursor.png",
            new PixelPoint(3, 29),
            StandardCursorType.No,
            out _cutCursorBitmap);
    }

    private static Cursor CreateCursorFromFile(
        string relativePath,
        PixelPoint hotSpot,
        StandardCursorType fallbackCursorType,
        out Bitmap? bitmap)
    {
        bitmap = null;
        var absolutePath = System.IO.Path.Combine(
            AppContext.BaseDirectory,
            relativePath.Replace('/', System.IO.Path.DirectorySeparatorChar));
        if (System.IO.File.Exists(absolutePath))
        {
            try
            {
                bitmap = new Bitmap(absolutePath);
                return new Cursor(bitmap, hotSpot);
            }
            catch
            {
                bitmap?.Dispose();
                bitmap = null;
                // Custom cursor is optional for the demo; fall back to a standard cursor if loading fails.
            }
        }

        return new Cursor(fallbackCursorType);
    }

    private void NotesCanvasOnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (_velocityEditor?.IsVisible == true && !ReferenceEquals(e.Source, _velocityEditor))
            ApplyVelocityEditor();

        var point = e.GetPosition(_notesCanvas);
        _lastPointerX = point.X;
        UpdateCursorLine(point.X);

        if (e.Source is Border { Tag: Note note })
        {
            if (e.GetCurrentPoint(_notesCanvas).Properties.IsLeftButtonPressed && e.ClickCount == 2)
            {
                OpenVelocityEditor(note);
                return;
            }

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
        _lastPointerX = point.X;
        UpdateCursorLine(point.X);

        if (_draggedNote != null)
        {
            var deltaX = point.X - _dragStartPoint.X;
            var deltaY = point.Y - _dragStartPoint.Y;

            var deltaTicks = (long)Math.Round(deltaX / PixelsPerBeat * TicksPerBeat);
            var newTime = NormalizeInteractionTicks(_dragOriginalTime + deltaTicks);
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

    private void NotesCanvasOnPointerEntered(object? sender, PointerEventArgs e)
    {
        var point = e.GetPosition(_notesCanvas);
        _lastPointerX = point.X;
        _cursorLine.IsVisible = true;
        UpdateCursorLine(point.X);
    }

    private void NotesCanvasOnPointerExited(object? sender, PointerEventArgs e)
    {
        _cursorLine.IsVisible = false;
    }

    private void AddNote(long time, long length, int noteNumber, int velocity = DefaultNoteVelocity)
    {
        var clampedVelocity = Math.Clamp(velocity, SevenBitNumber.MinValue, SevenBitNumber.MaxValue);
        var note = new Note((SevenBitNumber)noteNumber)
        {
            Time = Math.Max(0, time),
            Length = Math.Max(_gridStepTicks, length),
            Velocity = (SevenBitNumber)clampedVelocity
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

    private void ChangeNoteVelocity(Note note, int velocity)
    {
        var updatedVelocity = Math.Clamp(velocity, SevenBitNumber.MinValue, SevenBitNumber.MaxValue);
        _collection.ChangeObject(
            note,
            _ => note.Velocity = (SevenBitNumber)updatedVelocity);

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
        var (backgroundColor, borderColor) = GetNoteColors(note.Velocity);
        return new Border
        {
            Tag = note,
            Height = RowHeight - 2,
            CornerRadius = new CornerRadius(2),
            BorderBrush = new SolidColorBrush(borderColor),
            BorderThickness = new Thickness(1),
            Background = new SolidColorBrush(backgroundColor)
        };
    }

    private void UpdateNoteView(Note note)
    {
        if (!_noteViews.TryGetValue(note, out var view))
            return;

        view.Width = Math.Round(Math.Max(10, note.Length / (double)TicksPerBeat * PixelsPerBeat - 2));
        Canvas.SetLeft(view, Math.Round(note.Time / (double)TicksPerBeat * PixelsPerBeat + NoteVisualOffset));
        Canvas.SetTop(view, Math.Round(NoteNumberToRow(note.NoteNumber) * RowHeight + NoteVisualOffset));

        var (backgroundColor, borderColor) = GetNoteColors(note.Velocity);
        view.Background = new SolidColorBrush(backgroundColor);
        view.BorderBrush = new SolidColorBrush(borderColor);
    }

    private void RemoveNoteView(Note note)
    {
        if (!_noteViews.Remove(note, out var view))
            return;

        _notesCanvas.Children.Remove(view);
    }

    private void OpenVelocityEditor(Note note)
    {
        if (!_noteViews.TryGetValue(note, out var view))
            return;

        if (_velocityEditor == null)
        {
            _velocityEditor = new TextBox
            {
                Width = 54,
                Height = 24,
                Background = new SolidColorBrush(Color.Parse("#0F172A")),
                Foreground = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.Parse("#60A5FA")),
                BorderThickness = new Thickness(1),
                HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Center
            };

            _velocityEditor.KeyDown += VelocityEditorOnKeyDown;
            _velocityEditor.LostFocus += (_, _) => ApplyVelocityEditor();
            _notesCanvas.Children.Add(_velocityEditor);
        }

        _velocityEditingNote = note;
        _velocityEditor.Text = ((int)note.Velocity).ToString();

        var noteLeft = Canvas.GetLeft(view);
        var noteTop = Canvas.GetTop(view);
        var noteWidth = view.Width;

        var left = Math.Min(_notesCanvas.Width - _velocityEditor.Width - 4, noteLeft + noteWidth + 4);
        left = Math.Max(2, left);

        var top = Math.Max(0, Math.Min(_notesCanvas.Height - _velocityEditor.Height - 1, noteTop));

        Canvas.SetLeft(_velocityEditor, left);
        Canvas.SetTop(_velocityEditor, top);
        _velocityEditor.IsVisible = true;
        _velocityEditor.Focus();
        _velocityEditor.SelectAll();
    }

    private void VelocityEditorOnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            ApplyVelocityEditor();
            e.Handled = true;
            return;
        }

        if (e.Key == Key.Escape)
        {
            CloseVelocityEditor();
            e.Handled = true;
        }
    }

    private void ApplyVelocityEditor()
    {
        if (_velocityEditor?.IsVisible != true || _velocityEditingNote == null)
            return;

        var velocity = int.TryParse(_velocityEditor.Text, out var parsedVelocity)
            ? parsedVelocity
            : (int)_velocityEditingNote.Velocity;

        ChangeNoteVelocity(_velocityEditingNote, velocity);
        CloseVelocityEditor();
    }

    private void CloseVelocityEditor()
    {
        if (_velocityEditor == null)
            return;

        _velocityEditor.IsVisible = false;
        _velocityEditingNote = null;
        _notesCanvas.Focus();
    }

    private void EnsureDrawPreview()
    {
        if (_drawPreview != null)
            return;

        _drawPreview = new Border
        {
            Height = RowHeight - 2,
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

        _drawPreview.Width = Math.Round(Math.Max(10, width - 2));
        Canvas.SetLeft(_drawPreview, Math.Round(startX + NoteVisualOffset));
        Canvas.SetTop(_drawPreview, Math.Round(NoteNumberToRow(noteNumber) * RowHeight + NoteVisualOffset));
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

    private void UpdatePlayButtonState()
    {
        if (_playback?.IsRunning == true)
        {
            _playButton.Content = "■ Stop";
            _playButton.Background = new SolidColorBrush(Color.Parse("#B91C1C"));
        }
        else
        {
            _playButton.Content = "▶ Play";
            _playButton.Background = new SolidColorBrush(Color.Parse("#1D4ED8"));
        }
    }

    private void UpdateToolButtons()
    {
        var activeBackground = new SolidColorBrush(Color.Parse("#2563EB"));
        var inactiveBackground = new SolidColorBrush(Color.Parse("#1E3A8A"));
        var activeBorder = new SolidColorBrush(Color.Parse("#60A5FA"));
        var inactiveBorder = new SolidColorBrush(Color.Parse("#2E62AE"));

        var drawActive = _selectedTool == ToolMode.Draw;
        _drawToolButton.Background = drawActive ? activeBackground : inactiveBackground;
        _drawToolButton.BorderBrush = drawActive ? activeBorder : inactiveBorder;
        _drawToolButton.BorderThickness = new Thickness(1.2);
        _drawToolButton.Foreground = Brushes.White;

        var cutActive = _selectedTool == ToolMode.Cut;
        _cutToolButton.Background = cutActive ? activeBackground : inactiveBackground;
        _cutToolButton.BorderBrush = cutActive ? activeBorder : inactiveBorder;
        _cutToolButton.BorderThickness = new Thickness(1.2);
        _cutToolButton.Foreground = Brushes.White;
    }

    private void UpdateStatus()
    {
        var outputEndpointName = _outputEndpoint?.Name ?? "No output endpoint (silent playback)";
        var signatureText = _timeSignatureComboBox.IsEnabled ? _selectedTimeSignature.Name : "N/A";
        var toolName = _selectedTool == ToolMode.Draw ? "Draw notes" : "Cut notes";
        var snapText = _isSnappingEnabled ? "On" : "Off";

        _statusText.Text = $"Notes: {_noteViews.Count} | Grid: {GetGridStepName()} | Signature: {signatureText} | Tool: {toolName} | Snap: {snapText} | Output: {outputEndpointName}";
    }

    private string GetGridStepName()
    {
        return _selectedGridStepName;
    }

    private void MainWindowOnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Space)
            return;

        if (_velocityEditor?.IsFocused == true)
            return;

        TogglePlayback();
        e.Handled = true;
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

    private static (Color Background, Color Border) GetNoteColors(SevenBitNumber velocity)
    {
        var velocityFactor = velocity / (double)SevenBitNumber.MaxValue;
        var background = InterpolateColor(Color.Parse("#1E3A8A"), Color.Parse("#60A5FA"), velocityFactor);
        var border = InterpolateColor(Color.Parse("#172554"), Color.Parse("#1D4ED8"), velocityFactor);
        return (background, border);
    }

    private static Color InterpolateColor(Color start, Color end, double factor)
    {
        var clamped = Math.Clamp(factor, 0, 1);
        return Color.FromArgb(
            InterpolateByte(start.A, end.A, clamped),
            InterpolateByte(start.R, end.R, clamped),
            InterpolateByte(start.G, end.G, clamped),
            InterpolateByte(start.B, end.B, clamped));
    }

    private static byte InterpolateByte(byte start, byte end, double factor)
    {
        return (byte)Math.Round(start + (end - start) * factor);
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
        var velocity = (int)note.Velocity;
        var firstLength = splitTicks - note.Time;
        var secondLength = note.EndTime - splitTicks;

        RemoveNote(note);
        AddNote(note.Time, firstLength, noteNumber, velocity);
        AddNote(splitTicks, secondLength, noteNumber, velocity);
    }

    protected override void OnClosed(EventArgs e)
    {
        _uiTimer.Stop();

        _playback?.Stop();
        _playback?.Dispose();
        _outputEndpoint?.Dispose();
        _drawCursor?.Dispose();
        _cutCursor?.Dispose();
        _drawCursorBitmap?.Dispose();
        _cutCursorBitmap?.Dispose();

        base.OnClosed(e);
    }

    private readonly record struct GridStepOption(string Name, long Ticks);

    private readonly record struct TimeFormatOption(string Name, TimeSpanType Type);

    private readonly record struct TimeSignatureOption(string Name, byte Numerator, byte Denominator);

    private enum ToolMode
    {
        Draw,
        Cut
    }
}
