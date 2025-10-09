using CertiFlowTeam.Enums;

namespace CertiFlowTeam.Constants
{
    public static class SqlConstants
    {
        #region Table Names

        public static class TableNames
        {
            public const string Kullanicilar = "Kullanicilar";
            public const string Ayarlar = "Ayarlar";
            public const string Belgeler = "Belgeler";
            public const string BelgeLog = "BelgeLog";
            public const string Firmalar = "Firmalar";
        }

        #endregion

        #region Create Table Scripts

        public static class CreateTableScripts
        {
            public static string Kullanicilar => $@"
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='{TableNames.Kullanicilar}' AND xtype='U')
CREATE TABLE {TableNames.Kullanicilar} (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    AdSoyad NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) UNIQUE NOT NULL,
    Sifre NVARCHAR(255) NOT NULL,
    Rol INT NOT NULL DEFAULT {(int)Role.User},
    FirmaId INT,
    Telefon NVARCHAR(20),
    Aktif BIT DEFAULT 1,
    OlusturmaTarihi DATETIME2 DEFAULT GETDATE(),
    SonGirisTarihi DATETIME2,
    GuncellenmeTarihi DATETIME2,
    Silindi BIT DEFAULT 0,
    FOREIGN KEY (FirmaId) REFERENCES {TableNames.Firmalar}(Id)
);";

            public static string Firmalar => $@"
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='{TableNames.Firmalar}' AND xtype='U')
CREATE TABLE {TableNames.Firmalar} (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    FirmaAdi NVARCHAR(255) NOT NULL,
    VergiNo NVARCHAR(50),
    Adres NVARCHAR(500),
    Telefon NVARCHAR(20),
    Email NVARCHAR(100),
    YetkiliKisi NVARCHAR(100),
    YetkiliTelefon NVARCHAR(20),
    Aktif BIT DEFAULT 1,
    OlusturmaTarihi DATETIME2 DEFAULT GETDATE(),
    GuncellenmeTarihi DATETIME2,
    Silindi BIT DEFAULT 0
);";

            public static string Belgeler => $@"
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='{TableNames.Belgeler}' AND xtype='U')
CREATE TABLE {TableNames.Belgeler} (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    BelgeAdi NVARCHAR(255) NOT NULL,
    BelgeTuru NVARCHAR(100),
    BelgeNo NVARCHAR(50),
    DosyaYolu NVARCHAR(500) NOT NULL,
    DosyaBoyutu BIGINT,
    Aciklama NVARCHAR(1000),
    OnayDurumu INT NOT NULL DEFAULT {(int)DocumentApprovalStatus.Pending},
    YukleyenKullaniciId INT NOT NULL,
    OnaylayacakKullaniciId INT,
    OnaylayanKullaniciId INT,
    OnayTarihi DATETIME2,
    RedNedeni NVARCHAR(500),
    FirmaId INT,
    OlusturmaTarihi DATETIME2 DEFAULT GETDATE(),
    GuncellenmeTarihi DATETIME2,
    Silindi BIT DEFAULT 0,
    FOREIGN KEY (YukleyenKullaniciId) REFERENCES {TableNames.Kullanicilar}(Id),
    FOREIGN KEY (OnaylayacakKullaniciId) REFERENCES {TableNames.Kullanicilar}(Id),
    FOREIGN KEY (OnaylayanKullaniciId) REFERENCES {TableNames.Kullanicilar}(Id),
    FOREIGN KEY (FirmaId) REFERENCES {TableNames.Firmalar}(Id)
);";

            public static string BelgeLog => $@"
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='{TableNames.BelgeLog}' AND xtype='U')
CREATE TABLE {TableNames.BelgeLog} (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    BelgeId INT NOT NULL,
    KullaniciId INT NOT NULL,
    Islem NVARCHAR(100) NOT NULL,
    Aciklama NVARCHAR(500),
    EskiDurum INT,
    YeniDurum INT,
    EskiDegerler NVARCHAR(MAX),
    YeniDegerler NVARCHAR(MAX),
    IPAdresi NVARCHAR(45),
    IslemTarihi DATETIME2 DEFAULT GETDATE(),
    Basarili BIT DEFAULT 1,
    HataMesaji NVARCHAR(1000),
    Silindi BIT DEFAULT 0,
    FOREIGN KEY (BelgeId) REFERENCES {TableNames.Belgeler}(Id),
    FOREIGN KEY (KullaniciId) REFERENCES {TableNames.Kullanicilar}(Id)
);";

            public static string Ayarlar => $@"
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='{TableNames.Ayarlar}' AND xtype='U')
CREATE TABLE {TableNames.Ayarlar} (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    AyarAnahtari NVARCHAR(50) UNIQUE NOT NULL,
    GoruntulemeAdi NVARCHAR(100) NOT NULL,
    AyarDegeri NVARCHAR(255) NOT NULL,
    VarsayilanDeger NVARCHAR(255) NOT NULL,
    VeriTipi NVARCHAR(20) NOT NULL,
    Aciklama NVARCHAR(500),
    Aktif BIT DEFAULT 1,
    SistemAyari BIT DEFAULT 0,
    OlusturmaTarihi DATETIME2 DEFAULT GETDATE(),
    GuncellenmeTarihi DATETIME2
);

