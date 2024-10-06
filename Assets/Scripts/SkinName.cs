using System;
using System.Collections.Generic;
using System.Text;

internal struct SkinName : IEquatable<SkinName>
{
    public readonly string Module, Name;

    public SkinName(string module, string name)
    {
        Module = module;
        Name = name;
    }

    public override bool Equals(object other)
    {
        return other is SkinName && Equals((SkinName)other);
    }

    public bool Equals(SkinName other)
    {
        return Module == other.Module && Name == other.Name;
    }

    public override int GetHashCode()
    {
        int hashCode = 1044259399;
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Module);
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
        return hashCode;
    }

    public static bool operator ==(SkinName left, SkinName right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(SkinName left, SkinName right)
    {
        return !(left == right);
    }

    private const char Separator = '.';

    public override string ToString()
    {
        return new StringBuilder(Module.Length + Name.Length + 1)
            .Append(Module)
            .Append(Separator)
            .Append(Name)
            .ToString();
    }

    public static bool TryParse(string input, out SkinName result)
    {
        var split = input.IndexOf(Separator);
        if (split != input.LastIndexOf(Separator))
        {
            result = new SkinName();
            return false;
        }
        result = new SkinName(input.Substring(0, split), input.Substring(split + 1));
        return true;
    }
}

