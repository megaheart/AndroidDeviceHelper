using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AndroidDeviceHelper.ViewModel
{
    public class LinuxPath
    {
        public static string Combine(string path1, string path2)
        {
            path1 = path1.Trim();
            path2 = path2.Trim();
            if (path2[0] == '/') {
                return path2;
            }
            ReadOnlySpan<char> p1 = path1.AsSpan();
            ReadOnlySpan<char> p2 = path2.AsSpan();

            int len1 = path1.Length;
            int len2 = path2.Length;

            Span<char> p = stackalloc char[len1 + len2 + 1];
            int len = len1;
            p1.CopyTo(p);

            if (p1[len1 - 1] != '/')
            {
                p[len] = '/';
                len++;
            }
            
            p2.CopyTo(p.Slice(len, len2));
            len += len2;
            //if (p[len - 1] == '/')
            //{
            //    p[len] = '/';
            //    len++;
            //}
            //var s = p.ToString();
            return p.Slice(0, len).ToString();
        }
        public static string? GetDirectoryName(string path)
        {
            path = path.Trim();
            if (string.IsNullOrEmpty(path) || path == "/")
            {
                return null;
            }

            int idx = path.Length - 1;
            ReadOnlySpan<char> p = path.AsSpan();

            if (p[idx] == '/') idx--;

            while(idx > -1)
            {
                if (p[idx] == '/') { 
                    break; 
                }
                idx--;
            }

            if (idx == 0)
            {
                return "/";
            }
            else if (idx == -1)
            {
                return "/";
            }

            return path.Remove(idx);
        }

        public static string NormalizeDirectoryName(string path)
        {
            if(path == null) throw new ArgumentNullException("path");
            ReadOnlySpan<char> p = path.AsSpan();
            Span<char> s = stackalloc char[p.Length];
            int i = 0, len = 0;
            //char last = '\0';
            while(i < p.Length)
            {
                if (p[i] == '/' || p[i] == '\\')
                {
                    while(i + 1 < p.Length && (p[i + 1] == '/' || p[i + 1] == '\\'))
                    {
                        i++;
                    }
                }

                if (p[i] == '\\') s[len++] = '/';
                else s[len++] = p[i++];
            }

            if(len > 1 && s[len - 1] == '/') len--;

            return s.Slice(0, len).ToString();
        }
    }
}
