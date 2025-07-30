using Fablescript.Utility.Base;
using Microsoft.CSharp.RuntimeBinder;
using System.Dynamic;
using System.Globalization;
using System.Reflection;

namespace Fablescript.Core.Engine
{
  /// <summary>
  /// Dynamic value that yields default(T) when converted to a required type T.
  /// If constructed with a non-null value, it tries to convert that value first.
  /// </summary>
  sealed class DynamicValue : DynamicObject
  {
    public static readonly DynamicValue Null = new DynamicValue(null);

    private readonly object? _value;

    public DynamicValue(object? value) => _value = value;


    public override bool TryConvert(ConvertBinder binder, out object? result)
    {
      var targetType = binder.Type;
      var nonNullable = Nullable.GetUnderlyingType(targetType) ?? targetType;

      // If we actually have a value, try to return/convert it.
      if (_value is not null)
      {
        // If already assignable, just return it.
        if (targetType.IsInstanceOfType(_value))
        {
          result = _value;
          return true;
        }

        // Try common IConvertible-based conversions (numbers, strings, decimals, etc.).
        try
        {
          if (_value is IConvertible)
          {
            result = Convert.ChangeType(_value, nonNullable, CultureInfo.CurrentCulture);
            return true;
          }
        }
        catch
        {
          // fall through to defaults
        }
      }

      // No value (or conversion failed):
      // - For nullable T? → null
      // - For value types → default(T)
      // - For reference types → null
      if (Nullable.GetUnderlyingType(targetType) is not null)
      {
        result = null; // T? gets null
        return true;
      }

      result = nonNullable.IsValueType ? Activator.CreateInstance(nonNullable) : null;
      return true;
    }

    // If someone chains deeper (e.g., obj.Missing.Something), keep returning defaulting dynamics.
    public override bool TryGetMember(GetMemberBinder binder, out object? result)
    {
      if (_value == null)
      {
        result = DynamicValue.Null;
      }
      else if (_value is DynamicObject dobj)
      {
        try
        {
          dobj.TryGetMember(binder, out result);
          result = result == null ? DynamicValue.Null : new DynamicValue(result);
        }
        catch (RuntimeBinderException)
        {
          result = DynamicValue.Null;
        }
      }
      else
      {
        var flags = BindingFlags.Instance | BindingFlags.Public;
        var t = _value.GetType();
        var p = t.GetProperty(binder.Name, flags);
        var f = t.GetField(binder.Name, flags);
        if (p is not null && p.GetIndexParameters().Length == 0)
        {
          result = new DynamicValue(p.GetValue(_value));
        }
        else if (f != null)
        {
          result = new DynamicValue(f.GetValue(_value));
        }
        else
        {
          result = DynamicValue.Null;
        }
      }
      return true;
    }


    public override string ToString() => _value?.ToString() ?? "";
  }
}
