# Blog
Bu bir blog yönetim sistemi projesidir.

Bu sisteme üye olan kullanıcılar istedikleri sayıda blog oluşturabilir, blog'larına istedikleri sayıda yazı girebilirler.

Blog yazıları Markdown olarak düzenlenir ve HTML olarak gösterilir.

## Özellikler

### Blog ve İçerik Yönetimi
- Çoklu blog oluşturma - kullanıcılar birden fazla blog sahibi olabilir
- Markdown destekli yazı editörü ile kolay içerik oluşturma
- SEO dostu URL'ler (slug sistemi)
- Blog görüntüleme sayacı
- Responsive tasarım - mobil ve desktop uyumlu

### Gelişmiş Yorum Sistemi
- **Kayıtlı kullanıcılar**: Yorum yazma, düzenleme, silme
- **Misafir kullanıcılar**: Ad ve email ile yorum yazabilme
- **Cevaplı yorumlar**: Thread yapısında tartışma imkanı
- **Blog sahipleri**: Tüm yorumları yönetebilme, yorumları açma/kapatma


### Kullanıcı Sistemi
- Güvenli kullanıcı kaydı ve girişi
- BCrypt ile şifre hash'leme
- Session tabanlı kimlik doğrulama
- Kullanıcı bazlı yetkilendirme


## Geliştirme
Projeyi çalıştırmak için gerekli bilgiler bu bölümde listelenmiştir.

### Gereksinimler
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download) - Backend framework
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) - PostgreSQL container için
- Git - Kaynak kod yönetimi

### Çalıştırma
1. **Projeyi bilgisayarınıza indirin**
   ```bash
   git clone <repository-url>
   cd Blog
   ```

2. **PostgreSQL veritabanını başlatın**
   ```bash
   docker-compose up -d
   ```
   Bu komut PostgreSQL'i Docker container içinde başlatır.

3. **Proje bağımlılıklarını yükleyin**
   ```bash
   cd BlogProject
   dotnet restore
   ```

4. **Veritabanı tablolarını oluşturun**
   ```bash
   dotnet ef database update
   ```

5. **Uygulamayı çalıştırın**
   ```bash
   dotnet run
   ```

6.**Tarayıcınızda açın**
   `dotnet run` komutundan sonra konsol çıktısında gösterilen adrese gidin
   (örnek: https://localhost:5263)

**Veritabanı Bilgileri:**
- Host: localhost:5432
- Database: blogdb  
- Username: bloguser
- Password: blogpassword123

### Testleri Çalıştırma
Şu anda projede otomatik testler bulunmamaktadır. Manuel test için:
1. Yeni kullanıcı hesabı oluşturun
2. Blog oluşturun ve yazı ekleyin
3. Yorum sistemi fonksiyonlarını test edin
4. Farklı kullanıcı rolleri ile yetkilendirmeleri kontrol edin

## Projeyi Yayımlama
> Buraya projeyi internete yayımlamak için gerekli yönergeler girilecek.
> Yayımlama işi daha sonraki bir aşama olacağı için şimdilik bu alan bu şekilde bırakılabilir.

## Katkı Sağlama
Projeye katkıda bulunmak için lütfen [CONTRIBUTING.md](./CONTRIBUTING.md) dosyasındaki yönergeleri inceleyiniz.
