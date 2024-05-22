using System; 
using System.Collections.Generic;

using HTTP_NET_Project;

public abstract class mUtils
{
    /// <summary>
    /// Parse into a Dictionary<string,string> the post attributes in the format "att1=hello&att2=world"
    /// </summary>
    /// <param name="postAtts">post attributes in string followiung the format "att1=hello&att2=world"</param>
    /// <returns>Dictionary containint the attribures or null in case that an error has ocurred or is not formated correctly</returns>
    public static Dictionary<string, string> ParsePostAttributes(string postAtts)
    {
        if (postAtts.Length <= 1)
        {
            Console.WriteLine(" length less than 1");
            return null;
        }

        Dictionary<string, string> dic = new Dictionary<string, string>(); 
        
        // Check if single attribute
        if (postAtts.IndexOf("&") <= -1)
        {
            if (postAtts.IndexOf("=") <= -1) {
                Console.WriteLine("Error siungle attrr");
                return null; 
            }

            string[] attribute = postAtts.Split("=", StringSplitOptions.RemoveEmptyEntries);
            dic.Add(attribute[0], attribute[1]);
            return dic; 
        }
        
        string[] attributes = postAtts.Split("&");

        foreach (string att in attributes)
        {
            string[] split = att.Split("=", StringSplitOptions.RemoveEmptyEntries);
            if (split.Length != 2)
            {
                Console.WriteLine("Error double attrr: " + att + " current ");
                return null; }
            
            dic.Add(split[0], split[1]);
        }

        return dic; 
    }
    
    public static string GetUntilOrEmpty(string text, string stopAt)
    {
        if (!String.IsNullOrWhiteSpace(text))
        {
            int charLocation = text.IndexOf(stopAt, StringComparison.Ordinal);

            if (charLocation > 0)
            {
                return text.Substring(0, charLocation);
            }
        }

        return String.Empty;
    }

    /// <summary>
    /// Parses the HTTP version speficied as HTTP/1.1 into a float containing the version
    /// </summary>
    /// <param name="httpVersion">Http version in  header format "HTTP/1.1"</param>
    /// <returns>Http version in float variable</returns>
    public static float ParseHttpVersion(string httpVersion)
    {
        string[] parts = httpVersion.Split("/");

        if (parts.Length >= 2)
        {
            try
            {
                return float.Parse(parts[1]); 
            }
            catch (Exception e)
            {
                return 0; 
            }
        }

        return 0; 
    }
    
    /// <summary>
    /// Parse headers by separating the different header options and create a dictionary with them.
    /// So as example 'Accept:' header variable will be stores as dictionary['Accept'] = ...
    /// </summary>
    /// <param name="content">Header variable section in string</param>
    /// <returns>Returns a Dictionary<string, string> that has all the header variables parsed</returns>
    public static Dictionary<string, string> ParseHeaders(string content)
    {
        String[] headerLines = content.Split('\r', '\n');
        
        Dictionary<string,string> headerValues = new Dictionary<string, string>();
        
        foreach (string headerLine in headerLines)
        {
            string headerDetail = headerLine.Trim();
            int delimiterIndex = headerLine.IndexOf(':');
            if (delimiterIndex >= 0)
            {
                String headerName =     headerLine.Substring(0, delimiterIndex).Trim();
                String headerValue =    headerLine.Substring(delimiterIndex + 1).Trim();
                if (!headerValues.ContainsKey(headerName))
                {
                    headerValues.Add(headerName, headerValue);
                }
                else
                {
                    //Console.ForegroundColor = ConsoleColor.Red; 
                    //Console.WriteLine("Exception handled> Already header attributes contains: " + headerName);
                }
            }
            else
            {
                if (headerLine == "\r\n" || headerLine == "\n")
                {
                    break;
                }
            }
        }

        return headerValues; 
    }

    public static string TakeHeaderAttributes(string[] header)
    {
        
        int startIndex = 1;
        int endIndex = 1;

        for (int i = startIndex; i < header.Length; i++)
        {
            if (header[i].Length == 0 || header[i] == "")
            {
                endIndex = i;
                break; 
            }
        }

        string headerAttributes = "";
        for (int i = startIndex; i < endIndex; i++)
        {
            headerAttributes += header[i]; 
        }

        return headerAttributes; 
    }

    /*
    public static EncryptPassword (string Password, string salt) //-------------------------_
    {
        //uses salt to encrypt the password by adding random characters to it
        string salt1 = salt.Substring(0, 2);
        string salt2 = salt.Substring(2, 2);
        string salt3 = salt.Substring(4, 2);
        Password.Insert(Password.Length / 2, salt2); //inserts the salt2 in the middle of the password
        return salt1 + password + salt3; //encrypted password
    }

    public static DecryptPassword (string encryptedPassword)
    {
        //removes salt
        encryptedPassword = Password.Remove((Password.Length/2)-1, 2); //The middle -1 as the salt was inserted in the middle and now the original middle has been moved
        return encryptedPassword.Substring(2, Password.Length - 4);
    }//-------------------------^
    */

    public static string EncryptPassword(string password, string salt)
    {
        // Verify that the salt is at least 6 characters long.
        if (salt.Length < 6)
        {
            throw new ArgumentException("Salt must be at least 6 characters long.");
        }

        // Use salt to encrypt the password by adding random characters to it.
        string salt1 = salt.Substring(0, 2);
        string salt2 = salt.Substring(2, 2);
        string salt3 = salt.Substring(4, 2);

        // Insert salt2 in the middle of the password.
        password = password.Insert(password.Length / 2, salt2);

        // Return encrypted password.
        return salt1 + password + salt3;
    }

    public static string DecryptPassword(string encryptedPassword)
    {
        // Verify that the encrypted password is at least 8 characters long.
        if (encryptedPassword.Length < 8)
        {
            throw new ArgumentException("Encrypted password is too short.");
        }

        // Delete salt1 and salt3.
        encryptedPassword = encryptedPassword.Substring(2, encryptedPassword.Length - 4);

        // Delete salt2
        encryptedPassword = encryptedPassword.Remove((encryptedPassword.Length / 2) - 1, 2);

        return encryptedPassword;
    }

    /// <summary>
    /// Return true if contains at least one of the substrings given in the array
    /// </summary>
    /// <param name="str">String to check if contains one of them</param>
    /// <param name="substr">subtstring that will be check if one of them is located within the str</param>
    /// <returns></returns>
    public static bool StringContainsOne(string str, string[] substr)
    {
        foreach (string s in substr)
        {
            if (str.Contains(s))
            {
                return true; 
            }
        }

        return false; 
    }
    

}