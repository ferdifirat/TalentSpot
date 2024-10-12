namespace TalentSpot.Application.Constants
{
    public class ResponseMessageConstants
    {
        public const string BenefitAlreadyExists = "Yan hak zaten mevcut.";
        public const string BenefitNotFound = "Yan hak bulunamadı.";
        public const string DuplicateBenefit = "Aynı yan hak zaten başka bir kayıt olarak mevcut.";
        public const string InternalServerError = "İç sunucu hatası.";
        public const string NoCompaniesFound = "İlan bulunamadı.";
        public const string CompanyNotFound = "Şirket bulunamadı.";
        public const string CompanyUpdateFailed = "Şirket bilgileri güncellenemedi.";
        public const string ForbiddenWordAlreadyExists = "Yasaklı kelime zaten mevcut.";
        public const string AnotherForbiddenWordExists = "Aynı kelime zaten başka bir kayıt olarak mevcut.";
        public const string NoAllowedJobPostings = "Bu şirketin ilan yayınlama hakkı kalmamıştır.";
        public const string InvalidWorkType = "Geçersiz çalışma türü.";
        public const string InvalidBenefits = "Geçersiz yan haklar.";
        public const string JobNotFound = "İlan bulunamadı.";
        public const string JobCreationError = "Bir hata oluştu: {0}";
        public const string JobUpdateError = "İlan güncellenirken hata oluştu.";
        public const string NoActiveJobs = "Belirtilen tarih aralığında aktif iş ilanı bulunamadı.";
        public const string UserAlreadyExists = "Bu telefon numarasıyla kayıtlı bir firma bulunmaktadır.";
        public const string InvalidCredentials = "Invalid phone number or password.";
        public const string RegistrationFailed = "User registration failed.";
        public const string WorkTypeAlreadyExists = "Çalışma türü zaten mevcut.";
        public const string WorkTypeExistsElsewhere = "Aynı çalışma türü zaten başka bir kayıt olarak mevcut.";
    }
}
