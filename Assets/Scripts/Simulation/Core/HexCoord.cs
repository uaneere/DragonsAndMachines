using System;
using System.Collections.Generic;

namespace DragonsAndMachines.Core
{
    public readonly struct HexCoord : IEquatable<HexCoord>
    {
        public static readonly HexCoord[] NeighborDeltas =
        {
            new HexCoord(1, 0),
            new HexCoord(-1, 0),
            new HexCoord(0, 1),
            new HexCoord(0, -1),
            new HexCoord(1, 1),
            new HexCoord(-1, -1)
        };

        public int Q { get; }
        public int R { get; }

        public HexCoord(int q, int r)
        {
            Q = q;
            R = r;
        }

        public string CellId => $"hex:{Q}:{R}";

        public HexCoord Neighbor(int index) => this + NeighborDeltas[index];

        public IEnumerable<HexCoord> Neighbors()
        {
            for (var i = 0; i < NeighborDeltas.Length; i++)
                yield return this + NeighborDeltas[i];
        }

        public static HexCoord operator +(HexCoord a, HexCoord b) => new HexCoord(a.Q + b.Q, a.R + b.R);

        public static HexCoord operator -(HexCoord a, HexCoord b) => new HexCoord(a.Q - b.Q, a.R - b.R);

        public bool Equals(HexCoord other) => Q == other.Q && R == other.R;

        public override bool Equals(object obj) => obj is HexCoord other && Equals(other);

        public override int GetHashCode() => (Q * 397) ^ R;

        public override string ToString() => $"({Q},{R})";

        public static bool operator ==(HexCoord a, HexCoord b) => a.Equals(b);

        public static bool operator !=(HexCoord a, HexCoord b) => !a.Equals(b);
    }
}
