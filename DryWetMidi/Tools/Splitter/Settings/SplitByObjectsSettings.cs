using Melanchall.DryWetMidi.Interaction;
using System;

namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Defines how a MIDI file should be split by objects using
    /// <see cref="Splitter.SplitByObjects(Core.MidiFile, ObjectType, SplitByObjectsSettings, ObjectDetectionSettings)"/> method.
    /// More info in the
    /// <see href="xref:a_file_splitting#splitbyobjects">MIDI file splitting: SplitByObjects</see> article.
    /// </summary>
    /// <seealso cref="Splitter"/>
    public sealed class SplitByObjectsSettings
    {
        #region Properties

        /// <summary>
        /// Gets or sets a method to get the ID (key) of an object. The default value
        /// is <c>null</c> which means the default key selector will be used.
        /// </summary>
        public Func<ITimedObject, object> KeySelector { get; set; }

        /// <summary>
        /// Gets or sets a predicate to determine whether an object should be copied
        /// to each new file or not. The default value is <c>null</c> which means no
        /// one object will be copied to each file.
        /// </summary>
        public Predicate<ITimedObject> WriteToAllFilesPredicate { get; set; }

        /// <summary>
        /// Gets or sets a predicate to filter objects out. The default value is <c>null</c>
        /// which means no filter applied.
        /// </summary>
        public Predicate<ITimedObject> Filter { get; set; }

        /// <summary>
        /// Gets or sets a predicate to filter out objects that should be copied to each new
        /// file (i.e. for those objects <see cref="WriteToAllFilesPredicate"/> returns <c>true</c> for).
        /// The default value is <c>null</c> which means no filter applied.
        /// </summary>
        public Predicate<ITimedObject> AllFilesObjectsFilter { get; set; }

        #endregion
    }
}
