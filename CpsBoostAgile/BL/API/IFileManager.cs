using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CpsBoostAgile.BL.API
{
    public interface IFileManager
    {
        string ExportRetrospective(string retrospectiveId);
        string ExportPokerPlanning(string id);
    }
}
