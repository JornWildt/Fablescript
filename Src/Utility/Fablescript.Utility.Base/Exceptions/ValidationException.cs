﻿namespace Fablescript.Utility.Base.Exceptions
{
  public class ValidationException : Exception
  {
    public ValidationException() { }

    public ValidationException(string message)
      : base(message) { }

    public ValidationException(string message, Exception innerException)
      : base(message, innerException) { }
  }
}