IF NOT EXISTS (SELECT * FROM {TableNames.Ayarlar} WHERE AyarAnahtari = 'maks_dosya_boyutu_mb')
INSERT INTO {TableNames.Ayarlar} (AyarAnahtari, GoruntulemeAdi, AyarDegeri, VarsayilanDeger, VeriTipi, Aciklama, SistemAyari)
VALUES ('maks_dosya_boyutu_mb', 'Maksimum Dosya Boyutu (MB)', '50', '50', 'int', 'Yüklenebilecek maksimum dosya boyutu (MB cinsinden)', 1);

IF NOT EXISTS (SELECT * FROM {TableNames.Ayarlar} WHERE AyarAnahtari = 'izin_verilen_dosya_turleri')
INSERT INTO {TableNames.Ayarlar} (AyarAnahtari, GoruntulemeAdi, AyarDegeri, VarsayilanDeger, VeriTipi, Aciklama, SistemAyari)
VALUES ('izin_verilen_dosya_turleri', 'İzin Verilen Dosya Türleri', '.pdf,.doc,.docx,.xls,.xlsx,.jpg,.png', '.pdf,.doc,.docx,.xls,.xlsx,.jpg,.png', 'string', 'Yüklenebilecek dosya uzantıları (virgülle ayrılmış)', 1);

IF NOT EXISTS (SELECT * FROM {TableNames.Ayarlar} WHERE AyarAnahtari = 'otomatik_onay_etkin')
INSERT INTO {TableNames.Ayarlar} (AyarAnahtari, GoruntulemeAdi, AyarDegeri, VarsayilanDeger, VeriTipi, Aciklama, SistemAyari)
VALUES ('otomatik_onay_etkin', 'Otomatik Onay Etkin', '0', '0', 'bit', 'Belirli koşullarda otomatik onay yapılması', 1);";
        }

        #endregion

        #region Table Creation Order

        public static readonly string[] TableCreationOrder = {
            TableNames.Firmalar,
            TableNames.Kullanicilar,
            TableNames.Belgeler,
            TableNames.BelgeLog,
            TableNames.Ayarlar
        };

        #endregion

        #region Table Existence Check

        public static string GetTableExistsScript(string tableName) =>
            $"SELECT COUNT(*) FROM sysobjects WHERE name='{tableName}' AND xtype='U'";

        #endregion

        #region Column Check Scripts

        public static string GetColumnExistsScript(string tableName, string columnName) =>
            $"SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{tableName}' AND COLUMN_NAME = '{columnName}'";

        #endregion

        #region Migration Scripts

        public static class MigrationScripts
        {
            public static string AddBelgeTuruToBelgeler => $@"
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{TableNames.Belgeler}' AND COLUMN_NAME = 'BelgeTuru')
BEGIN
    ALTER TABLE {TableNames.Belgeler} ADD BelgeTuru NVARCHAR(100) NULL;
    PRINT 'BelgeTuru kolonu eklendi';
END
ELSE
BEGIN
    PRINT 'BelgeTuru kolonu zaten mevcut';
END";
        }

        #endregion

        #region Log Actions

        public static class LogActions
        {
            public const string BelgeYuklendi = "BelgeYuklendi";
            public const string BelgeGuncellendi = "BelgeGuncellendi";
            public const string BelgeSilindi = "BelgeSilindi";
            public const string BelgeOnaylandi = "BelgeOnaylandi";
            public const string BelgeReddedildi = "BelgeReddedildi";
            public const string BelgeIptalEdildi = "BelgeIptalEdildi";
            public const string KullaniciGirisi = "KullaniciGirisi";
            public const string KullaniciCikisi = "KullaniciCikisi";
            public const string KullaniciOlusturuldu = "KullaniciOlusturuldu";
            public const string KullaniciGuncellendi = "KullaniciGuncellendi";
            public const string KullaniciSilindi = "KullaniciSilindi";
            public const string FirmaOlusturuldu = "FirmaOlusturuldu";
            public const string FirmaGuncellendi = "FirmaGuncellendi";
            public const string FirmaSilindi = "FirmaSilindi";
            public const string VeritabaniKontrolEdildi = "VeritabaniKontrolEdildi";
            public const string VeritabaniOlusturuldu = "VeritabaniOlusturuldu";
        }

        #endregion
    }
}
