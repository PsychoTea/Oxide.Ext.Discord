using System.Text;

namespace Oxide.Ext.Discord.Helpers
{
    class HttpUtility
    {
        public static string UrlEncode(byte[] bytes)
        {
            int num = 0;
            int num1 = 0;

            for (int i = 0; i < bytes.Length; i++)
            {
                char chr = (char)bytes[i];
                if (chr == ' ')
                {
                    num++;
                }
                else if (!HttpUtility.IsSafe(chr))
                {
                    num1++;
                }
            }

            byte[] hex = new byte[bytes.Length + num1 * 2];
            int num2 = 0;

            for (int j = 0; j < bytes.Length; j++)
            {
                byte num3 = bytes[j];
                char chr1 = (char)num3;

                if (HttpUtility.IsSafe(chr1))
                {
                    int num4 = num2;
                    num2 = num4 + 1;
                    hex[num4] = num3;
                }
                else if (chr1 != ' ')
                {
                    int num5 = num2;
                    num2 = num5 + 1;
                    hex[num5] = 37;
                    int num6 = num2;
                    num2 = num6 + 1;
                    hex[num6] = (byte)HttpUtility.IntToHex(num3 >> 4 & 15);
                    int num7 = num2;
                    num2 = num7 + 1;
                    hex[num7] = (byte)HttpUtility.IntToHex(num3 & 15);
                }
                else
                {
                    int num8 = num2;
                    num2 = num8 + 1;
                    hex[num8] = 43;
                }
            }

            return Encoding.ASCII.GetString(hex);
        }

        internal static bool IsSafe(char ch)
        {
            if (ch >= 'a' && ch <= 'z' || 
                ch >= 'A' && ch <= 'Z' || 
                ch >= '0' && ch <= '9')
            {
                return true;
            }

            char chr = ch;

            if (chr != '!')
            {
                switch (chr)
                {
                    case '\'':
                    case '(':
                    case ')':
                    case '*':
                    case '-':
                    case '.':
                        {
                            break;
                        }
                    case '+':
                    case ',':
                        {
                            return false;
                        }
                    default:
                        {
                            if (chr != '\u005F')
                            {
                                return false;
                            }
                            else
                            {
                                break;
                            }
                        }
                }
            }

            return true;
        }

        internal static char IntToHex(int n)
        {
            if (n <= 9)
            {
                return (char)(n + 48);
            }

            return (char)(n - 10 + 97);
        }
    }
}
