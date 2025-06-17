using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class PCGEnemyStats
{
    public float HP;
    public float Damage;
    public float AttackRate;
    public float AttackRange;
    public float MovementSpeed;

    private PCGConfigValuesScriptableObject _config;

    public PCGEnemyStats(PCGConfigValuesScriptableObject config)
    {
        _config = config;
        HP = Random.Range(config.MinHp, config.MaxHp);
        Damage = Random.Range(config.MinDamage, config.MaxDamage);
        AttackRate = Random.Range(config.MinAttackRate, config.MaxAttackRate);
        AttackRange = Random.Range(config.MinAttackRange, config.MaxAttackRange);
        MovementSpeed = Random.Range(config.MinMovementSpeed, config.MaxMovementSpeed);
    }

    public PCGEnemyStats(PCGEnemyStats other)
    {
        HP = other.HP;
        Damage = other.Damage;
        AttackRate = other.AttackRate;
        AttackRange = other.AttackRange;
        MovementSpeed = other.MovementSpeed;
        _config = other._config;
    }

    public void PrintStats()
    {
        Debug.Log($"HP = {HP}, Damage = {Damage}, AttackRate = {AttackRate}, AttackRange = {AttackRange}, MovementSpeed = {MovementSpeed}");
    }

    // NORMALIZACIÓN
    private float Normalize(float value, float min, float max)
    {
        return (value - min) / (max - min);
    }

    public float NormalizedHP => Normalize(HP, _config.MinHp, _config.MaxHp);
    public float NormalizedDamage => Normalize(Damage, _config.MinDamage, _config.MaxDamage);
    public float NormalizedAttackRate => Normalize(AttackRate, _config.MinAttackRate, _config.MaxAttackRate);
    public float NormalizedAttackRange => Normalize(AttackRange, _config.MinAttackRange, _config.MaxAttackRange);
    public float NormalizedMovementSpeed => Normalize(MovementSpeed, _config.MinMovementSpeed, _config.MaxMovementSpeed);

    // FUNCIÓN DE DIFICULTAD V2
    public float GetDifficultyV2()
    {
        return HP + (Damage * (1f / AttackRate)) + AttackRange + MovementSpeed;
    }

    // FUNCIÓN DE BALANCE
    public float GetBalanceScore()
    {
        float sum = (NormalizedHP - 0.5f) +
                    (NormalizedDamage - 0.5f) +
                    (NormalizedAttackRate - 0.5f) +
                    (NormalizedAttackRange - 0.5f) +
                    (NormalizedMovementSpeed - 0.5f);

        return 1f - Mathf.Abs(Mathf.Clamp(sum, -1f, 1f));
    }

    // FUNCIÓN TOTAL
    public float GetTotalScore(float difficultyWeight, float balanceWeight)
    {
        float difficultyScore = Normalize(GetDifficultyV2(), _config.MinTotalDifficulty, _config.MaxTotalDifficulty);
        float balanceScore = GetBalanceScore();
        return difficultyScore * difficultyWeight + balanceScore * balanceWeight;
    }

    // GENERAR VECINOS
    public List<PCGEnemyStats> GetNeighbors()
    {
        List<PCGEnemyStats> result = new List<PCGEnemyStats>();

        void AddNeighbor(string fieldName, float currentValue, float step, float min, float max)
        {
            float minus = Mathf.Max(currentValue - step, min);
            float plus = Mathf.Min(currentValue + step, max);

            PCGEnemyStats statMinus = new PCGEnemyStats(this);
            statMinus.GetType().GetField(fieldName).SetValue(statMinus, minus);
            result.Add(statMinus);

            PCGEnemyStats statPlus = new PCGEnemyStats(this);
            statPlus.GetType().GetField(fieldName).SetValue(statPlus, plus);
            result.Add(statPlus);
        }

        AddNeighbor(nameof(HP), HP, _config.HpStepDistance, _config.MinHp, _config.MaxHp);
        AddNeighbor(nameof(Damage), Damage, _config.DamageStepDistance, _config.MinDamage, _config.MaxDamage);
        AddNeighbor(nameof(AttackRate), AttackRate, _config.AttackRateStepDistance, _config.MinAttackRate, _config.MaxAttackRate);
        AddNeighbor(nameof(AttackRange), AttackRange, _config.AttackRangeStepDistance, _config.MinAttackRange, _config.MaxAttackRange);
        AddNeighbor(nameof(MovementSpeed), MovementSpeed, _config.MovementSpeedStepDistance, _config.MinMovementSpeed, _config.MaxMovementSpeed);

        return result;
    }
}
