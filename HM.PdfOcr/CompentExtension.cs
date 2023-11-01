using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HM.PdfOcr
{
    public static class CompentExtension
    {
        public static TResult SafeInvoke<T, TResult>(this T isi, Func<T, TResult> call) where T : ISynchronizeInvoke
        {
            if (isi.InvokeRequired)
            {
                IAsyncResult result = isi.BeginInvoke(call, new object[] { isi });
                object endResult = isi.EndInvoke(result); return (TResult)endResult;
            }
            else
            {
                return call(isi);
            }
        }

        public static void SafeInvoke<T>(this T isi, Action<T> call) where T : ISynchronizeInvoke
        {
            if (isi.InvokeRequired)
            {
                isi.BeginInvoke(call, new object[] { isi });
            }
            else
            {
                call(isi);
            }
        }

        public static void SafeInvoke<T>(this T isi, object obj, Action<T, object> call) where T : ISynchronizeInvoke
        {
            if (isi.InvokeRequired)
            {
                isi.BeginInvoke(call, new object[] { isi, obj });
            }
            else
            {
                call(isi, obj);
            }
        }
    }
}
