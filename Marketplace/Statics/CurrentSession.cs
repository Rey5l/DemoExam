using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Marketplace.Model;

namespace Marketplace.Statics
{
    public static class CurrentSession
    {
        public static Users CurrentUser { get; set; }
    }
}
