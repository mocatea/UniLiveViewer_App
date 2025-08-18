using System;

namespace UniLiveViewer.ValueObject
{
    /// <summary>
    /// アクター種に関係なく完全ユニークなID
    /// ステージ生成順、常時インクリメントとする
    /// MEMO: null使いたいのでstructにしない
    /// </summary>
    public sealed class InstanceId : IEquatable<InstanceId>
    {
        public int Id { get; }

        public InstanceId(int id)
        {
            Id = id;
        }

        public bool Equals(InstanceId other)
            => other is not null && Id == other.Id;

        public override bool Equals(object? obj)
            => obj is InstanceId other && Equals(other);

        public override int GetHashCode() => Id;

        public static bool operator ==(InstanceId left, InstanceId right)
            => left is null ? right is null : left.Equals(right);

        public static bool operator !=(InstanceId left, InstanceId right)
            => !(left == right);

        public override string ToString() => Id.ToString();
    }
}
