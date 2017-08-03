using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public sealed class Pattern
    {
        #region Constructor

        internal Pattern(IEnumerable<IPatternAction> actions)
        {
            Actions = actions;
        }

        #endregion

        #region Properties

        internal IEnumerable<IPatternAction> Actions { get; }

        #endregion

        #region Methods

        public TrackChunk ToTrackChunk(TempoMap tempoMap, FourBitNumber channel)
        {
            var context = new PatternContext(tempoMap, channel);
            var result = InvokeActions(0, context);

            //

            var trackChunk = new TrackChunk();

            using (var notesManager = trackChunk.ManageNotes())
            {
                notesManager.Notes.Add(result.Notes ?? Enumerable.Empty<Note>());
            }

            //

            return trackChunk;
        }

        public MidiFile ToFile(FourBitNumber channel)
        {
            var midiFile = new MidiFile();
            var tempoMap = midiFile.GetTempoMap();

            midiFile.Chunks.Add(ToTrackChunk(tempoMap, channel));

            return midiFile;
        }

        internal PatternActionResult InvokeActions(long time, PatternContext context)
        {
            var notes = new List<Note>();

            foreach (var action in Actions)
            {
                var actionResult = action.Invoke(time, context);

                var newTime = actionResult.Time;
                if (newTime != null)
                    time = newTime.Value;

                var addedNotes = actionResult.Notes;
                if (addedNotes != null)
                    notes.AddRange(addedNotes);
            }

            return new PatternActionResult(time, notes);
        }

        #endregion
    }
}
