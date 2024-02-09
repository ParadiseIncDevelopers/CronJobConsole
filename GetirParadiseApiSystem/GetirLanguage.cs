using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetirParadiseApiSystem
{
    public abstract class GetirLanguage
    {
        public abstract KeyValuePair<string, string> TurkishValue { get; set; }
        public abstract KeyValuePair<string, string> EnglishValue { get; set; }
    }
}
