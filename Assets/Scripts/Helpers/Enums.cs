namespace Checkers.Helpers
{
	/// <summary> Типы партии в шашки.</summary>
	public enum CheckersPartyMode {
		Simple, // Обычный 
		Record, // Обычный с записью 
		Replay  // Воспроизведение 
	}

	/// <summary>Тип активности актора (игрового объекта).</summary>
	public enum ActorActionType { Select, Move, Remove }
}
