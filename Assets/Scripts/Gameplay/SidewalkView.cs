using System.Collections.Generic;
using UnityEngine;

namespace MindTheCrack.Gameplay
{
    /// <summary>
    /// Greybox sidewalk: one continuous pavement strip with the cracks painted
    /// on top of it as dark lines.
    ///
    /// The first attempt modelled each stone as its own raised cube and let the
    /// gap between them be the crack. On device that read as a single blank
    /// slab: at a 29 degree camera pitch a 0.13 wide gap is completely occluded
    /// by the near stone's own 0.26 height, so the player never saw a line.
    /// A painted line reads at any camera angle, which is what
    /// docs/game-design.md 11 actually asks for - the crack has to be the
    /// darkest value on screen.
    ///
    /// A faint ruler is painted under the cracks at half a unit, because the
    /// three step lengths are 2, 3 and 5 of those. Judging "is that gap one
    /// step or one and a half" off bare pavement is guesswork; counting blocks
    /// is not. The ruler stays much lighter than a crack - the crack keeps the
    /// darkest value on screen.
    /// </summary>
    public sealed class SidewalkView : MonoBehaviour
    {
        const float SidewalkWidth = 2.2f;
        const float CrackWidth = 0.16f;
        const float GridSpacing = 0.5f;   // normal 2, long 3, jump 5
        const float GridWidth = 0.035f;
        const float PavementTop = 0.1f;
        const float StripLength = 90f;

        const float AheadOfPlayer = 30f;
        const float BehindPlayer = 12f;

        readonly List<GameObject> _lines = new();
        readonly List<float> _lineAt = new();
        readonly List<GameObject> _grid = new();
        readonly List<int> _gridAt = new();

        Material _pavementMaterial;
        Material _crackMaterial;
        Material _gridMaterial;
        Transform _root;
        Transform _pavement;

        void Awake()
        {
            _root = new GameObject("Sidewalk").transform;
            _root.SetParent(transform, false);

            _pavementMaterial = Greybox.NewMaterial(new Color(0.88f, 0.87f, 0.83f));
            _crackMaterial = Greybox.NewMaterial(new Color(0.09f, 0.09f, 0.11f));
            _gridMaterial = Greybox.NewMaterial(new Color(0.74f, 0.73f, 0.70f));

            var strip = Greybox.Cube("pavement", _root, _pavementMaterial);
            strip.transform.localScale = new Vector3(SidewalkWidth, PavementTop * 2f, StripLength);
            _pavement = strip.transform;
        }

        public void Rebuild(SidewalkGenerator generator)
        {
            foreach (var l in _lines) Destroy(l);
            _lines.Clear();
            _lineAt.Clear();

            foreach (var g in _grid) Destroy(g);
            _grid.Clear();
            _gridAt.Clear();

            Advance(generator, 0f);
        }

        public void Advance(SidewalkGenerator generator, float playerX)
        {
            generator.EnsureAhead(playerX);

            // The strip is long enough that sliding it with the player is
            // cheaper than streaming geometry.
            _pavement.position = new Vector3(0f, 0f, playerX + StripLength * 0.5f - BehindPlayer);

            BuildGrid(playerX);

            foreach (float crack in generator.Cracks)
            {
                if (crack < playerX - BehindPlayer || crack > playerX + AheadOfPlayer) continue;
                if (_lineAt.Contains(crack)) continue;

                var line = Greybox.Cube($"crack_{crack:0.00}", _root, _crackMaterial);
                line.transform.localScale = new Vector3(SidewalkWidth * 1.01f, 0.02f, CrackWidth);
                line.transform.position = new Vector3(0f, PavementTop + 0.005f, crack);

                _lines.Add(line);
                _lineAt.Add(crack);
            }

            for (int i = _lines.Count - 1; i >= 0; i--)
            {
                if (_lines[i] == null || _lineAt[i] < playerX - BehindPlayer)
                {
                    if (_lines[i] != null) Destroy(_lines[i]);
                    _lines.RemoveAt(i);
                    _lineAt.RemoveAt(i);
                }
            }

            for (int i = _grid.Count - 1; i >= 0; i--)
            {
                if (_grid[i] == null || _gridAt[i] * GridSpacing < playerX - BehindPlayer)
                {
                    if (_grid[i] != null) Destroy(_grid[i]);
                    _grid.RemoveAt(i);
                    _gridAt.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Indexed off whole multiples of the spacing rather than stepping a
        /// float forward, so the ruler cannot drift out of alignment with the
        /// landings over a long run.
        /// </summary>
        void BuildGrid(float playerX)
        {
            int first = Mathf.CeilToInt((playerX - BehindPlayer) / GridSpacing);
            int last = Mathf.FloorToInt((playerX + AheadOfPlayer) / GridSpacing);

            for (int i = first; i <= last; i++)
            {
                if (_gridAt.Contains(i)) continue;

                float z = i * GridSpacing;
                var line = Greybox.Cube($"grid_{i}", _root, _gridMaterial);
                line.transform.localScale = new Vector3(SidewalkWidth * 0.9f, 0.02f, GridWidth);
                // Below the cracks: where the two coincide the crack wins.
                line.transform.position = new Vector3(0f, PavementTop + 0.003f, z);

                _grid.Add(line);
                _gridAt.Add(i);
            }
        }

        /// <summary>Height of the walking surface, for anything placed on it.</summary>
        public static float SurfaceY => PavementTop;
    }
}
