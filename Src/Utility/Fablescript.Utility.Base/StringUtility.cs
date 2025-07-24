using System.Security.Cryptography;
using System.Text;

namespace Fablescript.Utility.Base
{
  public class StringUtility
  {
    public static readonly char[] AlfanumericCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray();


    public static string GetRandomStringFromCharacterSet(int length, char[] allowedCharacters)
    {
      StringBuilder s = new StringBuilder();
      for (int i = 0; i < length; i++)
      {
        int index = RandomNumberGenerator.GetInt32(allowedCharacters.Length);
        s.Append(allowedCharacters[index]);
      }

      return s.ToString();
    }
  }
}
