namespace _Scripts.Common
{
    public interface IHealth
    {
        int CurrentHealth { get; }
        int MaxHealth { get; }
        bool IsAlive { get; }
        bool IsDead { get; } 
        
        void SetUp(int newMaxHealth);

        void Damage(int amount);
        void Heal(int amount);
    }
}