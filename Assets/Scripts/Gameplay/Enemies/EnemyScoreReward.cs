using UnityEngine;

[DisallowMultipleComponent]
public sealed class EnemyScoreReward : MonoBehaviour
{
    [SerializeField] private int _scoreReward = 10;

    public int ScoreReward => Mathf.Max(0, _scoreReward);
}
