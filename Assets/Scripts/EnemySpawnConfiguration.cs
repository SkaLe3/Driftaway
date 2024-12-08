using UnityEngine;
using System.Collections.Generic;


[CreateAssetMenu(fileName = "EnemySpawnConfiguration", menuName = "Game/EnemySpawnConfiguration")]
public class EnemySpawnConfiguration : ScriptableObject
{
    public float MinimumTimeDifficultySecs;
    public List<EnemySpawnPosition> EnemySpawnPositions;
}

[System.Serializable]
public class EnemySpawnPosition
{
    public Vector3 Position;
    public EnemyAI EnemyPrefab;
}