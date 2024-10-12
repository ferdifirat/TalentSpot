using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TalentSpot.Application.Constants
{
    public class ResponseMessages
    {
        public const string BenefitAlreadyExists = "Yan hak zaten mevcut.";
        public const string BenefitNotFound = "Yan hak bulunamadı.";
        public const string DuplicateBenefit = "Aynı yan hak zaten başka bir kayıt olarak mevcut.";
        public const string InternalServerError = "İç sunucu hatası.";
    }
}
