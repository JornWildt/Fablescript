using System.ComponentModel.DataAnnotations;

namespace Fablescript.Utility.Base
{
  public static class ObjectValidator
  {
    public static void ValidateObject(string messagePrefix, object src)
    {
      var context = new ValidationContext(src);
      try
      {
        Validator.ValidateObject(src, context, validateAllProperties: true);
      }
      catch (ValidationException ex)
      {
        throw new ValidationException(
          new ValidationResult(messagePrefix + ex.Message, ex.ValidationResult.MemberNames),
          ex.ValidationAttribute,
          ex.Value);
      }
    }
  }
}
