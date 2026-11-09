using System;

namespace Avalonia.Controls.Maui.Platforms.EmbeddedPopups
{
    /// <summary>
    /// Detects that an embedded view moved while its popups are open, for example because its page scrolled.
    /// </summary>
    /// <remarks>
    /// The first position after <see cref="Reset"/> is the reference. Later positions that differ from it by more
    /// than <see cref="Threshold"/> on either axis count as a move, so layout rounding does not.
    /// </remarks>
    internal sealed class EmbeddedPopupHostMoveTracker
    {
        /// <summary>The distance, in device-independent pixels, that the host must move to count as moved.</summary>
        public const double Threshold = 1;

        private Point? _reference;

        /// <summary>Forgets the reference position. The next position becomes the new reference.</summary>
        public void Reset() => _reference = null;

        /// <summary>
        /// Returns true if <paramref name="position"/> is more than <see cref="Threshold"/> away from the reference.
        /// </summary>
        /// <param name="position">The current position of the host, or null if it is not in a window.</param>
        public bool HasMoved(Point? position)
        {
            if (position is not { } current)
                return false;

            if (_reference is not { } reference)
            {
                _reference = current;
                return false;
            }

            return Math.Abs(current.X - reference.X) > Threshold || Math.Abs(current.Y - reference.Y) > Threshold;
        }
    }
}
