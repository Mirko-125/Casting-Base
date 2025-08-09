using CastingBase;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CastingBase
{
    public class CastingDirector : User
    {
        public Production? Production { get; set; }
    }
}