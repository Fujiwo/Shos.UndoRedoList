using System;

namespace Shos.Collections
{
    public struct ModuloArithmetic : IEquatable<ModuloArithmetic>
    {
        public const int DefaultDivisor = 100;

        int value;

        public int Value {
            get => value;
            set {
                if (value < 0 || value >= Divisor)
                    throw new ArgumentOutOfRangeException();
                this.value = value;
            }
        }

        public int Divisor { get; private set; }
        public bool IsValid => Value >= 0;
        public ModuloArithmetic Previous => new ModuloArithmetic(Divisor) { Value = PreviousValue };
        public ModuloArithmetic Next     => new ModuloArithmetic(Divisor) { Value = NextValue     };

        int PreviousValue => Value == 0 ? Value + Divisor - 1 : Value - 1;
        int NextValue     => (Value + 1) % Divisor;

        public ModuloArithmetic(int divisor = DefaultDivisor, bool isValid = true)
        {
            if (divisor <= 1)
                throw new ArgumentOutOfRangeException();
            Divisor = divisor;
            value   = isValid ? 0 : -1;
        }

        public bool Equals(ModuloArithmetic item) => this.Equals(item);
        public override bool Equals(object item) => Value.Equals(((ModuloArithmetic)item).Value);
        public override int GetHashCode() => Value.GetHashCode();

        public void MovePrevious() => Value = PreviousValue;
        public void MoveNext    () => Value = NextValue    ;

        public ModuloArithmetic InvalidItem => new ModuloArithmetic(this.Divisor, false);

        public static bool operator ==(ModuloArithmetic item1, ModuloArithmetic item2) => item1.Value == item2.Value;
        public static bool operator !=(ModuloArithmetic item1, ModuloArithmetic item2) => !(item1 == item2);
        public static bool operator ==(ModuloArithmetic item, int value) => item.Value == value;
        public static bool operator !=(ModuloArithmetic item, int value) => !(item == value);
        public static bool operator ==(int value, ModuloArithmetic item) => value == item.Value;
        public static bool operator !=(int value, ModuloArithmetic item) => !(value == item);
        public static ModuloArithmetic operator ++(ModuloArithmetic item) => new ModuloArithmetic(item.Divisor) { Value = item.NextValue     };
        public static ModuloArithmetic operator --(ModuloArithmetic item) => new ModuloArithmetic(item.Divisor) { Value = item.PreviousValue };
        public static int operator -(ModuloArithmetic item1, ModuloArithmetic item2)
        {
            if (item1.Divisor != item2.Divisor)
                throw new InvalidOperationException();

            var result = item1.Value - item2.Value;
            return result >= 0 ? result : result + item1.Divisor;
        }
    }
}
