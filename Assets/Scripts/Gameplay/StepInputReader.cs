using MindTheCrack.Config;
using MindTheCrack.Rhythm;
using UnityEngine;

namespace MindTheCrack.Gameplay
{
    /// <summary>
    /// Reads the one input the game has: tap for a long step, swipe up to jump.
    ///
    /// Two rules from docs/game-design.md 2.1:
    ///   - Unity UI Buttons are never used, their callback fires on release.
    ///   - The timestamp is taken at press, not at release, and not at the end
    ///     of the frame. A swipe that starts as a press keeps the press
    ///     timestamp, so flicking does not cost the player timing accuracy.
    ///
    /// Polling happens in this component's own Update, ahead of the conductor
    /// (execution order below). Polling from the run controller left the order
    /// of "sample input" and "commit the beat" up to Unity: on the unlucky
    /// ordering every press made in the frame a beat commits was still
    /// unsampled, so it was judged against the *next* beat, a whole beat early,
    /// and thrown away. That reads as a dead tap.
    /// </summary>
    [DefaultExecutionOrder(-200)]
    public sealed class StepInputReader : MonoBehaviour
    {
        const float SwipeThresholdFraction = 0.06f; // of screen height

        public struct PendingInput
        {
            public StepKind Kind;
            public double SongTime;
            public bool HasValue;
        }

        public Conductor Conductor;

        /// <summary>Inputs older than this are dropped instead of carried to the next beat.</summary>
        public double MaxAge = 0.2;

        /// <summary>
        /// Song time of the most recent press, whatever became of it. The HUD
        /// flashes on this: a tap that is going to be thrown away still has to
        /// look received, or a mistimed tap is indistinguishable from a dead
        /// patch of screen.
        /// </summary>
        public double LastPressSongTime { get; private set; } = double.NegativeInfinity;

        PendingInput _pending;
        Vector2 _pressPos;
        bool _pressed;
        bool _upgradedToSwipe;
        int _activeFinger = -1;

        public PendingInput Consume()
        {
            var p = _pending;
            _pending = default;
            return p;
        }

        void Update()
        {
            if (Conductor == null || !Conductor.Running) return;
            Poll(Conductor.SongTime);
        }

        /// <summary>Samples the device. Public so tests can drive it directly.</summary>
        public void Poll(double songTime)
        {
            // A press that missed its beat must not be re-judged against the
            // next one: one late tap used to cost two steps, the beat it was
            // late for and the beat it was then a half-beat early for.
            if (_pending.HasValue && songTime - _pending.SongTime > MaxAge)
                _pending = default;

            float threshold = Screen.height * SwipeThresholdFraction;

            if (TryGetPress(out Vector2 pos, out int finger))
            {
                _pressPos = pos;
                _pressed = true;
                _activeFinger = finger;
                _upgradedToSwipe = false;
                Queue(StepKind.Long, songTime);
            }
            else if (_pressed && TryGetDrag(_activeFinger, out Vector2 current))
            {
                if (!_upgradedToSwipe && current.y - _pressPos.y > threshold)
                {
                    _upgradedToSwipe = true;
                    // Keep the original press timestamp - the player committed
                    // then. If the press was already consumed by a beat the
                    // swipe has to queue itself, otherwise setting Kind on a
                    // spent slot drops the jump silently.
                    if (_pending.HasValue) _pending.Kind = StepKind.Jump;
                    else Queue(StepKind.Jump, songTime);
                }
            }

            if (TryGetRelease(_activeFinger))
            {
                _pressed = false;
                _activeFinger = -1;
            }
        }

        void Queue(StepKind kind, double songTime)
        {
            LastPressSongTime = songTime;
            _pending.Kind = kind;
            _pending.SongTime = songTime;
            _pending.HasValue = true;
        }

        // Every touch is scanned, not touch 0. Players tap alternating fingers
        // at speed, and the second finger comes down while the first is still
        // on the glass - as touch 1, which the old reader never looked at.
        static bool TryGetPress(out Vector2 pos, out int finger)
        {
            pos = default;
            finger = -1;

            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch t = Input.GetTouch(i);
                if (t.phase == TouchPhase.Began)
                {
                    pos = t.position;
                    finger = t.fingerId;
                    return true;
                }
            }
            if (Input.touchCount > 0) return false;

            if (Input.GetMouseButtonDown(0)) { pos = Input.mousePosition; return true; }
            if (Input.GetKeyDown(KeyCode.Space)) { pos = Vector2.zero; return true; }
            return false;
        }

        static bool TryGetDrag(int finger, out Vector2 pos)
        {
            pos = default;
            if (Input.touchCount > 0)
            {
                if (!TryFind(finger, out Touch t)) return false;
                if (t.phase is TouchPhase.Moved or TouchPhase.Stationary)
                {
                    pos = t.position;
                    return true;
                }
                return false;
            }
            if (Input.GetMouseButton(0)) { pos = Input.mousePosition; return true; }
            // Keyboard fallback for editor testing: up arrow upgrades to a jump.
            if (Input.GetKey(KeyCode.UpArrow))
            {
                pos = new Vector2(0, Screen.height);
                return true;
            }
            return false;
        }

        static bool TryGetRelease(int finger)
        {
            if (Input.touchCount > 0)
            {
                if (!TryFind(finger, out Touch t)) return false;
                return t.phase is TouchPhase.Ended or TouchPhase.Canceled;
            }
            return Input.GetMouseButtonUp(0) || Input.GetKeyUp(KeyCode.Space);
        }

        static bool TryFind(int finger, out Touch touch)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch t = Input.GetTouch(i);
                if (t.fingerId == finger) { touch = t; return true; }
            }
            touch = default;
            return false;
        }
    }
}
