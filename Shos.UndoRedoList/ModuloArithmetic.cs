namespace Shos.Collections;

///<summary>Modular arithmetic.</summary>
///<seealso href="https://en.wikipedia.org/wiki/Modular_arithmetic">Modular arithmetic - Wikipedia</seealso>
public struct ModuloArithmetic : IEquatable<ModuloArithmetic>
{
    public const int DefaultDivisor = 100;

    int value;

    /// <summary>Value (0 or more and less than <see cref="Divisor"/>, or -1 when invalid).</summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when setting value is less than 0 or (Divisor or more).</exception>
    public int Value {
        readonly get => value;
        set {
            if (value < 0 || value >= Divisor)
                throw new ArgumentOutOfRangeException(nameof(value), value, $"{nameof(Value)} must be 0 or more and less than {nameof(Divisor)} ({Divisor}).");
            this.value = value;
        }
    }

    public int Divisor { get; }
    public readonly bool IsValid => Value >= 0;
    public readonly ModuloArithmetic Previous => new(Divisor) { Value = PreviousValue };
    public readonly ModuloArithmetic Next     => new(Divisor) { Value = NextValue     };

    readonly int PreviousValue => Value == 0 ? Value + Divisor - 1 : Value - 1;
    readonly int NextValue     => (Value + 1) % Divisor;

    /// <summary>Initializes a new instance with the divisor.</summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when divisor is 1 or less.</exception>
    public ModuloArithmetic(int divisor = DefaultDivisor, bool isValid = true)
    {
        if (divisor <= 1)
            throw new ArgumentOutOfRangeException(nameof(divisor), divisor, $"{nameof(divisor)} must be 2 or more.");
        Divisor = divisor;
        value   = isValid ? 0 : -1;
    }

    #region IEquatable<T> implementation
    public readonly bool Equals(ModuloArithmetic item) => Value.Equals(item.Value);
    #endregion
    public override readonly bool Equals(object? item) => item is ModuloArithmetic moduloArithmetic && Equals(moduloArithmetic);
    public override readonly int GetHashCode() => Value.GetHashCode();

    public void MovePrevious() => Value = PreviousValue;
    public void MoveNext    () => Value = NextValue    ;

    public readonly ModuloArithmetic InvalidItem => new(Divisor, false);

    public static bool operator ==(ModuloArithmetic item1, ModuloArithmetic item2) => item1.Value == item2.Value;
    public static bool operator !=(ModuloArithmetic item1, ModuloArithmetic item2) => !(item1 == item2);
    public static bool operator ==(ModuloArithmetic item, int value) => item.Value == value;
    public static bool operator !=(ModuloArithmetic item, int value) => !(item == value);
    public static bool operator ==(int value, ModuloArithmetic item) => value == item.Value;
    public static bool operator !=(int value, ModuloArithmetic item) => !(value == item);
    public static ModuloArithmetic operator ++(ModuloArithmetic item) => new(item.Divisor) { Value = item.NextValue     };
    public static ModuloArithmetic operator --(ModuloArithmetic item) => new(item.Divisor) { Value = item.PreviousValue };

    /// <summary>Returns the distance from item2 to item1 (0 or more and less than Divisor).</summary>
    /// <exception cref="InvalidOperationException">Thrown when item1.Divisor and item2.Divisor are different.</exception>
    public static int operator -(ModuloArithmetic item1, ModuloArithmetic item2)
    {
        if (item1.Divisor != item2.Divisor)
            throw new InvalidOperationException($"{nameof(Divisor)}s are different ({item1.Divisor} and {item2.Divisor}).");

        var result = item1.Value - item2.Value;
        return result >= 0 ? result : result + item1.Divisor;
    }
}
