using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using ModernWpf.Controls;

namespace GenshinLyreMidiPlayer.WPF.ModernWPF.Errors;

internal static class ExceptionHandler
{
    private static readonly Dictionary<Type, List<Enum>> ExceptionOptions = new()
    {
        [typeof(InvalidChannelEventParameterValueException)] = new()
        {
            InvalidChannelEventParameterValuePolicy.SnapToLimits,
            InvalidChannelEventParameterValuePolicy.ReadValid
        },
        [typeof(InvalidMetaEventParameterValueException)] = new()
        {
            InvalidMetaEventParameterValuePolicy.SnapToLimits
        },
        [typeof(InvalidSystemCommonEventParameterValueException)] = new()
        {
            InvalidSystemCommonEventParameterValuePolicy.SnapToLimits
        },
        [typeof(UnknownChunkException)] = new()
        {
            UnknownChunkIdPolicy.ReadAsUnknownChunk,
            UnknownChunkIdPolicy.Skip
        },

        [typeof(InvalidChunkSizeException)] = new()
        {
            InvalidChunkSizePolicy.Ignore
        },
        [typeof(MissedEndOfTrackEventException)] = new()
        {
            MissedEndOfTrackPolicy.Ignore
        },
        [typeof(NoHeaderChunkException)] = new()
        {
            NoHeaderChunkPolicy.Ignore
        },
        [typeof(NotEnoughBytesException)] = new()
        {
            NotEnoughBytesPolicy.Ignore
        },
        [typeof(UnexpectedTrackChunksCountException)] = new()
        {
            UnexpectedTrackChunksCountPolicy.Ignore
        },
        [typeof(UnknownChannelEventException)] = new()
        {
            UnknownChannelEventPolicy.SkipStatusByte
        },
        [typeof(UnknownFileFormatException)] = new()
        {
            UnknownFileFormatPolicy.Ignore
        }
    };

    private static readonly IReadOnlyList<Type> FatalExceptions = new List<Type>
    {
        typeof(InvalidMidiTimeCodeComponentException),
        typeof(TooManyTrackChunksException),
        typeof(UnexpectedRunningStatusException)
    };

    /// <summary>
    ///     Tries to handle a <see cref="MidiException" />
    ///     with user interaction whether to abort the reading or ignore the exception.
    /// </summary>
    /// <param name="e">The <see cref="MidiException" /> that was thrown.</param>
    /// <param name="settings">The existing <see cref="ReadingSettings" /> to modify.</param>
    /// <returns>A <see cref="bool" /> whether reading can continue or not.</returns>
    [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
    [SuppressMessage("ReSharper", "CheckForReferenceEqualityInstead.1")]
    public static async Task<bool> TryHandleException(Exception e, ReadingSettings settings)
    {
        var command = ExceptionOptions
            .FirstOrDefault(type =>
                type.Key.Equals(e.GetType())).Value;

        var exceptionDialog = new ErrorContentDialog(e, command);
        var result = await exceptionDialog.ShowAsync();

        if (result == ContentDialogResult.None || FatalExceptions.Contains(e.GetType()))
            return false;

        var option = result switch
        {
            ContentDialogResult.Primary   => command.ElementAtOrDefault(0),
            ContentDialogResult.Secondary => command.ElementAtOrDefault(1),
            _                             => null
        };

        if (option is null) return false;

        switch (e)
        {
            // User selectable policy
            case InvalidChannelEventParameterValueException:
                settings.InvalidChannelEventParameterValuePolicy = (InvalidChannelEventParameterValuePolicy) option;
                break;
            case InvalidMetaEventParameterValueException:
                settings.InvalidMetaEventParameterValuePolicy = (InvalidMetaEventParameterValuePolicy) option;
                break;
            case InvalidSystemCommonEventParameterValueException:
                settings.InvalidSystemCommonEventParameterValuePolicy
                    = (InvalidSystemCommonEventParameterValuePolicy) option;
                break;
            case UnknownChannelEventException:
                settings.UnknownChannelEventPolicy = (UnknownChannelEventPolicy) option;
                break;
            case UnknownChunkException:
                settings.UnknownChunkIdPolicy = (UnknownChunkIdPolicy) option;
                break;

            // Ignorable policies
            case InvalidChunkSizeException:
                settings.InvalidChunkSizePolicy = InvalidChunkSizePolicy.Ignore;
                break;
            case MissedEndOfTrackEventException:
                settings.MissedEndOfTrackPolicy = MissedEndOfTrackPolicy.Ignore;
                break;
            case NoHeaderChunkException:
                settings.NoHeaderChunkPolicy = NoHeaderChunkPolicy.Ignore;
                break;
            case NotEnoughBytesException:
                settings.NotEnoughBytesPolicy = NotEnoughBytesPolicy.Ignore;
                break;
            case UnexpectedTrackChunksCountException:
                settings.UnexpectedTrackChunksCountPolicy = UnexpectedTrackChunksCountPolicy.Ignore;
                break;

            case UnknownFileFormatException:
                settings.UnknownFileFormatPolicy = UnknownFileFormatPolicy.Ignore;
                break;

            // Fatal exceptions
            case InvalidMidiTimeCodeComponentException:
            case TooManyTrackChunksException:
            case UnexpectedRunningStatusException:
                return false;
        }

        return true;
    }
}