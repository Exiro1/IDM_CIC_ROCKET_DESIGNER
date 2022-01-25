using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketDesigner
{
    public class SolidWorksException : Exception
    {

        public SolidWorksException()
        {

        }

        public SolidWorksException(String ex) : base(ex)
        {

        }

        public SolidWorksException(String ex, Exception e) : base(ex, e)
        {

        }

    }
}
